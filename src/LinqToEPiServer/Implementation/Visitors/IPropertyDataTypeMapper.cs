using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors
{
    public interface IPropertyDataTypeMapper
    {
        bool TryMap(PropertyComparison propertyComparison, out PropertyDataType type);
    }
}