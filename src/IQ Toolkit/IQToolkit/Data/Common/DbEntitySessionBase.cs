// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.IO;

namespace IQToolkit.Data.Common
{
    using System.Data.Common;

    public abstract class DbEntitySessionBase : IEntitySession
    {
        SessionProvider sessionProvider;
        Dictionary<MappingEntity, ISessionTable> tables;

        public DbEntitySessionBase(DbEntityProviderBase provider)
        {
            this.sessionProvider = new SessionProvider(this, provider);
            this.tables = new Dictionary<MappingEntity, ISessionTable>();
        }

        public abstract void SubmitChanges();

        public DbEntityProviderBase Provider
        {
            get { return this.sessionProvider.UnderlyingProvider; }
        }

        IEntityProvider IEntitySession.Provider
        {
            get { return this.Provider; }
        }

        protected IEnumerable<ISessionTable> GetTables()
        {
            return this.tables.Values;
        }

        public ISessionTable GetTable(Type elementType, string tableId)
        {
            return this.GetTable(this.sessionProvider.Mapping.GetEntity(elementType, tableId));
        }

        public ISessionTable<T> GetTable<T>(string tableId)
        {
            return (ISessionTable<T>)this.GetTable(typeof(T), tableId);
        }

        protected virtual ISessionTable GetTable(MappingEntity entity)
        {
            ISessionTable table;
            if (!this.tables.TryGetValue(entity, out table))
            {
                table = this.CreateTable(entity);
                this.tables.Add(entity, table);
            }
            return table;
        }

        protected abstract ISessionTable CreateTable(MappingEntity entity);

        private object OnEntityMaterialized(MappingEntity entity, object instance)
        {
            IEntitySessionTable table = (IEntitySessionTable)this.GetTable(entity);
            return table.OnEntityMaterialized(instance);
        }

        protected interface IEntitySessionTable : ISessionTable
        {
            object OnEntityMaterialized(object instance);
            MappingEntity Entity { get; }
        }

        protected abstract class SessionTable<T> : Query<T>, ISessionTable<T>, ISessionTable, IEntitySessionTable
        {
            DbEntitySessionBase session;
            MappingEntity entity;
            IEntityTable<T> underlyingTable;

            public SessionTable(DbEntitySessionBase session, MappingEntity entity)
                : base(session.sessionProvider)
            {
                this.session = session;
                this.entity = entity;
                this.underlyingTable = (IEntityTable<T>)session.Provider.GetTable(entity);
            }

            public IEntitySession Session
            {
                get { return this.session; }
            }

            public MappingEntity Entity 
            {
                get { return this.entity; }
            }

            public IEntityTable<T> ProviderTable
            {
                get { return this.underlyingTable; }
            }

            IEntityTable ISessionTable.ProviderTable
            {
                get { return this.underlyingTable; }
            }

            public T GetById(object id)
            {
                return this.underlyingTable.GetById(id);
            }

            object ISessionTable.GetById(object id)
            {
                return this.GetById(id);
            }

            public virtual object OnEntityMaterialized(object instance)
            {
                return instance;
            }

            public virtual void SetSubmitAction(T instance, SubmitAction action)
            {
                throw new NotImplementedException();
            }

            void ISessionTable.SetSubmitAction(object instance, SubmitAction action)
            {
                this.SetSubmitAction((T)instance, action);
            }

            public virtual SubmitAction GetSubmitAction(T instance)
            {
                throw new NotImplementedException();
            }

            SubmitAction ISessionTable.GetSubmitAction(object instance)
            {
                return this.GetSubmitAction((T)instance);
            }
        }

        class SessionProvider : DbEntityProviderBase
        {
            DbEntitySessionBase session;
            DbEntityProviderBase provider;

            public SessionProvider(DbEntitySessionBase session, DbEntityProviderBase provider)
            {
                this.session = session;
                this.provider = provider;
            }

            public DbEntityProviderBase UnderlyingProvider
            {
                get { return this.provider; }
            }

            public override DbConnection Connection
            {
                get { return this.provider.Connection; }
            }

            public override DbTransaction Transaction
            {
                get { return this.provider.Transaction; }
                set { this.provider.Transaction = value; }
            }

            public override QueryMapping Mapping
            {
                get { return this.provider.Mapping; }
            }

            public override QueryLanguage Language
            {
                get { return this.provider.Language; }
            }

            public override QueryPolicy Policy
            {
                get { return this.provider.Policy; }
            }

            public override TextWriter Log
            {
                get { return this.provider.Log; }
                set { this.provider.Log = value; }
            }

            public override int RowsAffected
            {
                get { return this.provider.RowsAffected; }
            }

            public override object Convert(object value, Type type)
            {
                return this.provider.Convert(value, type);
            }

            public override IEntityTable GetTable(MappingEntity entity)
            {
                return this.provider.GetTable(entity);
            }

            public override IEnumerable<T> Execute<T>(QueryCommand command, Func<DbDataReader, T> fnProjector, MappingEntity entity, object[] paramValues)
            {
                return this.provider.Execute<T>(command, Wrap(fnProjector, entity), entity, paramValues);
            }

            public override IEnumerable<int> ExecuteBatch(QueryCommand query, IEnumerable<object[]> paramSets, int batchSize, bool stream)
            {
                return this.provider.ExecuteBatch(query, paramSets, batchSize, stream);
            }

            public override IEnumerable<T> ExecuteBatch<T>(QueryCommand query, IEnumerable<object[]> paramSets, Func<DbDataReader, T> fnProjector, MappingEntity entity, int batchSize, bool stream)
            {
                return this.provider.ExecuteBatch<T>(query, paramSets, Wrap(fnProjector, entity), entity, batchSize, stream);
            }

            public override IEnumerable<T> ExecuteDeferred<T>(QueryCommand query, Func<DbDataReader, T> fnProjector, MappingEntity entity, object[] paramValues)
            {
                return this.provider.ExecuteDeferred<T>(query, Wrap(fnProjector, entity), entity, paramValues);
            }

            public override int ExecuteCommand(QueryCommand query, object[] paramValues)
            {
                return this.provider.ExecuteCommand(query, paramValues);
            }

            public override int ExecuteCommand(string commandText)
            {
                return this.provider.ExecuteCommand(commandText);
            }

            private Func<DbDataReader, T> Wrap<T>(Func<DbDataReader, T> fnProjector, MappingEntity entity)
            {
                Func<DbDataReader, T> fnWrapped = dr => (T)this.session.OnEntityMaterialized(entity, fnProjector(dr));
                return fnWrapped;
            }

            public override string GetQueryText(Expression expression)
            {
                return this.provider.GetQueryText(expression);
            }

            public override object Execute(Expression expression)
            {
                return this.provider.Execute(expression);
            }
        }
    }
}