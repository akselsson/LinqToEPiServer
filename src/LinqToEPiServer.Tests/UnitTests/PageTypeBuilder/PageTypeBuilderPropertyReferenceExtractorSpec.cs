using System;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEpiServer.PageTypeBuilder;
using LinqToEPiServer.Tests.Model;
using LinqToEPiServer.Tests.UnitTests.QueryTranslation;

namespace LinqToEPiServer.Tests.UnitTests.PageTypeBuilder
{
    public class PageTypeBuilderPropertyReferenceExtractorSpec
    {
        public class PageReferenceProperty : PropertyReferenceExtractorSpec<PageReference>
        {
            protected override Expression<Func<PageData, PageReference>> expression
            {
                get { return pd => ((QueryPage) pd).LinkedPage; }
            }

            protected override string expected_property_name
            {
                get { return "LinkedPage"; }
            }

            protected override IPropertyReferenceExtractor system_under_test
            {
                get { return new PageTypeBuilderPropertyReferenceExtractor(); }
            }

            protected override PropertyDataType? expected_property_type
            {
                get { return PropertyDataType.PageReference; }
            }
        }

        public class PageTypeProperty : PropertyReferenceExtractorSpec<int>
        {
            protected override Expression<Func<PageData, int>> expression
            {
                get { return pd => ((QueryPage) pd).LinkedPageType; }
            }

            protected override string expected_property_name
            {
                get { return "LinkedPageType"; }
            }

            protected override IPropertyReferenceExtractor system_under_test
            {
                get { return new PageTypeBuilderPropertyReferenceExtractor(); }
            }

            protected override PropertyDataType? expected_property_type
            {
                get { return PropertyDataType.PageType; }
            }
        }
    }
}