using System;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;
using LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public class PageDataPropertyNameExtractorSpec : EPiTestBase
    {
        public abstract class GetPropertyName<TProperty> : PageDataPropertyNameExtractorSpec
        {
            private PropertyReference _result;
            private IPropertyReferenceExtractor _extractor;
            protected abstract Expression<Func<PageData, TProperty>> expression { get; }
            protected abstract string expected_property_name { get; }

            protected override void because()
            {
                base.because();
                _extractor = system_under_test;
                _result = _extractor.GetPropertyReference(expression);
            }

            protected abstract IPropertyReferenceExtractor system_under_test { get; }
            protected abstract PropertyDataType? expected_property_type { get; }

            [Test]
            public void should_get_clr_type_of_property()
            {
                Assert.AreEqual(typeof(TProperty), _result.ValueType);
            }


            [Test]
            public void should_get_name_of_property()
            {
                Assert.AreEqual(expected_property_name, _result.PropertyName);
            }

            [Test]
            public void should_apply_to()
            {
                Assert.IsTrue(_extractor.AppliesTo(expression));
            }

            [Test]
            public void should_get_property_data_type()
            {
                Assert.AreEqual(expected_property_type,_result.Type);
            }

        }

        public class PageName : GetPropertyName<string>
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

        public class UrlSegment : GetPropertyName<string>
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

        public class Indexer_cast_to_int : GetPropertyName<int>
        {
            protected override Expression<Func<PageData, int>> expression
            {
                get { return pd => (int)pd["test"]; }
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

        public class Indexer_cast_to_nullbale_int : GetPropertyName<int?>
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

        public class Indexer : GetPropertyName<object>
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