using System;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Expressions;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public abstract class PropertyReferenceExtractorSpec<TProperty> : SpecRequiringEPiRuntime
    {
        private PropertyReference _result;
        private IPropertyReferenceExtractor _extractor;

        protected override void because()
        {
            base.because();
            _extractor = system_under_test;
            _result = _extractor.GetPropertyReference(expression);
        }

        protected abstract IPropertyReferenceExtractor system_under_test { get; }
        protected abstract PropertyDataType? expected_property_type { get; }
        protected abstract Expression<Func<PageData, TProperty>> expression { get; }
        protected abstract string expected_property_name { get; }

        [Test]
        public void should_get_correct_property_name()
        {
            Assert.AreEqual(expected_property_name, _result.PropertyName);
        }

        [Test]
        public void should_be_applicable_to_expression()
        {
            Assert.IsTrue(_extractor.AppliesTo(expression));
        }

        [Test]
        public void should_get_property_data_type()
        {
            Assert.AreEqual(expected_property_type, _result.Type);
        }

    }
}