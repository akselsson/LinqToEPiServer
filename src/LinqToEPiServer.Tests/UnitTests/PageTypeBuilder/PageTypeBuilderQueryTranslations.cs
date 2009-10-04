using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;
using LinqToEpiServer.PageTypeBuilder;
using LinqToEPiServer.Tests.Fakes;
using LinqToEPiServer.Tests.Model;
using NUnit.Framework;
using LinqToEPiServer.Tests.Helpers;

namespace LinqToEPiServer.Tests.UnitTests.PageTypeBuilder
{
    public class PageTypeBuilderQueryTranslations : EPiTestBase
    {
        private IQueryable<QueryPage> system_under_test;
        protected override void establish_context()
        {
            base.establish_context();
            var repository = new PageTypeBuilderRepository(new StubQueryExecutor());
            system_under_test = repository.FindDescendantsOf<QueryPage>(PageReference.StartPage);
        }

        [Test]
        public void page_reference()
        {
            system_under_test
                .Where(pd => pd.LinkedPage == new PageReference(12))
                .should_be_translated_to(new PropertyCriteria()
                                             {
                                                 Condition = CompareCondition.Equal,
                                                 IsNull = false,
                                                 Name = "LinkedPage",
                                                 Required = true,
                                                 Type = PropertyDataType.PageReference,
                                                 Value = "12"
                                             });
        }

        [Test]
        public void page_type()
        {
            system_under_test
                .Where(pd=>pd.LinkedPageType == 12)
                .should_be_translated_to(new PropertyCriteria()
                                             {
                                                 Condition = CompareCondition.Equal,
                                                 IsNull = false,
                                                 Name = "LinkedPageType",
                                                 Required = true,
                                                 Type = PropertyDataType.PageType,
                                                 Value = "12"
                                             });
                
        }
    }
}
