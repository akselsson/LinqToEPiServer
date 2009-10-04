using System.Linq.Expressions;
using System.Reflection;
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
            memberName = EnsureNameIsPrefixed(memberName);

            if (RepresentsPageTypeMember(e.Member))
                return new PropertyReference(memberName, PropertyDataType.PageType);

            return new PropertyReference(memberName, e.Type);
        }

        private static string EnsureNameIsPrefixed(string memberName)
        {
            if (!memberName.StartsWith(PropertyPrefix))
            {
                return PropertyPrefix + memberName;
            }
            return memberName;
        }

        private static bool RepresentsPageTypeMember(MemberInfo member)
        {
            return member.Name == PagetypeID || member.Name == PageTypeName;
        }

        protected override bool AppliesToMember(MemberExpression e)
        {
            return e.Member.DeclaringType == typeof (PageData);
        }
    }
}