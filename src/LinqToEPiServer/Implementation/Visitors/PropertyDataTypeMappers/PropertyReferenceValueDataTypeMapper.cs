using System;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyDataTypeMappers
{
    public class PropertyReferenceValueDataTypeMapper : IPropertyDataTypeMapper
    {
        public bool TryMap(PropertyComparison propertyComparison, out PropertyDataType type)
        {
            var mapped = propertyComparison.Property.Type;
            if (mapped.HasValue)
            {
                type = mapped.Value;
                return true;
            }
            type = default(PropertyDataType);
            return false;
        }
    }
}