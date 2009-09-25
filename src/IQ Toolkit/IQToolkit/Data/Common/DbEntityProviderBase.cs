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

namespace IQToolkit.Data.Common
{
    using Mapping;

    public abstract class DbEntityProviderBase : QueryProvider, IEntityProvider
    {
        public abstract DbConnection Connection { get; }
        public abstract DbTransaction Transaction { get; set; }
        public abstract QueryMapping Mapping { get; }
        public abstract QueryLanguage Language { get; }
        public abstract QueryPolicy Policy { get; }
        public abstract TextWriter Log { get; set; }
        public abstract IEntityTable GetTable(MappingEntity entity);

        public virtual IEntityTable<T> GetTable<T>(string tableId)
        {
            return (IEntityTable<T>)this.GetTable(typeof(T), tableId);
        }

        public virtual IEntityTable GetTable(Type type, string tableId)
        {
            return this.GetTable(this.Mapping.GetEntity(type, tableId));
        }

        // called from compiled execution plan
        public abstract int RowsAffected { get; }
        public abstract object Convert(object value, Type type);
        public abstract IEnumerable<T> Execute<T>(QueryCommand command, Func<DbDataReader, T> fnProjector, MappingEntity entity, object[] paramValues);
        public abstract IEnumerable<int> ExecuteBatch(QueryCommand query, IEnumerable<object[]> paramSets, int batchSize, bool stream);
        public abstract IEnumerable<T> ExecuteBatch<T>(QueryCommand query, IEnumerable<object[]> paramSets, Func<DbDataReader, T> fnProjector, MappingEntity entity, int batchSize, bool stream);
        public abstract IEnumerable<T> ExecuteDeferred<T>(QueryCommand query, Func<DbDataReader, T> fnProjector, MappingEntity entity, object[] paramValues);
        public abstract int ExecuteCommand(QueryCommand query, object[] paramValues);
        public abstract int ExecuteCommand(string commandText);
    }
}
