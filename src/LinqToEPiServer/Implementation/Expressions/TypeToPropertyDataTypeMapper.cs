using System;
using System.Collections.Generic;
using EPiServer.Core;

namespace LinqToEPiServer.Implementation.Expressions
{
    public class TypeToPropertyDataTypeMapper
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

        public static bool TryMap(Type type,out PropertyDataType propertyDataType)
        {
            return TypeMap.TryGetValue(type, out propertyDataType);
        }
        
    }
}