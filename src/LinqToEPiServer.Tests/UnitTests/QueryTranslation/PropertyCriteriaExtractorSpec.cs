using System;
using System.Linq;
using System.Linq.Expressions;
using LinqToEPiServer.Implementation.Helpers;
using LinqToEPiServer.Implementation.Visitors;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public class PropertyCriteriaExtractorSpec :EPiTestBase
    {
        private PropertyCriteriaExtractor system_under_test;
        protected override void establish_context()
        {
            base.establish_context();
            system_under_test = new PropertyCriteriaExtractor(new[]{new PageDataIndexerPropertyReferenceExtractor()});
        }

        [Test]
        public void should_extract_single_criteria_twice_with_same_results()
        {
            Expression<Action<IQueryable<PageData>>> expression = q => q.Where(pd => pd["PageName"] == "test");
            
            var first = system_under_test.ConvertToCriteria(expression);
            var second = system_under_test.ConvertToCriteria(expression);

            CollectionAssert.AreEqual(first.EquatableWithFormatting(),second.EquatableWithFormatting());
        }
    }
}
