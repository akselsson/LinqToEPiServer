using System;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyDataTypeMappers
{
    public class PropertyReferenceValueDataTypeMapper : TypeBasedPropertyDataTypeMapperBase
    {
        protected override Type GetType(PropertyComparison propertyComparison)
        {
            return propertyComparison.PropertyValueType;
        }
    }
}