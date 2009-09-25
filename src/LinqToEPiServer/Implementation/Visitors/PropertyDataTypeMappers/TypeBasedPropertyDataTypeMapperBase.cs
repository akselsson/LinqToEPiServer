using System;
using System.Collections.Generic;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyDataTypeMappers
{
    public abstract class TypeBasedPropertyDataTypeMapperBase : IPropertyDataTypeMapper
    {
        private static readonly Dictionary<Type, PropertyDataType> TypeMap
            = new Dictionary<Type, PropertyDataType>
                  {
                      {typeof (string), PropertyDataType.String},
                      {typeof (int), PropertyDataType.Number},
                      {typeof (int?), PropertyDataType.Number},
                      {typeof (DateTime), PropertyDataType.Date},
                      {typeof (DateTime?), PropertyDataType.Date},
                      {typeof (bool), PropertyDataType.Boolean},
                      {typeof (bool?), PropertyDataType.Boolean},
                      {typeof (double), PropertyDataType.FloatNumber},
                      {typeof (double?), PropertyDataType.FloatNumber},
                      {typeof (float), PropertyDataType.FloatNumber},
                      {typeof (float?), PropertyDataType.FloatNumber},
                      {typeof (PageReference), PropertyDataType.PageReference},
                  };

        #region IPropertyDataTypeMapper Members

        public virtual bool TryMap(PropertyComparison propertyComparison, out PropertyDataType type)
        {
            type = default(PropertyDataType);
            Type clrType = GetType(propertyComparison);
            if (clrType == null)
                return false;
            if (TryMapType(clrType, out type))
            {
                return true;
            }
            if (clrType == typeof (object))
                return false;
            throw new NotSupportedException(String.Format("Can not map target type to PropertyDataType for {0}:",
                                                          clrType));
        }

        #endregion

        protected abstract Type GetType(PropertyComparison propertyComparison);

        protected bool TryMapType(Type clrType, out PropertyDataType type)
        {
            return TypeMap.TryGetValue(clrType, out type);
        }
    }
}