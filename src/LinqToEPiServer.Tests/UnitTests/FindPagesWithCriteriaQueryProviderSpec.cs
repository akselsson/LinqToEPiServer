using System.Linq;
using System.Linq.Expressions;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer.Implementation;
using LinqToEPiServer.Implementation.Visitors;
using LinqToEPiServer.Tests.Fakes;
using NUnit.Framework;
using Rhino.Mocks;

namespace LinqToEPiServer.Tests.UnitTests
{
    public class FindPagesWithCriteriaQueryProviderSpec : SpecRequiringEPiRuntime
    {
        private FindPagesWithCriteriaQueryProvider system_under_test;

        protected override void establish_context()
        {
            base.establish_context();
            system_under_test = new FindPagesWithCriteriaQueryProvider(PageReference.StartPage, new StubQueryExecutor());
        }

        public class when_getting_query_text : FindPagesWithCriteriaQueryProviderSpec
        {
            private string query_text;

            protected override void because()
            {
                base.because();
                var query = from page in new Query<PageData>(system_under_test)
                            where page.PageName == "test" && page.URLSegment == "url"
                            select page;
                query_text = system_under_test.GetQueryText(query.Expression);
            }

            [Test]
            public void should_return_a_formatted_list_of_criteria()
            {
                Assert.AreEqual(
                    @"{ Condition = Equal, IsNull = False, Name = PageName, Required = True, Type = String, Value = test }
{ Condition = Equal, IsNull = False, Name = PageURLSegment, Required = True, Type = String, Value = url }",
                    query_text);
            }
        }

        public class execute_query_with_custom_rewriter : FindPagesWithCriteriaQueryProviderSpec
        {
            private IExpressionRewriter rewriter;
            private ConstantExpression expression;

            protected override void establish_context()
            {
                base.establish_context();
                expression = Expression.Constant(this);
                rewriter = MockRepository.GenerateStub<IExpressionRewriter>();
                system_under_test.AddRewriter(rewriter);
            }

            protected override void because()
            {
                base.because();
                system_under_test.Execute(expression);
            }

            [Test]
            public void should_rewrite_expression_with_rewriter()
            {
                rewriter.AssertWasCalled(r => r.Rewrite(expression));
            }
        }
    }
}