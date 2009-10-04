using System;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;
using LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors;
using PageTypeBuilder;
using System.Linq;

namespace LinqToEpiServer.PageTypeBuilder
{
    public class PageTypeBuilderPropertyReferenceExtractor : MemberPropertyReferenceExtractorBase
    {
        protected override PropertyReference GetPropertyReferencFromMember(MemberExpression e)
        {
            var member = e.Member;
            PropertyDataType propertyDataType = GetPropertyDataType(member);
            return new PropertyReference(member.Name,e.Type, propertyDataType);
        }

       
        protected override bool AppliesToMember(MemberExpression e)
        {
            return typeof(TypedPageData).IsAssignableFrom(e.Member.DeclaringType);
        }

        private PropertyDataType GetPropertyDataType(MemberInfo member)
        {
            PageTypePropertyAttribute pageTypePropertyAttribute = GetPageTypePropertyAttribute(member);
            PropertyData propertyData = GetDefaultPropertyData(pageTypePropertyAttribute);
            return propertyData.Type;
        }

        private PropertyData GetDefaultPropertyData(PageTypePropertyAttribute pageTypePropertyAttribute)
        {
            return (PropertyData)Activator.CreateInstance(pageTypePropertyAttribute.Type);
        }

        private PageTypePropertyAttribute GetPageTypePropertyAttribute(MemberInfo member)
        {
            var pageTypePropertyAttribute = member
                .GetCustomAttributes(typeof(PageTypePropertyAttribute), false)
                .OfType<PageTypePropertyAttribute>().SingleOrDefault();

            if (pageTypePropertyAttribute == null)
                throw new InvalidOperationException(string.Format("Property {0} does not have PageTypePropertyAttribute", member));
            return pageTypePropertyAttribute;
        }

    }
}