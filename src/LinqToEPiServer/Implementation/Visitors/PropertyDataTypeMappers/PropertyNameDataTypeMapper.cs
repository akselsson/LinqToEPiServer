using System.Collections.Generic;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyDataTypeMappers
{
    public class PropertyNameDataTypeMapper : IPropertyDataTypeMapper
    {
        public static readonly Dictionary<string, PropertyDataType> PropertyNameMap
            = new Dictionary<string, PropertyDataType>
                  {
                      {"PageTypeName", PropertyDataType.PageType},
                      {"PageTypeID", PropertyDataType.PageType}
                  };

        #region IPropertyDataTypeMapper Members

        public bool TryMap(PropertyComparison propertyComparison, out PropertyDataType type)
        {
            string propertyName = propertyComparison.PropertyName;
            return PropertyNameMap.TryGetValue(propertyName, out type);
        }

        #endregion
    }
}