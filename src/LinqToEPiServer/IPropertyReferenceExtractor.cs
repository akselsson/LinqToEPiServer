using System.Linq.Expressions;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer
{
    public interface IPropertyReferenceExtractor
    {
        PropertyReference GetPropertyReference(Expression expression);
        bool AppliesTo(Expression expression);
    }
}