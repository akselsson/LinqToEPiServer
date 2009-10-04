using System;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;
using LinqToEPiServer.Implementation.Visitors;
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
                _extractor = GetExtractor();
                _result = _extractor.GetPropertyReference(expression);
            }

            protected abstract IPropertyReferenceExtractor GetExtractor();

            [Test]
            public void should_get_type_of_property()
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

            protected override IPropertyReferenceExtractor GetExtractor()
            {
                return new PageDataMemberPropertyReferenceExtractor();
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

            protected override IPropertyReferenceExtractor GetExtractor()
            {
                return new PageDataMemberPropertyReferenceExtractor();
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

            protected override IPropertyReferenceExtractor GetExtractor()
            {
                return new PageDataIndexerPropertyReferenceExtractor();
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

            protected override IPropertyReferenceExtractor GetExtractor()
            {
                return new PageDataIndexerPropertyReferenceExtractor();
            }
        }
    }
}