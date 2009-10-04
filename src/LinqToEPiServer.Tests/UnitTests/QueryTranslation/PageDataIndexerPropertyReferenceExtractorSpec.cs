using System;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public class PageDataIndexerPropertyReferenceExtractorSpec
    {
        public class Indexer_cast_to_int : PropertyReferenceExtractorSpec<int>
        {
            protected override Expression<Func<PageData, int>> expression
            {
                get { return pd => (int) pd["test"]; }
            }

            protected override string expected_property_name
            {
                get { return "test"; }
            }

            protected override IPropertyReferenceExtractor system_under_test
            {
                get { return new PageDataIndexerPropertyReferenceExtractor(); }
            }

            protected override PropertyDataType? expected_property_type
            {
                get { return PropertyDataType.Number; }
            }
        }

        public class Indexer_cast_to_nullbale_int : PropertyReferenceExtractorSpec<int?>
        {
            protected override Expression<Func<PageData, int?>> expression
            {
                get { return pd => pd["test"] as int?; }
            }

            protected override string expected_property_name
            {
                get { return "test"; }
            }

            protected override IPropertyReferenceExtractor system_under_test
            {
                get { return new PageDataIndexerPropertyReferenceExtractor(); }
            }

            protected override PropertyDataType? expected_property_type
            {
                get { return PropertyDataType.Number; }
            }
        }

        public class Indexer : PropertyReferenceExtractorSpec<object>
        {
            protected override Expression<Func<PageData, object>> expression
            {
                get { return pd => pd["test"]; }
            }

            protected override string expected_property_name
            {
                get { return "test"; }
            }

            protected override IPropertyReferenceExtractor system_under_test
            {
                get { return new PageDataIndexerPropertyReferenceExtractor(); }
            }

            protected override PropertyDataType? expected_property_type
            {
                get { return null; }
            }
        }
    }
}