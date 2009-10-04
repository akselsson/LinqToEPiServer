using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class PageDataMemberPropertyReferenceExtractor : MemberPropertyReferenceExtractorBase
    {
        protected override PropertyReference GetPropertyReferencFromMember(MemberExpression e)
        {
            string memberName = e.Member.Name;
            if (!memberName.StartsWith("Page"))
                memberName = "Page" + memberName;
            return new PropertyReference(memberName, e.Type);
        }

        protected override bool AppliesToMember(MemberExpression e)
        {
            return e.Member.DeclaringType == typeof (PageData);
        }
    }
}