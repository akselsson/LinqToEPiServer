// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IQToolkit.Data
{
    using Common;

    public class DbEntitySession : DbEntitySessionBase
    {
        public DbEntitySession(DbEntityProviderBase provider)
            : base(provider)
        {
        }

        public override void SubmitChanges()
        {
            bool opened = false;
            if (this.Provider.Connection.State == System.Data.ConnectionState.Closed)
            {
                this.Provider.Connection.Open();
                opened = true;
            }
            try
            {
                if (this.Provider.Transaction == null)
                {
                    var trans = this.Provider.Connection.BeginTransaction();
                    try
                    {
                        this.Provider.Transaction = trans;
                        this.DoSubmit();
                        trans.Commit();
                    }
                    finally
                    {
                        this.Provider.Transaction = null;
                        trans.Dispose();
                    }
                }
                else
                {
                    this.DoSubmit();
                }
            }
            finally
            {
                if (opened)
                    this.Provider.Connection.Close();
            }
        }

        private void DoSubmit()
        {
            foreach (var item in this.GetOrderedItems())
            {
                var sessionTable = item.Table;
                var providerTable = sessionTable.ProviderTable;
                switch (item.State)
                {
                    case SubmitAction.Delete:
                        providerTable.Delete(item.Instance);
                        sessionTable.RemoveFromCache(item.Instance);
                        sessionTable.AssignAction(item.Instance, SubmitAction.None);
                        break;
                    case SubmitAction.Insert:
                        providerTable.Insert(item.Instance);
                        sessionTable.AddToCache(item.Instance);
                        sessionTable.AssignAction(item.Instance, SubmitAction.PossibleUpdate);
                        break;
                    case SubmitAction.InsertOrUpdate:
                        providerTable.InsertOrUpdate(item.Instance);
                        sessionTable.AddToCache(item.Instance);
                        sessionTable.AssignAction(item.Instance, SubmitAction.PossibleUpdate);
                        break;
                    case SubmitAction.PossibleUpdate:
                        if (this.Provider.Mapping.IsModified(item.Entity, item.Instance, item.Original))
                        {
                            providerTable.Update(item.Instance);
                            sessionTable.AssignAction(item.Instance, SubmitAction.PossibleUpdate);
                        }
                        break;
                    case SubmitAction.Update:
                        providerTable.Update(item.Instance);
                        break;
                    default:
                        break; // do nothing
                }
            }
        }

        protected override ISessionTable CreateTable(MappingEntity entity)
        {
            return (ISessionTable)Activator.CreateInstance(typeof(TrackedTable<>).MakeGenericType(entity.ElementType), new object[] { this, entity });
        }

        interface ITrackedTable : IEntitySessionTable
        {
            object GetFromCacheById(object key);
            object AddToCache(object instance);
            void RemoveFromCache(object instance);
            void AssignAction(object instance, SubmitAction action);
            IEnumerable<TrackedItem> TrackedItems { get; }
            TrackedItem GetTrackedItem(object instance);
        }

        protected class TrackedTable<T> : SessionTable<T>, ITrackedTable
        {
            Dictionary<T, TrackedItem> tracked;
            Dictionary<object, T> identityCache;

            public TrackedTable(DbEntitySession session, MappingEntity entity)
                : base(session, entity)
            {
                this.tracked = new Dictionary<T, TrackedItem>();
                this.identityCache = new Dictionary<object, T>();
            }

            IEnumerable<TrackedItem> ITrackedTable.TrackedItems
            {
                get { return this.tracked.Values; }
            }

            TrackedItem ITrackedTable.GetTrackedItem(object instance)
            {
                TrackedItem ti;
                if (this.tracked.TryGetValue((T)instance, out ti))
                    return ti;
                return null;
            }

            object ITrackedTable.GetFromCacheById(object key)
            {
                T cached;
                this.identityCache.TryGetValue(key, out cached);
                return cached;
            }

            public override object OnEntityMaterialized(object instance)
            {
                T typedInstance = (T)instance;
                var cached = this.AddToCache(typedInstance);
                if ((object)cached == (object)typedInstance)
                {
                    this.AssignAction(typedInstance, SubmitAction.PossibleUpdate);
                }
                return cached;
            }

            public override SubmitAction GetSubmitAction(T instance)
            {
                TrackedItem ti;
                if (this.tracked.TryGetValue(instance, out ti))
                {
                    if (ti.State == SubmitAction.PossibleUpdate && ((DbEntityProviderBase)this.Provider).Mapping.IsModified(ti.Entity, ti.Instance, ti.Original))
                        return SubmitAction.Update;
                    return ti.State;
                }
                return SubmitAction.None;
            }

            public override void SetSubmitAction(T instance, SubmitAction action)
            {
                switch (action)
                {
                    case SubmitAction.None:
                    case SubmitAction.PossibleUpdate:
                    case SubmitAction.Update:
                    case SubmitAction.Delete:
                        var cached = this.AddToCache(instance);
                        if ((object)cached != (object)instance)
                        {
                            throw new InvalidOperationException("An different instance with the same key is already in the cache.");
                        }
                        break;
                }
                this.AssignAction(instance, action);
            }

            private T AddToCache(T instance)
            {
                object key = ((DbEntityProviderBase)this.Provider).Mapping.GetPrimaryKey(this.Entity, instance);
                T cached;
                if (!this.identityCache.TryGetValue(key, out cached))
                {
                    cached = instance;
                    this.identityCache.Add(key, cached);
                }
                return cached;
            }

            object ITrackedTable.AddToCache(object instance)
            {
                return this.AddToCache((T)instance);
            }

            private void RemoveFromCache(T instance)
            {
                object key = ((DbEntityProviderBase)this.Provider).Mapping.GetPrimaryKey(this.Entity, instance);
                this.identityCache.Remove(key);
            }

            void ITrackedTable.RemoveFromCache(object instance)
            {
                this.RemoveFromCache((T)instance);
            }

            private void AssignAction(T instance, SubmitAction action)
            {
                TrackedItem ti;
                this.tracked.TryGetValue(instance, out ti);

                switch (action)
                {
                    case SubmitAction.Insert:
                    case SubmitAction.InsertOrUpdate:
                    case SubmitAction.Update:
                    case SubmitAction.Delete:
                    case SubmitAction.None:
                        this.tracked[instance] = new TrackedItem(this, instance, ti != null ? ti.Original : null, action, ti != null ? ti.HookedEvent : false);
                        break;
                    case SubmitAction.PossibleUpdate:
                        if (ti == null || !ti.HookedEvent)
                        {
                            INotifyPropertyChanging notify = instance as INotifyPropertyChanging;
                            if (notify != null)
                            {
                                notify.PropertyChanging += new PropertyChangingEventHandler(this.OnPropertyChanging);
                                this.tracked[instance] = new TrackedItem(this, instance, null, SubmitAction.PossibleUpdate, true);
                            }
                            else
                            {
                                object original = ((DbEntityProviderBase)(this.Provider)).Mapping.CloneEntity(this.Entity, instance);
                                this.tracked[instance] = new TrackedItem(this, instance, original, SubmitAction.PossibleUpdate, false);
                            }
                        }
                        else
                        {
                            goto case SubmitAction.Update;
                        }
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unknown SubmitAction: {0}", action));
                }
            }

            void ITrackedTable.AssignAction(object instance, SubmitAction action)
            {
                this.AssignAction((T)instance, action);
            }

            protected virtual void OnPropertyChanging(object sender, PropertyChangingEventArgs args)
            {
                TrackedItem ti;
                if (this.tracked.TryGetValue((T)sender, out ti) && ti.State == SubmitAction.PossibleUpdate)
                {
                    object clone = ((DbEntityProviderBase)this.Provider).Mapping.CloneEntity(ti.Entity, ti.Instance);
                    this.tracked[(T)sender] = new TrackedItem(this, ti.Instance, clone, SubmitAction.Update, true);
                }
            }
        }

        class TrackedItem
        {
            ITrackedTable table;
            object instance;
            object original;
            SubmitAction state;
            bool hookedEvent;

            internal TrackedItem(ITrackedTable table, object instance, object original, SubmitAction state, bool hookedEvent)
            {
                this.table = table;
                this.instance = instance;
                this.original = original;
                this.state = state;
                this.hookedEvent = hookedEvent;
            }

            public ITrackedTable Table
            {
                get { return this.table; }
            }

            public MappingEntity Entity 
            {
                get { return this.table.Entity; }
            }

            public object Instance
            {
                get { return this.instance; }
            }

            public object Original
            {
                get { return this.original; }
            }

            public SubmitAction State
            {
                get { return this.state; }
            }

            public bool HookedEvent
            {
                get { return this.hookedEvent; }
            }

            public static readonly IEnumerable<TrackedItem> EmptyList = new TrackedItem[] { };
        }

        private IEnumerable<TrackedItem> GetOrderedItems()
        {
            var list = this.GetTables().SelectMany(tab => ((ITrackedTable)tab).TrackedItems).ToList();
            var edges = this.GetEdges(list).Distinct().ToList();
            var stLookup = edges.ToLookup(e => e.Source, e => e.Target);
            var tsLookup = edges.ToLookup(e => e.Target, e => e.Source);

            return TopologicalSorter.Sort(list, c =>
            {
                switch (c.State)
                {
                    case SubmitAction.Insert:
                    case SubmitAction.InsertOrUpdate:
                        // all things this instance depends on must come first
                        return stLookup[c];
                    case SubmitAction.Delete:
                        // all things that depend on this instance must come first
                        return tsLookup[c];
                    default:
                        return TrackedItem.EmptyList;
                }
            });
        }

        private TrackedItem GetTrackedItem(EntityInfo entity)
        {
            ITrackedTable table = (ITrackedTable)this.GetTable(entity.Mapping);
            return table.GetTrackedItem(entity.Instance);
        }

        private IEnumerable<Edge> GetEdges(IEnumerable<TrackedItem> items)
        {
            foreach (var c in items)
            {
                foreach (var d in this.Provider.Mapping.GetDependingEntities(c.Entity, c.Instance))
                {
                    var dc = this.GetTrackedItem(d);
                    if (dc != null)
                    {
                        yield return new Edge(dc, c);
                    }
                }
                foreach (var d in this.Provider.Mapping.GetDependentEntities(c.Entity, c.Instance))
                {
                    var dc = this.GetTrackedItem(d);
                    if (dc != null)
                    {
                        yield return new Edge(c, dc);
                    }
                }
                // if another object exists with same key that is being deleted, then the inserted object depends on the deleted object 
                if (c.State == SubmitAction.Insert || c.State == SubmitAction.InsertOrUpdate)
                {
                    object cached = c.Table.GetFromCacheById(this.Provider.Mapping.GetPrimaryKey(c.Entity, c.Instance));
                    if (cached != null && cached != c.Instance)
                    {
                        var dc = this.GetTrackedItem(new EntityInfo(cached, c.Entity));
                        if (dc.State == SubmitAction.Delete)
                        {
                            yield return new Edge(c, dc);
                        }
                    }
                }
            }
        }

        class Edge : IEquatable<Edge>
        {
            internal TrackedItem Source { get; private set; }
            internal TrackedItem Target { get; private set; }
            int hash;

            internal Edge(TrackedItem source, TrackedItem target)
            {
                this.Source = source;
                this.Target = target;
                this.hash = this.Source.GetHashCode() + this.Target.GetHashCode();
            }

            public bool Equals(Edge edge)
            {
                return edge != null && this.Source == edge.Source && this.Target == edge.Target;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as Edge);
            }

            public override int GetHashCode()
            {
                return this.hash;
            }
        }
    }
}