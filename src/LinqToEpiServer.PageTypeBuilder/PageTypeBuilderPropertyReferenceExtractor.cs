using System.Linq.Expressions;
using LinqToEPiServer.Implementation.Expressions;
using LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors;
using PageTypeBuilder;

namespace LinqToEpiServer.PageTypeBuilder
{
    public class PageTypeBuilderPropertyReferenceExtractor : MemberPropertyReferenceExtractorBase
    {
        protected override PropertyReference GetPropertyReferencFromMember(MemberExpression e)
        {
            return new PropertyReference(e.Member.Name, e.Type);
        }

        protected override bool AppliesToMember(MemberExpression e)
        {
            return typeof(TypedPageData).IsAssignableFrom(e.Member.DeclaringType);
        }
    }
}