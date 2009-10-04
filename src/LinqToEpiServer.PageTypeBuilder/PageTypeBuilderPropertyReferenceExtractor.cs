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
            PropertyDataType propertyDataType = GetPropertyDataType(e);
            return new PropertyReference(e.Member.Name, propertyDataType);
        }

        protected override bool AppliesToMember(MemberExpression e)
        {
            return HasPageTypePropertyAttribute(e.Member);
        }

        private static bool HasPageTypePropertyAttribute(ICustomAttributeProvider member)
        {
            return member.GetCustomAttributes(typeof (PageTypePropertyAttribute), false).Any();
        }

        private PropertyDataType GetPropertyDataType(MemberExpression expression)
        {
            PageTypePropertyAttribute pageTypePropertyAttribute = GetPageTypePropertyAttribute(expression.Member);
            
            if (pageTypePropertyAttribute.Type == null)
                return GetPropertyTypeFromReturnType(expression);

            PropertyData emptyPropertyData = GetEmptyPropertyData(pageTypePropertyAttribute);
            return emptyPropertyData.Type;
        }

        private PropertyDataType GetPropertyTypeFromReturnType(MemberExpression member)
        {
            PropertyDataType propertyDataType;
            if (TypeToPropertyDataTypeMapper.TryMap(member.Type, out propertyDataType))
                return propertyDataType;
            throw new NotSupportedException(string.Format("Could not map member {0} to PropertyDataType", member));
        }

        private PropertyData GetEmptyPropertyData(PageTypePropertyAttribute pageTypePropertyAttribute)
        {
            return (PropertyData) Activator.CreateInstance(pageTypePropertyAttribute.Type);
        }

        private PageTypePropertyAttribute GetPageTypePropertyAttribute(MemberInfo member)
        {
            var pageTypePropertyAttribute = member
                .GetCustomAttributes(typeof (PageTypePropertyAttribute), false)
                .OfType<PageTypePropertyAttribute>()
                .SingleOrDefault();

            if (pageTypePropertyAttribute == null)
                throw new InvalidOperationException(string.Format(
                                                        "Property {0} does not have PageTypePropertyAttribute", member));
            return pageTypePropertyAttribute;
        }
    }
}