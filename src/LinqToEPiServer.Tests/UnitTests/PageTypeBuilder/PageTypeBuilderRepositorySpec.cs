using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;
using LinqToEpiServer.PageTypeBuilder;
using LinqToEPiServer.Tests.Fakes;
using LinqToEPiServer.Tests.Helpers;
using LinqToEPiServer.Tests.Model;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.UnitTests.PageTypeBuilder
{
    public class PageTypeBuilderRepositorySpec : EPiTestBase
    {
        public PageTypeBuilderRepository system_under_test;
        public StubQueryExecutor query_executor;

        protected override void establish_context()
        {
            base.establish_context();
            query_executor = new StubQueryExecutor();
            system_under_test = new PageTypeBuilderRepository(query_executor);
        }

        public class with_empty_query : PageTypeBuilderRepositorySpec
        {
            private IQueryable<QueryPage> query;

            protected override void because()
            {
                base.because();
                query = system_under_test.FindDescendantsOf<QueryPage>(PageReference.StartPage);
            }

            [Test]
            public void should_return_enumerable_collection()
            {
                CollectionAssert.IsEmpty(query);
            }

            [Test]
            public void should_not_changed_criteria()
            {
                query.should_be_translated_to();
            }

            [Test]
            public void should_return_enumerable_of_correct_type()
            {
                Assert.IsNotNull(query.ToArray());
            }
        }

        public class with_query_for_one_property : PageTypeBuilderRepositorySpec
        {
            private IQueryable<QueryPage> query;

            protected override void because()
            {
                base.because();
                query = system_under_test
                    .FindDescendantsOf<QueryPage>(PageReference.StartPage)
                    .Where(qp => qp.Text == "test");
            }

            [Test]
            public void should_add_criteria_for_property()
            {
                query.should_be_translated_to(new PropertyCriteria()
                                                  {
                                                      Condition = CompareCondition.Equal,
                                                      IsNull = false,
                                                      Name = "Text",
                                                      Required = true,
                                                      Type = PropertyDataType.String,
                                                      Value = "test"
                                                  });
            }
        }
    }
}