using System;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public class PageDataMemberPropertyReferenceExtractorSpec
    {
        public class PageName : PropertyReferenceExtractorSpec<string>
        {
            protected override Expression<Func<PageData, string>> expression
            {
                get { return pd => pd.PageName; }
            }

            protected override string expected_property_name
            {
                get { return "PageName"; }
            }

            protected override IPropertyReferenceExtractor system_under_test
            {
                get { return new PageDataMemberPropertyReferenceExtractor(); }
            }

            protected override PropertyDataType? expected_property_type
            {
                get { return PropertyDataType.String; }
            }
        }

        public class UrlSegment : PropertyReferenceExtractorSpec<string>
        {
            protected override Expression<Func<PageData, string>> expression
            {
                get { return pd => pd.URLSegment; }
            }

            protected override string expected_property_name
            {
                get { return "PageURLSegment"; }
            }

            protected override IPropertyReferenceExtractor system_under_test
            {
                get { return new PageDataMemberPropertyReferenceExtractor(); }
            }

            protected override PropertyDataType? expected_property_type
            {
                get { return PropertyDataType.String; }
            }
        }
    }
}