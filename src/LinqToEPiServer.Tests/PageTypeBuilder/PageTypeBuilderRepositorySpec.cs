using System;
using System;
using System;
using System;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;
using LinqToEPiServer.Implementation;
using LinqToEPiServer.Implementation.Visitors.Rewriters;
using LinqToEPiServer.Tests.Fakes;
using LinqToEPiServer.Tests.Helpers;
using LinqToEPiServer.Tests.Model;
using NUnit.Framework;
using PageTypeBuilder;
using LinqToEPiServer.Implementation.Visitors;

namespace LinqToEPiServer.Tests.PageTypeBuilder
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

        public class when_query_returns_empty_result_set : PageTypeBuilderRepositorySpec
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
            public void should_add_page_type_criteria_to_executed_query()
            {
                query.should_be_translated_to(new PropertyCriteria
                {
                    Condition = CompareCondition.Equal,
                    IsNull = false,
                    Name = "PageTypeID",
                    Required = true,
                    Type = PropertyDataType.PageType,
                    Value = "1"
                });
            }

            [Test]
            public void should_return_enumerable_of_correct_type()
            {
                // Add a result transformer
                Assert.IsNotNull(query.ToArray());
            }

            [Test]
            public void should_get_page_type_id_from_page_type_resolver()
            {
                Assert.Fail("Not implemented");
            }

        }
    }

    public class PageTypeBuilderRepository
    {
        private readonly StubQueryExecutor _executor;

        public PageTypeBuilderRepository(StubQueryExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            _executor = executor;
        }

        public IQueryable<T> FindDescendantsOf<T>(PageReference reference) where T : TypedPageData
        {
            var provider = new FindPagesWithCriteriaQueryProvider(reference, _executor);
            provider.AddRewriter(new RemoveOfTypeRewriter<T>());
            return new PageDataQuery(provider).OfType<T>();
        }
    }

    public class RemoveOfTypeRewriter<T> : ExpressionRewriterBase
    {
        private readonly MethodInfo QueryableOfType = ReflectionHelper.MethodOf<IQueryable>(q => q.OfType<T>());
        private readonly MethodInfo QueryableWhere = ReflectionHelper.MethodOf<IQueryable<PageData>>(q => q.Where(pd=>true));
        private readonly Expression<Func<PageData, bool>> PageTypeIDPredicate = pd => pd.PageTypeID == 1;

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method == QueryableOfType)
            {
                return AddPageTypeIDWhere(m.Arguments[0]);
            }
            return base.VisitMethodCall(m);
        }

        private Expression AddPageTypeIDWhere(Expression expression)
        {
            return Expression.Call(QueryableWhere, expression, PageTypeIDPredicate);
        }
    }
}