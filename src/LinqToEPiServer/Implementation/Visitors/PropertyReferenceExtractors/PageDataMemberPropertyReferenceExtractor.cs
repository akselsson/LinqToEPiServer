using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors
{
    public class PageDataMemberPropertyReferenceExtractor : MemberPropertyReferenceExtractorBase
    {
        private const string PagetypeID = "PageTypeID";
        private const string PageTypeName = "PageTypeName";
        private const string PropertyPrefix = "Page";

        protected override PropertyReference GetPropertyReferencFromMember(MemberExpression e)
        {
            string memberName = e.Member.Name;
            memberName = EnsureNameIsPrefixedWithPage(memberName);

            if (RepresentsPageTypeMember(memberName))
                return new PropertyReference(memberName, e.Type, PropertyDataType.PageType);

            return new PropertyReference(memberName, e.Type);
        }

        private static string EnsureNameIsPrefixedWithPage(string memberName)
        {
            if (!memberName.StartsWith(PropertyPrefix))
            {
                return PropertyPrefix + memberName;
            }
            return memberName;
        }

        private static bool RepresentsPageTypeMember(string memberName)
        {
            return memberName == PagetypeID || memberName == PageTypeName;
        }

        protected override bool AppliesToMember(MemberExpression e)
        {
            return e.Member.DeclaringType == typeof (PageData);
        }
    }
}