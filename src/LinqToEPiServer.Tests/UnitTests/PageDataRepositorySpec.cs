using System.Linq;
using EPiServer.Core;
using LinqToEPiServer.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace LinqToEPiServer.Tests.UnitTests
{
    public class PageDataRepositorySpec : SpecRequiringEPiRuntime
    {
        private PageDataRepository system_under_test;
        private IQueryExecutor executor;

        protected override void establish_context()
        {
            base.establish_context();
            executor = MockRepository.GenerateStub<IQueryExecutor>();
            system_under_test = new PageDataRepository(executor);
        }
        
        public class find_all_pages_under_start_page : PageDataRepositorySpec
        {
            private PageDataCollection _results;
            private PageDataCollection _expected;

            protected override void establish_context()
            {
                base.establish_context();
                _expected = new PageDataCollection() { new PageData() };
                executor.Stub(e => e.FindPagesWithCriteria(null)).IgnoreArguments().Return(_expected);
            }

            protected override void because()
            {
                base.because();
               
                _results = system_under_test.FindDescendantsOf(PageReference.StartPage).ToPageDataCollection();
            }
           
            [Test]
            public void should_query_from_correct_start_point()
            {
                executor.AssertWasCalled(e=>e.FindPagesWithCriteria(PageReference.StartPage));
            }

            [Test]
            public void should_return_expected_results()
            {
                CollectionAssert.AreEqual(_expected.ToArray(), _results.ToArray());
            }
        }

        public class find_all_pages_under_root_page : PageDataRepositorySpec
        {
            protected override void establish_context()
            {
                base.establish_context();
                executor.Stub(e => e.FindPagesWithCriteria(null)).IgnoreArguments().Return(new PageDataCollection());
            }
            protected override void because()
            {
                base.because();
                system_under_test.FindDescendantsOf(PageReference.RootPage).ToPageDataCollection();
            }

            [Test]
            public void should_query_from_correct_start_point()
            {
                executor.AssertWasCalled(e=>e.FindPagesWithCriteria(PageReference.RootPage));
            }
        }
    }
}