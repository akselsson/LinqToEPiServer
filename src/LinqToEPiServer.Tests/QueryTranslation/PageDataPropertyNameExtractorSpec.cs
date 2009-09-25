using System;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;
using LinqToEPiServer.Implementation.Visitors;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.QueryTranslation
{
    public class PageDataPropertyNameExtractorSpec : EPiTestBase
    {
        public abstract class GetPropertyName<T> : PageDataPropertyNameExtractorSpec
        {
            private PropertyReference _result;
            protected abstract Expression<Func<PageData, T>> expression { get; }
            protected abstract string expected_property_name { get; }

            protected override void because()
            {
                base.because();
                _result = PageDataPropertyReferenceExtractor.GetPropertyReference(expression);
            }

            [Test]
            public void should_get_type_of_property()
            {
                Assert.AreEqual(typeof(T), _result.ValueType);
            }


            [Test]
            public void should_get_name_of_property()
            {
                Assert.AreEqual(expected_property_name, _result.PropertyName);
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
        }
    }
}