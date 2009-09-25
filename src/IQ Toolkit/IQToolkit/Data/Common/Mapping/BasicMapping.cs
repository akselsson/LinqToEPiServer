// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IQToolkit.Data.Common
{
    public abstract class BasicMapping : QueryMapping
    {
        QueryLanguage language;

        protected BasicMapping(QueryLanguage language)
        {
            this.language = language;
        }

        public override QueryLanguage Language
        {
            get { return this.language; }
        }

        public override MappingEntity GetEntity(Type elementType, string tableId)
        {
            if (tableId == null)
                tableId = this.GetTableId(elementType);
            return new BasicMappingEntity(elementType, tableId);
        }

        public override MappingEntity GetEntity(MemberInfo contextMember)
        {
            Type elementType = TypeHelper.GetElementType(TypeHelper.GetMemberType(contextMember));
            return this.GetEntity(elementType);
        }

        class BasicMappingEntity : MappingEntity
        {
            string entityID;
            Type type;

            public BasicMappingEntity(Type type, string entityID)
            {
                this.entityID = entityID;
                this.type = type;
            }

            public override string TableId
            {
                get { return this.entityID; }
            }

            public override Type ElementType
            {
                get { return this.type; }
            }

            public override Type EntityType
            {
                get { return this.type; }
            }
        }

        public override bool IsRelationship(MappingEntity entity, MemberInfo member)
        {
            return this.IsAssociationRelationship(entity, member);
        }

        /// <summary>
        /// Deterimines is a property is mapped onto a column or relationship
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual bool IsMapped(MappingEntity entity, MemberInfo member)
        {
            return true;
        }

        /// <summary>
        /// Determines if a property is mapped onto a column
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual bool IsColumn(MappingEntity entity, MemberInfo member)
        {
            return this.IsMapped(entity, member) && this.Language.IsScalar(TypeHelper.GetMemberType(member));
        }

        /// <summary>
        /// Determines if a property represents or is part of the entities unique identity (often primary key)
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public override bool IsPrimaryKey(MappingEntity entity, MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// Determines if a property is computed after insert or update
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual bool IsComputed(MappingEntity entity, MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// Determines if a property is generated on the server during insert
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual bool IsGenerated(MappingEntity entity, MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// Determines if a property can be part of an update operation
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual bool IsUpdatable(MappingEntity entity, MemberInfo member)
        {
            return !this.IsPrimaryKey(entity, member);
        }

        /// <summary>
        /// The type of the entity on the other side of the relationship
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual MappingEntity GetRelatedEntity(MappingEntity entity, MemberInfo member)
        {
            Type relatedType = TypeHelper.GetElementType(TypeHelper.GetMemberType(member));
            return this.GetEntity(relatedType);
        }

        /// <summary>
        /// Determines if the property is an assocation relationship.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual bool IsAssociationRelationship(MappingEntity entity, MemberInfo member)
        {
            return false;
        }

        /// <summary>
        /// Returns the key members on this side of the association
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual IEnumerable<MemberInfo> GetAssociationKeyMembers(MappingEntity entity, MemberInfo member)
        {            
            return new MemberInfo[] { };
        }

        /// <summary>
        /// Returns the key members on the other side (related side) of the association
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual IEnumerable<MemberInfo> GetAssociationRelatedKeyMembers(MappingEntity entity, MemberInfo member)
        {
            return new MemberInfo[] { };
        }

        /// <summary>
        /// The name of the corresponding database table
        /// </summary>
        /// <param name="rowType"></param>
        /// <returns></returns>
        protected virtual string GetTableName(MappingEntity entity)
        {
            return entity.EntityType.Name;
        }

        /// <summary>
        /// The name of the corresponding table column
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual string GetColumnName(MappingEntity entity, MemberInfo member)
        {
            return member.Name;
        }

        /// <summary>
        /// The query language specific type for the column
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        protected virtual QueryType GetColumnType(MappingEntity entity, MemberInfo member)
        {
            return this.Language.TypeSystem.GetColumnType(TypeHelper.GetMemberType(member));
        }

        /// <summary>
        /// A sequence of all the mapped members
        /// </summary>
        /// <param name="rowType"></param>
        /// <returns></returns>
        public override IEnumerable<MemberInfo> GetMappedMembers(MappingEntity entity)
        {
            //Type type = entity.ElementType.IsInterface ? entity.EntityType : entity.ElementType;
            Type type = entity.EntityType;
            HashSet<MemberInfo> members = new HashSet<MemberInfo>(type.GetFields().Cast<MemberInfo>().Where(m => this.IsMapped(entity, m)));
            members.UnionWith(type.GetProperties().Cast<MemberInfo>().Where(m => this.IsMapped(entity, m)));
            return members.OrderBy(m => m.Name);
        }

        public override ProjectionExpression GetQueryExpression(MappingEntity entity)
        {
            var tableAlias = new TableAlias();
            var selectAlias = new TableAlias();
            var table = new TableExpression(tableAlias, entity, this.GetTableName(entity));

            Expression projector = this.GetEntityExpression(table, entity);
            var pc = ColumnProjector.ProjectColumns(this.Language, projector, null, selectAlias, tableAlias);

            return new ProjectionExpression(
                new SelectExpression(selectAlias, pc.Columns, table, null),
                pc.Projector
                );
        }

        public override EntityExpression GetEntityExpression(Expression root, MappingEntity entity)
        {
            // must be some complex type constructed from multiple columns
            var assignments = new List<EntityAssignment>();
            foreach (MemberInfo mi in this.GetMappedMembers(entity))
            {
                if (!this.IsAssociationRelationship(entity, mi))
                {
                    Expression me = this.GetMemberExpression(root, entity, mi);
                    if (me != null)
                    {
                        assignments.Add(new EntityAssignment(mi, me));
                    }
                }
            }

            return new EntityExpression(entity, BuildEntityExpression(entity, assignments));
        }


        public class EntityAssignment
        {
            public MemberInfo Member { get; private set; }
            public Expression Expression { get; private set; }
            public EntityAssignment(MemberInfo member, Expression expression)
            {
                this.Member = member;
                System.Diagnostics.Debug.Assert(expression != null);
                this.Expression = expression;
            }
        }

        protected virtual Expression BuildEntityExpression(MappingEntity entity, IList<EntityAssignment> assignments)
        {
            NewExpression newExpression;

            // handle cases where members are not directly assignable
            EntityAssignment[] readonlyMembers = assignments.Where(b => TypeHelper.IsReadOnly(b.Member)).ToArray();
            ConstructorInfo[] cons = entity.EntityType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            bool hasNoArgConstructor = cons.Any(c => c.GetParameters().Length == 0);

            if (readonlyMembers.Length > 0 || !hasNoArgConstructor)
            {
                // find all the constructors that bind all the read-only members
                var consThatApply = cons.Select(c => this.BindConstructor(c, readonlyMembers))
                                        .Where(cbr => cbr != null && cbr.Remaining.Count == 0).ToList();
                if (consThatApply.Count == 0)
                {
                    throw new InvalidOperationException(string.Format("Cannot construct type '{0}' with all mapped includedMembers.", entity.ElementType));
                }
                // just use the first one... (Note: need better algorithm. :-)
                if (readonlyMembers.Length == assignments.Count)
                {
                    return consThatApply[0].Expression;
                }
                var r = this.BindConstructor(consThatApply[0].Expression.Constructor, assignments);

                newExpression = r.Expression;
                assignments = r.Remaining;
            }
            else
            {
                newExpression = Expression.New(entity.EntityType);
            }

            Expression result;
            if (assignments.Count > 0)
            {
                if (entity.ElementType.IsInterface)
                {
                    assignments = this.MapAssignments(assignments, entity.EntityType).ToList();
                }
                result = Expression.MemberInit(newExpression, (MemberBinding[])assignments.Select(a => Expression.Bind(a.Member, a.Expression)).ToArray());
            }
            else
            {
                result = newExpression;
            }

            if (entity.ElementType != entity.EntityType)
            {
                result = Expression.Convert(result, entity.ElementType);
            }

            return result;
        }

        private IEnumerable<EntityAssignment> MapAssignments(IEnumerable<EntityAssignment> assignments, Type entityType)
        {
            foreach (var assign in assignments)
            {
                MemberInfo[] members = entityType.GetMember(assign.Member.Name, BindingFlags.Instance|BindingFlags.Public);
                if (members != null && members.Length > 0)
                {
                    yield return new EntityAssignment(members[0], assign.Expression);
                }
                else
                {
                    yield return assign;
                }
            }
        }

        protected virtual ConstructorBindResult BindConstructor(ConstructorInfo cons, IList<EntityAssignment> assignments)
        {
            var ps = cons.GetParameters();
            var args = new Expression[ps.Length];
            var mis = new MemberInfo[ps.Length];
            HashSet<EntityAssignment> members = new HashSet<EntityAssignment>(assignments);
            HashSet<EntityAssignment> used = new HashSet<EntityAssignment>();

            for (int i = 0, n = ps.Length; i < n; i++)
            {
                ParameterInfo p = ps[i];
                var assignment = members.FirstOrDefault(a =>
                    p.Name == a.Member.Name
                    && p.ParameterType.IsAssignableFrom(a.Expression.Type));
                if (assignment == null)
                {
                    assignment = members.FirstOrDefault(a =>
                        string.Compare(p.Name, a.Member.Name, true) == 0
                        && p.ParameterType.IsAssignableFrom(a.Expression.Type));
                }
                if (assignment != null)
                {
                    args[i] = assignment.Expression;
                    if (mis != null)
                        mis[i] = assignment.Member;
                    used.Add(assignment);
                }
                else
                {
                    MemberInfo[] mems = cons.DeclaringType.GetMember(p.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    if (mems != null && mems.Length > 0)
                    {
                        args[i] = Expression.Constant(TypeHelper.GetDefault(p.ParameterType), p.ParameterType);
                        mis[i] = mems[0];
                    }
                    else
                    {
                        // unknown parameter, does not match any member
                        return null;
                    }
                }
            }

            members.ExceptWith(used);

            return new ConstructorBindResult(Expression.New(cons, args, mis), members);
        }

        protected class ConstructorBindResult
        {
            public NewExpression Expression { get; private set; }
            public ReadOnlyCollection<EntityAssignment> Remaining { get; private set; }
            public ConstructorBindResult(NewExpression expression, IEnumerable<EntityAssignment> remaining)
            {
                this.Expression = expression;
                this.Remaining = remaining.ToReadOnly();
            }
        }

        public override EntityExpression IncludeMembers(EntityExpression entity, Func<MemberInfo, bool> fnIsIncluded)
        {
            var assignments = this.GetAssignments(entity.Expression).ToDictionary(ma => ma.Member.Name);
            bool anyAdded = false;
            foreach (var mi in this.GetMappedMembers(entity.Entity))
            {
                EntityAssignment ea;
                if (!assignments.TryGetValue(mi.Name, out ea) && fnIsIncluded(mi))
                {
                    ea = new EntityAssignment(mi, this.GetMemberExpression(entity.Expression, entity.Entity, mi));
                    assignments.Add(mi.Name, ea);
                    anyAdded = true;
                }
            }
            if (anyAdded)
            {
                return new EntityExpression(entity.Entity, this.BuildEntityExpression(entity.Entity, assignments.Values.ToList()));
            }
            return entity;
        }

        private IEnumerable<EntityAssignment> GetAssignments(Expression newOrMemberInit)
        {
            var assignments = new List<EntityAssignment>();
            var minit = newOrMemberInit as MemberInitExpression;
            if (minit != null)
            {
                assignments.AddRange(minit.Bindings.OfType<MemberAssignment>().Select(a => new EntityAssignment(a.Member, a.Expression)));
                newOrMemberInit = minit.NewExpression;
            }
            var nex = newOrMemberInit as NewExpression;
            if (nex != null && nex.Members != null)
            {
                assignments.AddRange(
                    Enumerable.Range(0, nex.Arguments.Count)
                              .Where(i => nex.Members[i] != null)
                              .Select(i => new EntityAssignment(nex.Members[i], nex.Arguments[i]))
                              );
            }
            return assignments;
        }

        public override Expression GetMemberExpression(Expression root, MappingEntity entity, MemberInfo member)
        {
            if (this.IsAssociationRelationship(entity, member))
            {
                MappingEntity relatedEntity = this.GetRelatedEntity(entity, member);
                ProjectionExpression projection = this.GetQueryExpression(relatedEntity);

                // make where clause for joining back to 'root'
                var declaredTypeMembers = this.GetAssociationKeyMembers(entity, member).ToList();
                var associatedMembers = this.GetAssociationRelatedKeyMembers(entity, member).ToList();

                Expression where = null;
                for (int i = 0, n = associatedMembers.Count; i < n; i++)
                {
                    Expression equal = 
                        this.GetMemberExpression(projection.Projector, relatedEntity, associatedMembers[i]).Equal(
                            this.GetMemberExpression(root, entity, declaredTypeMembers[i])
                        );
                    where = (where != null) ? where.And(equal) : equal;
                }

                TableAlias newAlias = new TableAlias();
                var pc = ColumnProjector.ProjectColumns(this.Language, projection.Projector, null, newAlias, projection.Select.Alias);

                LambdaExpression aggregator = Aggregator.GetAggregator(TypeHelper.GetMemberType(member), typeof(IEnumerable<>).MakeGenericType(pc.Projector.Type));
                return new ProjectionExpression(
                    new SelectExpression(newAlias, pc.Columns, projection.Select, where),
                    pc.Projector, aggregator
                    );
            }
            else
            {
                AliasedExpression aliasedRoot = root as AliasedExpression;
                if (aliasedRoot != null && this.IsColumn(entity, member))
                {
                    return new ColumnExpression(TypeHelper.GetMemberType(member), this.GetColumnType(entity, member), aliasedRoot.Alias, this.GetColumnName(entity, member));
                }
                return QueryBinder.BindMember(root, member);
            }
        }

        public override Expression GetInsertExpression(MappingEntity entity, Expression instance, LambdaExpression selector)
        {
            var tableAlias = new TableAlias();
            var table = new TableExpression(tableAlias, entity, this.GetTableName(entity));
            var assignments = this.GetColumnAssignments(table, instance, entity, (e, m) => !this.IsGenerated(e, m));

            if (selector != null)
            {
                return new BlockCommand(
                    new InsertCommand(table, assignments),
                    this.GetInsertResult(entity, instance, selector, null)
                    );
            }

            return new InsertCommand(table, assignments);
        }

        private IEnumerable<ColumnAssignment> GetColumnAssignments(Expression table, Expression instance, MappingEntity entity, Func<MappingEntity, MemberInfo, bool> fnIncludeColumn)
        {
            foreach (var m in this.GetMappedMembers(entity))
            {
                if (this.IsColumn(entity, m) && fnIncludeColumn(entity, m))
                {
                    yield return new ColumnAssignment(
                        (ColumnExpression)this.GetMemberExpression(table, entity, m),
                        Expression.MakeMemberAccess(instance, m)
                        );
                }
            }
        }

        protected virtual Expression GetInsertResult(MappingEntity entity, Expression instance, LambdaExpression selector, Dictionary<MemberInfo, Expression> map)
        {
            var tableAlias = new TableAlias();
            var tex = new TableExpression(tableAlias, entity, this.GetTableName(entity));
            var aggregator = Aggregator.GetAggregator(selector.Body.Type, typeof(IEnumerable<>).MakeGenericType(selector.Body.Type));

            Expression where;
            DeclarationCommand genIdCommand = null;
            var generatedIds = this.GetMappedMembers(entity).Where(m => this.IsPrimaryKey(entity, m) && this.IsGenerated(entity, m)).ToList();
            if (generatedIds.Count > 0)
            {
                if (map == null || !generatedIds.Any(m => map.ContainsKey(m)))
                {
                    var localMap = new Dictionary<MemberInfo, Expression>();
                    genIdCommand = this.GetGeneratedIdCommand(entity, generatedIds.ToList(), localMap);
                    map = localMap;
                }

                // is this just a retrieval of one generated id member?
                var mex = selector.Body as MemberExpression;
                if (mex != null && this.IsPrimaryKey(entity, mex.Member) && this.IsGenerated(entity, mex.Member))
                {
                    if (genIdCommand != null)
                    {
                        // just use the select from the genIdCommand
                        return new ProjectionExpression(
                            genIdCommand.Source,
                            new ColumnExpression(mex.Type, genIdCommand.Variables[0].QueryType, genIdCommand.Source.Alias, genIdCommand.Source.Columns[0].Name),
                            aggregator
                            );
                    }
                    else
                    {
                        TableAlias alias = new TableAlias();
                        return new ProjectionExpression(
                            new SelectExpression(alias, new[] { new ColumnDeclaration("", map[mex.Member]) }, null, null),
                            new ColumnExpression(TypeHelper.GetMemberType(mex.Member), this.GetColumnType(entity, mex.Member), alias, ""),
                            aggregator
                            );
                    }
                }

                where = generatedIds.Select((m, i) =>
                    this.GetMemberExpression(tex, entity, m).Equal(map[m])
                    ).Aggregate((x, y) => x.And(y));
            }
            else
            {
                where = this.GetIdentityCheck(tex, entity, instance);
            }

            Expression typeProjector = this.GetEntityExpression(tex, entity);
            Expression selection = DbExpressionReplacer.Replace(selector.Body, selector.Parameters[0], typeProjector);
            TableAlias newAlias = new TableAlias();
            var pc = ColumnProjector.ProjectColumns(this.Language, selection, null, newAlias, tableAlias);
            var pe = new ProjectionExpression(
                new SelectExpression(newAlias, pc.Columns, tex, where),
                pc.Projector,
                aggregator
                );

            if (genIdCommand != null)
            {
                return new BlockCommand(genIdCommand, pe);
            }
            return pe;
        }

        protected virtual DeclarationCommand GetGeneratedIdCommand(MappingEntity entity, List<MemberInfo> members, Dictionary<MemberInfo, Expression> map)
        {
            var columns = new List<ColumnDeclaration>();
            var decls = new List<VariableDeclaration>();
            var alias = new TableAlias();
            foreach (var member in members)
            {
                Expression genId = this.Language.GetGeneratedIdExpression(member);
                var name = member.Name;
                columns.Add(new ColumnDeclaration(member.Name, genId));
                var colType = this.GetColumnType(entity, member);
                decls.Add(new VariableDeclaration(member.Name, colType, new ColumnExpression(genId.Type, colType, alias, member.Name)));
                if (map != null)
                {
                    var vex = new VariableExpression(member.Name, TypeHelper.GetMemberType(member), colType);
                    map.Add(member, vex);
                }
            }
            var select = new SelectExpression(alias, columns, null, null);
            return new DeclarationCommand(decls, select);
        }

        protected virtual Expression GetIdentityCheck(Expression root, MappingEntity entity, Expression instance)
        {
            return this.GetMappedMembers(entity)
            .Where(m => this.IsPrimaryKey(entity, m))
            .Select(m => this.GetMemberExpression(root, entity, m).Equal(Expression.MakeMemberAccess(instance, m)))
            .Aggregate((x, y) => x.And(y));
        }

        protected virtual Expression GetEntityExistsTest(MappingEntity entity, Expression instance)
        {
            ProjectionExpression tq = this.GetQueryExpression(entity);
            Expression where = this.GetIdentityCheck(tq.Select, entity, instance);
            return new ExistsExpression(new SelectExpression(new TableAlias(), null, tq.Select, where));
        }

        protected virtual Expression GetEntityStateTest(MappingEntity entity, Expression instance, LambdaExpression updateCheck)
        {
            ProjectionExpression tq = this.GetQueryExpression(entity);
            Expression where = this.GetIdentityCheck(tq.Select, entity, instance);
            Expression check = DbExpressionReplacer.Replace(updateCheck.Body, updateCheck.Parameters[0], tq.Projector);
            where = where.And(check);
            return new ExistsExpression(new SelectExpression(new TableAlias(), null, tq.Select, where));
        }

        public override Expression GetUpdateExpression(MappingEntity entity, Expression instance, LambdaExpression updateCheck, LambdaExpression selector, Expression @else)
        {
            var tableAlias = new TableAlias();
            var table = new TableExpression(tableAlias, entity, this.GetTableName(entity));

            var where = this.GetIdentityCheck(table, entity, instance);
            if (updateCheck != null)
            {
                Expression typeProjector = this.GetEntityExpression(table, entity);
                Expression pred = DbExpressionReplacer.Replace(updateCheck.Body, updateCheck.Parameters[0], typeProjector);
                where = where.And(pred);
            }

            var assignments = this.GetColumnAssignments(table, instance, entity, (e, m) => this.IsUpdatable(e, m));

            Expression update = new UpdateCommand(table, where, assignments);

            if (selector != null)
            {
                return new BlockCommand(
                    update,
                    new IFCommand(
                        this.Language.GetRowsAffectedExpression(update).GreaterThan(Expression.Constant(0)),
                        this.GetUpdateResult(entity, instance, selector),
                        @else
                        )
                    );
            }
            else if (@else != null)
            {
                return new BlockCommand(
                    update,
                    new IFCommand(
                        this.Language.GetRowsAffectedExpression(update).LessThanOrEqual(Expression.Constant(0)),
                        @else,
                        null
                        )
                    );
            }
            else
            {
                return update;
            }
        }

        protected virtual Expression GetUpdateResult(MappingEntity entity, Expression instance, LambdaExpression selector)
        {
            var tq = this.GetQueryExpression(entity);
            Expression where = this.GetIdentityCheck(tq.Select, entity, instance);
            Expression selection = DbExpressionReplacer.Replace(selector.Body, selector.Parameters[0], tq.Projector);
            TableAlias newAlias = new TableAlias();
            var pc = ColumnProjector.ProjectColumns(this.Language, selection, null, newAlias, tq.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(newAlias, pc.Columns, tq.Select, where),
                pc.Projector,
                Aggregator.GetAggregator(selector.Body.Type, typeof(IEnumerable<>).MakeGenericType(selector.Body.Type))
                );
        }

        public override Expression GetInsertOrUpdateExpression(MappingEntity entity, Expression instance, LambdaExpression updateCheck, LambdaExpression resultSelector)
        {
            if (updateCheck != null)
            {
                Expression insert = this.GetInsertExpression(entity, instance, resultSelector);
                Expression update = this.GetUpdateExpression(entity, instance, updateCheck, resultSelector, null);
                var check = this.GetEntityExistsTest(entity, instance);
                return new IFCommand(check, update, insert);
            }
            else 
            {
                Expression insert = this.GetInsertExpression(entity, instance, resultSelector);
                Expression update = this.GetUpdateExpression(entity, instance, updateCheck, resultSelector, insert);
                return update;
            }
        }

        public override Expression GetDeleteExpression(MappingEntity entity, Expression instance, LambdaExpression deleteCheck)
        {
            TableExpression table = new TableExpression(new TableAlias(), entity, this.GetTableName(entity));
            Expression where = null;

            if (instance != null)
            {
                where = this.GetIdentityCheck(table, entity, instance);
            }

            if (deleteCheck != null)
            {
                Expression row = this.GetEntityExpression(table, entity);
                Expression pred = DbExpressionReplacer.Replace(deleteCheck.Body, deleteCheck.Parameters[0], row);
                where = (where != null) ? where.And(pred) : pred;
            }

            return new DeleteCommand(table, where);
        }

        public override object CloneEntity(MappingEntity entity, object instance)
        {
            var clone = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(entity.EntityType);
            foreach (var mi in this.GetMappedMembers(entity))
            {
                if (this.IsColumn(entity, mi))
                {
                    mi.SetValue(clone, mi.GetValue(instance));
                }
            }
            return clone;
        }

        public override bool IsModified(MappingEntity entity, object instance, object original)
        {
            foreach (var mi in this.GetMappedMembers(entity))
            {
                if (this.IsColumn(entity, mi))
                {
                    if (!object.Equals(mi.GetValue(instance), mi.GetValue(original)))
                        return true;
                }
            }
            return false;
        }

        public override object GetPrimaryKey(MappingEntity entity, object instance)
        {
            object firstKey = null;
            List<object> keys = null;
            foreach (var mi in this.GetPrimaryKeyMembers(entity))
            {
                if (firstKey == null)
                {
                    firstKey = mi.GetValue(instance);
                }
                else
                {
                    if (keys == null)
                    {
                        keys = new List<object>();
                        keys.Add(firstKey);
                    }
                    keys.Add(mi.GetValue(instance));
                }
            }
            if (keys != null)
            {
                return new CompoundKey(keys.ToArray());
            }
            return firstKey;
        }

        public override Expression GetPrimaryKeyQuery(MappingEntity entity, Expression source, Expression[] keys)
        {
            // make predicate
            ParameterExpression p = Expression.Parameter(entity.ElementType, "p");
            Expression pred = null;
            var idMembers = this.GetPrimaryKeyMembers(entity).ToList();
            if (idMembers.Count != keys.Length)
            {
                throw new InvalidOperationException("Incorrect number of primary key values");
            }
            for (int i = 0, n = keys.Length; i < n; i++)
            {
                MemberInfo mem = idMembers[i];
                Type memberType = TypeHelper.GetMemberType(mem);
                if (keys[i] != null && TypeHelper.GetNonNullableType(keys[i].Type) != TypeHelper.GetNonNullableType(memberType))
                {
                    throw new InvalidOperationException("Primary key value is wrong type");
                }
                Expression eq = Expression.MakeMemberAccess(p, mem).Equal(keys[i]);
                pred = (pred == null) ? eq : pred.And(eq);
            }
            var predLambda = Expression.Lambda(pred, p);

            return Expression.Call(typeof(Queryable), "SingleOrDefault", new Type[] { entity.ElementType }, source, predLambda);
        }

        protected abstract bool IsRelationshipSource(MappingEntity entity, MemberInfo member);
        protected abstract bool IsRelationshipTarget(MappingEntity entity, MemberInfo member);

        public override IEnumerable<EntityInfo> GetDependentEntities(MappingEntity entity, object instance)
        {
            foreach (var mi in this.GetMappedMembers(entity))
            {
                if (this.IsRelationship(entity, mi) && this.IsRelationshipSource(entity, mi))
                {
                    MappingEntity relatedEntity = this.GetRelatedEntity(entity, mi);
                    var value = mi.GetValue(instance);
                    if (value != null)
                    {
                        var list = value as IList;
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (item != null)
                                {
                                    yield return new EntityInfo(item, relatedEntity);
                                }
                            }
                        }
                        else
                        {
                            yield return new EntityInfo(value, relatedEntity);
                        }
                    }
                }
            }
        }

        public override IEnumerable<EntityInfo> GetDependingEntities(MappingEntity entity, object instance)
        {
            foreach (var mi in this.GetMappedMembers(entity))
            {
                if (this.IsRelationship(entity, mi) && this.IsRelationshipTarget(entity, mi))
                {
                    MappingEntity relatedEntity = this.GetRelatedEntity(entity, mi);
                    var value = mi.GetValue(instance);
                    if (value != null)
                    {
                        var list = value as IList;
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (item != null)
                                {
                                    yield return new EntityInfo(item, relatedEntity);
                                }
                            }
                        }
                        else
                        {
                            yield return new EntityInfo(value, relatedEntity);
                        }
                    }
                }
            }
        }
    }
}