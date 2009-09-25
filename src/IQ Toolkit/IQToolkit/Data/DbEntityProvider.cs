// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IQToolkit.Data
{
    using Common;
    using Mapping;

    /// <summary>
    /// A LINQ IQueryable query provider that executes database queries over a DbConnection
    /// </summary>
    public class DbEntityProvider : DbEntityProviderBase
    {
        DbConnection connection;
        DbTransaction transaction;
        QueryMapping mapping;
        QueryLanguage language;
        QueryPolicy policy;
        TextWriter log;
        int rowsAffected;
        int nConnectedActions = 0;
        bool actionOpenedConnection = false;
        Dictionary<MappingEntity, IEntityTable> tables;

        public DbEntityProvider(DbConnection connection, QueryMapping mapping)
            : this(connection, mapping, QueryPolicy.Default)
        {
        }

        public DbEntityProvider(DbConnection connection, QueryMapping mapping, QueryPolicy policy)
        {
            if (connection == null)
                throw new InvalidOperationException("Connection not specified");
            if (mapping == null)
                throw new InvalidOperationException("Mapping not specified");
            this.connection = connection;
            this.mapping = mapping;
            this.language = mapping.Language;
            this.policy = policy;
            this.tables = new Dictionary<MappingEntity, IEntityTable>();
        }

        public override DbConnection Connection
        {
            get { return this.connection; }
        }

        public override DbTransaction Transaction
        {
            get { return this.transaction; }
            set
            {
                if (value != null && value.Connection != this.connection)
                    throw new InvalidOperationException("Transaction does not match connection.");
                this.transaction = value;
            }
        }

        public override QueryMapping Mapping
        {
            get { return this.mapping; }
        }

        public override QueryLanguage Language
        {
            get { return this.language; }
        }

        public override QueryPolicy Policy
        {
            get { return this.policy; }
        }

        public override TextWriter Log
        {
            get { return this.log; }
            set { this.log = value; }
        }

        public override int RowsAffected
        {
            get { return this.rowsAffected; }
        }

        protected bool ActionOpenedConnection
        {
            get { return this.actionOpenedConnection; }
        }

        protected virtual bool BufferResultRows
        {
            get { return false; }
        }

        public override IEntityTable GetTable(MappingEntity entity)
        {
            IEntityTable table;
            if (!this.tables.TryGetValue(entity, out table))
            {
                table = this.CreateTable(entity);
                this.tables.Add(entity, table);
            }
            return table;
        }

        protected virtual IEntityTable CreateTable(MappingEntity entity)
        {
            return (IEntityTable) Activator.CreateInstance(
                typeof(DbBasicEntityTable<>).MakeGenericType(entity.ElementType), 
                new object[] { this, entity }
                );
        }

        public class DbBasicEntityTable<T> : Query<T>, IEntityTable<T>, IHaveMappingEntity
        {
            MappingEntity entity;

            public DbBasicEntityTable(DbEntityProvider provider, MappingEntity entity)
                : base(provider)
            {
                this.entity = entity;
            }

            public MappingEntity Entity
            {
                get { return this.entity; }
            }

            new public IEntityProvider Provider
            {
                get { return (IEntityProvider)base.Provider; }
            }

            public string TableId
            {
                get { return this.entity.TableId; }
            }

            public Type EntityType
            {
                get { return this.entity.EntityType; }
            }

            public T GetById(object id)
            {
                var dbProvider = (DbEntityProviderBase)this.Provider;
                if (dbProvider != null)
                {
                    IEnumerable<object> keys = id as IEnumerable<object>;
                    if (keys == null)
                        keys = new object[] { id };
                    Expression query = dbProvider.Mapping.GetPrimaryKeyQuery(this.entity, this.Expression, keys.Select(v => Expression.Constant(v)).ToArray());
                    return this.Provider.Execute<T>(query);
                }
                return default(T);
            }

            object IEntityTable.GetById(object id)
            {
                return this.GetById(id);
            }

            public int Insert(T instance)
            {
                return Updatable.Insert(this, instance);
            }

            int IEntityTable.Insert(object instance)
            {
                return this.Insert((T)instance);
            }

            public int Delete(T instance)
            {
                return Updatable.Delete(this, instance);
            }

            int IEntityTable.Delete(object instance)
            {
                return this.Delete((T)instance);
            }

            public int Update(T instance)
            {
                return Updatable.Update(this, instance);
            }

            int IEntityTable.Update(object instance)
            {
                return this.Update((T)instance);
            }

            public int InsertOrUpdate(T instance)
            {
                return Updatable.InsertOrUpdate(this, instance);
            }

            int IEntityTable.InsertOrUpdate(object instance)
            {
                return this.InsertOrUpdate((T)instance);
            }
        }

        /// <summary>
        /// Converts the query expression into text that corresponds to the command that would be executed.
        /// Useful for debugging.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override string GetQueryText(Expression expression)
        {
            Expression plan = this.GetExecutionPlan(expression);
            var commands = CommandGatherer.Gather(plan).Select(c => c.CommandText).ToArray();
            return string.Join("\n\n", commands);
        }

        class CommandGatherer : DbExpressionVisitor
        {
            List<QueryCommand> commands = new List<QueryCommand>();

            public static ReadOnlyCollection<QueryCommand> Gather(Expression expression)
            {
                var gatherer = new CommandGatherer();
                gatherer.Visit(expression);
                return gatherer.commands.AsReadOnly();
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {
                QueryCommand qc = c.Value as QueryCommand;
                if (qc != null)
                {
                    this.commands.Add(qc);
                }
                return c;
            }
        }

        public string GetQueryPlan(Expression expression)
        {
            Expression plan = this.GetExecutionPlan(expression);
            return DbExpressionWriter.WriteToString(this.Language, plan);
        }

        /// <summary>
        /// Convert a value into the specified type.  This method is called during object materialization to convert
        /// values retrieved from an ADO DataReader into the appropriate member type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override object Convert(object value, Type type)
        {
            if (value == null)
            {
                return TypeHelper.GetDefault(type);
            }
            type = TypeHelper.GetNonNullableType(type);
            Type vtype = value.GetType();
            if (type != vtype)
            {
                if (type.IsEnum)
                {
                    if (vtype == typeof(string))
                    {
                        return Enum.Parse(type, (string)value);
                    }
                    else
                    {
                        Type utype = Enum.GetUnderlyingType(type);
                        if (utype != vtype)
                        {
                            value = System.Convert.ChangeType(value, utype);
                        }
                        return Enum.ToObject(type, value);
                    }
                }
                return System.Convert.ChangeType(value, type);
            }
            return value;
        }


        /// <summary>
        /// Execute the query expression (does translation, etc.)
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override object Execute(Expression expression)
        {
            Expression plan = this.GetExecutionPlan(expression);

            LambdaExpression lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                // compile & return the execution plan so it can be used multiple times
                LambdaExpression fn = Expression.Lambda(lambda.Type, plan, lambda.Parameters);
                return fn.Compile();
            }
            else
            {
                // compile the execution plan and invoke it
                Expression<Func<object>> efn = Expression.Lambda<Func<object>>(Expression.Convert(plan, typeof(object)));
                Func<object> fn = efn.Compile();
                return fn();
            }
        }

        /// <summary>
        /// Convert the query expression into an execution plan
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression GetExecutionPlan(Expression expression)
        {
            // strip off lambda for now
            LambdaExpression lambda = expression as LambdaExpression;
            if (lambda != null)
                expression = lambda.Body;

            // translate query into client & server parts
            Expression translation = this.Translate(expression);

            Expression provider = TypedSubtreeFinder.Find(expression, typeof(DbEntityProviderBase));
            if (provider == null)
            {
                Expression rootQueryable = TypedSubtreeFinder.Find(expression, typeof(IQueryable));
                provider = Expression.Convert(
                    Expression.Property(rootQueryable, typeof(IQueryable).GetProperty("Provider")),
                    typeof(DbEntityProviderBase)
                    );
            }

            return this.policy.BuildExecutionPlan(this.Mapping, translation, provider);
        }

        /// <summary>
        /// Do all query translations execpt building the execution plan
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal protected virtual Expression Translate(Expression expression)
        {
            // pre-evaluate local sub-trees
            expression = PartialEvaluator.Eval(expression, this.CanBeEvaluatedLocally);

            // convert references to LINQ operators into query specific nodes
            expression = QueryBinder.Bind(this.mapping, expression, this.CanBeEvaluatedLocally);

            // apply mapping (binds LINQ operators too)
            expression = this.mapping.Translate(expression);

            // any policy specific translations or validations
            expression = this.policy.Translate(this.Mapping, expression);

            // any language specific translations or validations
            expression = this.language.Translate(expression);

            return expression;
        }

        /// <summary>
        /// Determines whether a given expression can be executed locally. 
        /// (It contains no parts that should be translated to the target environment.)
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual bool CanBeEvaluatedLocally(Expression expression)
        {
            // any operation on a query can't be done locally
            ConstantExpression cex = expression as ConstantExpression;
            if (cex != null)
            {
                IQueryable query = cex.Value as IQueryable;
                if (query != null && query.Provider == this)
                    return false;
            }
            MethodCallExpression mc = expression as MethodCallExpression;
            if (mc != null &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable) ||
                 mc.Method.DeclaringType == typeof(Updatable))
                 )
            {
                return false;
            }
            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
                return true;
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

        protected virtual void StartUsingConnection()
        {
            if (this.connection.State == ConnectionState.Closed)
            {
                this.connection.Open();
                this.actionOpenedConnection = true;
            }
            this.nConnectedActions++;
        }

        protected virtual void StopUsingConnection()
        {
            System.Diagnostics.Debug.Assert(this.nConnectedActions > 0);
            this.nConnectedActions--;
            if (this.nConnectedActions == 0 && this.actionOpenedConnection)
            {
                this.connection.Close();
                this.actionOpenedConnection = false;
            }
        }

        /// <summary>
        /// Execute an actual query specified in the target language using the sADO connection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public override IEnumerable<T> Execute<T>(QueryCommand command, Func<DbDataReader, T> fnProjector, MappingEntity entity, object[] paramValues)
        {
            this.LogCommand(command, paramValues);
            this.StartUsingConnection();
            try
            {
                DbCommand cmd = this.GetCommand(command, paramValues);
                DbDataReader reader = this.ExecuteReader(cmd);
                var result = Project(reader, fnProjector, entity, true);
                if (this.ActionOpenedConnection)
                {
                    result = result.ToList();
                }
                else
                {
                    result = new EnumerateOnce<T>(result);
                }
                return result;
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        protected virtual DbDataReader ExecuteReader(DbCommand command)
        {
            var reader = command.ExecuteReader();
            if (this.BufferResultRows)
            {
                // use data table to buffer results
                var ds = new DataSet();
                ds.EnforceConstraints = false;
                var table = new DataTable();
                ds.Tables.Add(table);
                ds.EnforceConstraints = false;
                table.Load(reader);
                reader = table.CreateDataReader();
            }
            return reader;
        }

        /// <summary>
        /// Converts a data reader into a sequence of objects using a projector function on each row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="fnProject"></param>
        /// <returns></returns>
        protected virtual IEnumerable<T> Project<T>(DbDataReader reader, Func<DbDataReader, T> fnProjector, MappingEntity entity, bool closeReader)
        {
            try
            {
                while (reader.Read())
                {
                    yield return fnProjector(reader);
                }
            }
            finally
            {
                if (closeReader)
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// Execute an actual query that does not return mapped results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public override int ExecuteCommand(QueryCommand query, object[] paramValues)
        {
            this.LogCommand(query, paramValues);
            this.StartUsingConnection();
            try
            {
                DbCommand cmd = this.GetCommand(query, paramValues);
                this.rowsAffected = cmd.ExecuteNonQuery();
                return this.rowsAffected;
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        public override IEnumerable<int> ExecuteBatch(QueryCommand query, IEnumerable<object[]> paramSets, int batchSize, bool stream)
        {
            this.StartUsingConnection();
            try
            {
                var result = this.ExecuteBatch(query, paramSets);
                if (!stream || actionOpenedConnection)
                {
                    return result.ToList();
                }
                else
                {
                    return new EnumerateOnce<int>(result);
                }
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        private IEnumerable<int> ExecuteBatch(QueryCommand query, IEnumerable<object[]> paramSets)
        {
            this.LogCommand(query, null);
            DbCommand cmd = this.GetCommand(query, null);
            foreach (var paramValues in paramSets)
            {
                this.LogParameters(query, paramValues);
                this.LogMessage("");
                this.SetParameterValues(query, cmd, paramValues);
                this.rowsAffected = cmd.ExecuteNonQuery();
                yield return this.rowsAffected;
            }
        }

        public override IEnumerable<T> ExecuteBatch<T>(QueryCommand query, IEnumerable<object[]> paramSets, Func<DbDataReader, T> fnProjector, MappingEntity entity, int batchSize, bool stream)
        {
            this.StartUsingConnection();
            try
            {
                var result = this.ExecuteBatch(query, paramSets, fnProjector, entity);
                if (!stream || actionOpenedConnection)
                {
                    return result.ToList();
                }
                else
                {
                    return new EnumerateOnce<T>(result);
                }
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        private IEnumerable<T> ExecuteBatch<T>(QueryCommand query, IEnumerable<object[]> paramSets, Func<DbDataReader, T> fnProjector, MappingEntity entity)
        {
            this.LogCommand(query, null);
            DbCommand cmd = this.GetCommand(query, null);
            cmd.Prepare();
            foreach (var paramValues in paramSets)
            {
                this.LogParameters(query, paramValues);
                this.LogMessage("");
                this.SetParameterValues(query, cmd, paramValues);
                var reader = this.ExecuteReader(cmd);
                try
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        yield return fnProjector(reader);
                    }
                    else
                    {
                        yield return default(T);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// Get an IEnumerable that will execute the specified query when enumerated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"><  /param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public override IEnumerable<T> ExecuteDeferred<T>(QueryCommand query, Func<DbDataReader, T> fnProjector, MappingEntity entity, object[] paramValues)
        {
            this.LogCommand(query, paramValues);
            this.StartUsingConnection();
            try
            {
                DbCommand cmd = this.GetCommand(query, paramValues);
                DbDataReader reader = this.ExecuteReader(cmd);
                try
                {
                    while (reader.Read())
                    {
                        yield return fnProjector(reader);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        /// <summary>
        /// Get an ADO command object initialized with the command-text and parameters
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="paramNames"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        protected virtual DbCommand GetCommand(QueryCommand query, object[] paramValues)
        {
            // create command object (and fill in parameters)
            DbCommand cmd = this.connection.CreateCommand();
            cmd.CommandText = query.CommandText;
            if (this.Transaction != null)
                cmd.Transaction = this.Transaction;
            this.SetParameterValues(query, cmd, paramValues);
            return cmd;
        }

        protected virtual void SetParameterValues(QueryCommand query, DbCommand command, object[] paramValues)
        {
            if (query.Parameters.Count > 0 && command.Parameters.Count == 0)
            {
                for (int i = 0, n = query.Parameters.Count; i < n; i++)
                {
                    this.AddParameter(command, query.Parameters[i], paramValues != null ? paramValues[i] : null);
                }
            }
            else if (paramValues != null)
            {
                for (int i = 0, n = command.Parameters.Count; i < n; i++)
                {
                    DbParameter p = command.Parameters[i];
                    if (p.Direction == System.Data.ParameterDirection.Input
                     || p.Direction == System.Data.ParameterDirection.InputOutput)
                    {
                        p.Value = paramValues[i] ?? DBNull.Value;
                    }
                }
            }
        }

        protected virtual void AddParameter(DbCommand command, QueryParameter parameter, object value)
        {
            DbParameter p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }

        protected virtual void GetParameterValues(DbCommand command, object[] paramValues)
        {
            if (paramValues != null)
            {
                for (int i = 0, n = command.Parameters.Count; i < n; i++)
                {
                    if (command.Parameters[i].Direction != System.Data.ParameterDirection.Input)
                    {
                        object value = command.Parameters[i].Value;
                        if (value == DBNull.Value)
                            value = null;
                        paramValues[i] = value;
                    }
                }
            }
        }

        protected virtual void LogMessage(string message)
        {
            if (this.log != null)
            {
                this.log.WriteLine(message);
            }
        }

        /// <summary>
        /// Write a command & parameters to the log
        /// </summary>
        /// <param name="command"></param>
        /// <param name="paramValues"></param>
        protected virtual void LogCommand(QueryCommand command, object[] paramValues)
        {
            if (this.log != null)
            {
                this.log.WriteLine(command.CommandText);
                if (paramValues != null)
                {
                    this.LogParameters(command, paramValues);
                }
                this.log.WriteLine();
            }
        }

        protected virtual void LogParameters(QueryCommand command, object[] paramValues)
        {
            if (this.log != null && paramValues != null)
            {
                for (int i = 0, n = command.Parameters.Count; i < n; i++)
                {
                    var p = command.Parameters[i];
                    var v = paramValues[i];

                    if (v == null || v == DBNull.Value)
                    {
                        this.log.WriteLine("-- {0} = NULL", p.Name);
                    }
                    else
                    {
                        this.log.WriteLine("-- {0} = [{1}]", p.Name, v);
                    }
                }
            }
        }

        public override int ExecuteCommand(string commandText)
        {
            if (this.log != null)
            {
                this.log.WriteLine(commandText);
            }
            this.StartUsingConnection();
            try
            {
                DbCommand cmd = this.connection.CreateCommand();
                cmd.CommandText = commandText;
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        public virtual DbEntityProvider New(DbConnection connection, QueryMapping mapping, QueryPolicy policy)
        {
            return (DbEntityProvider)Activator.CreateInstance(this.GetType(), new object[] { connection, mapping, policy });
        }

        public virtual DbEntityProviderBase New(DbConnection connection)
        {
            var n = New(connection, this.Mapping, this.Policy);
            n.Log = this.Log;
            return n;
        }

        public virtual DbEntityProviderBase New(QueryMapping mapping)
        {
            var n = New(this.Connection, mapping, this.Policy);
            n.Log = this.Log;
            return n;
        }

        public virtual DbEntityProviderBase New(QueryPolicy policy)
        {
            var n = New(this.Connection, this.Mapping, policy);
            n.Log = this.Log;
            return n;
        }

        public static DbEntityProvider FromApplicationSettings()
        {
            var provider = System.Configuration.ConfigurationSettings.AppSettings["Provider"];
            var connection = System.Configuration.ConfigurationSettings.AppSettings["Connection"];
            var mapping = System.Configuration.ConfigurationSettings.AppSettings["Mapping"];
            return From(provider, connection, mapping);
        }

        public static DbEntityProvider From(string connectionString, string mappingId)
        {
            return From(null, connectionString, mappingId);
        }

        public static DbEntityProvider From(string provider, string connectionString, string mappingId)
        {
            if (provider == null)
            {
                var clower = connectionString.ToLower();
                // try sniffing connection to figure out provider
                if (clower.Contains(".mdb") || clower.Contains(".accdb"))
                {
                    provider = "IQToolkit.Data.Access";
                }
                else if (clower.Contains(".sdf"))
                {
                    provider = "IQToolkit.Data.SqlServerCe";
                }
                else if (clower.Contains(".sl3") || clower.Contains(".db3"))
                {
                    provider = "IQToolkit.Data.SQLite";
                }
                else if (clower.Contains(".mdf"))
                {
                    provider = "IQToolkit.Data.SqlClient";
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Query sessionProvider not specified and cannot be inferred."));
                }
            }

            Type providerType = GetProviderType(provider);
            if (providerType == null)
                throw new InvalidOperationException(string.Format("Unable to find query sessionProvider '{0}'", provider));

            Type adoConnectionType = GetAdoConnectionType(providerType);
            if (adoConnectionType == null)
                throw new InvalidOperationException(string.Format("Unable to deduce ADO sessionProvider for '{0}'", providerType.Name));
            DbConnection connection = (DbConnection)Activator.CreateInstance(adoConnectionType);

            // is the connection string just a filename?
            if (!connectionString.Contains('='))
            {
                MethodInfo gcs = providerType.GetMethod("GetConnectionString", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
                if (gcs != null)
                {
                    connectionString = (string)gcs.Invoke(null, new object[] { connectionString });
                }
            }

            connection.ConnectionString = connectionString;

            Type languageType = FindInstancesIn(typeof(QueryLanguage), providerType.Namespace).FirstOrDefault();
            if (languageType == null)
                throw new InvalidOperationException(string.Format("Unabled to determine correct QueryLanguage for '{0}'", providerType.Name));
            var language = (QueryLanguage)Activator.CreateInstance(languageType);
            var mapping = GetMapping(mappingId, language);

            return (DbEntityProvider)Activator.CreateInstance(providerType, new object[] { connection, mapping, QueryPolicy.Default });
        }

        private static Type GetAdoConnectionType(Type providerType)
        {
            if (providerType.IsSubclassOf(typeof(DbEntityProvider)))
            {
                // sniff constructors 
                foreach (var con in providerType.GetConstructors())
                {
                    foreach (var arg in con.GetParameters())
                    {
                        if (arg.ParameterType.IsSubclassOf(typeof(DbConnection)))
                            return arg.ParameterType;
                    }
                }
            }
            return null;
        }

        private static Type GetProviderType(string providerName)
        {
            if (!string.IsNullOrEmpty(providerName))
            {
                var type = FindInstancesIn(typeof(DbEntityProvider), providerName).FirstOrDefault();
                if (type != null)
                    return type;
            }
            return null;
        }

        private static QueryMapping GetMapping(string mappingId, QueryLanguage language)
        {
            if (mappingId != null)
            {
                Type type = FindLoadedType(mappingId);
                if (type != null)
                {
                    return new AttributeMapping(language, type);
                }
                if (File.Exists(mappingId))
                {
                    return XmlMapping.FromXml(language, File.ReadAllText(mappingId));
                }
            }
            return new ImplicitMapping(language);
        }

        private static Type FindLoadedType(string typeName)
        {
            foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assem.GetType(typeName, false, true);
                if (type != null)
                    return type;
            }
            return null;
        }

        private static IEnumerable<Type> FindInstancesIn(Type type, string assemblyName)
        {
            Assembly assembly = GetAssemblyForNamespace(assemblyName);
            if (assembly != null)
            {
                foreach (var atype in assembly.GetTypes())
                {
                    if (string.Compare(atype.Namespace, assemblyName, 0) == 0
                        && type.IsAssignableFrom(atype))
                    {
                        yield return atype;
                    }
                }
            }
        }

        private static Assembly GetAssemblyForNamespace(string nspace)
        {
            foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assem.FullName.Contains(nspace))
                {
                    return assem;
                }
            }

            return Load(nspace + ".dll");
        }

        private static Assembly Load(string name)
        {
            // try to load it.
            try
            {
                return Assembly.LoadFrom(name);
            }
            catch
            {
            }
            return null;
        }
    }
}
