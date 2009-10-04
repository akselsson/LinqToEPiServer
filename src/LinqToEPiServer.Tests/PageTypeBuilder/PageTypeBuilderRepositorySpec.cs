using System;
using System.Collections;
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
            provider.AddRewriter(new RemoveOfTypeRewriter<T>(provider));
            return new PageDataQuery(provider).OfType<T>();
        }
    }

    public class RemoveOfTypeRewriter<T> : ExpressionRewriterBase
    {
        private readonly IResultTransformerContainer _provider;
        private readonly MethodInfo _queryableOfType = ReflectionHelper.MethodOf<IQueryable>(q => q.OfType<T>());

        public RemoveOfTypeRewriter(IResultTransformerContainer provider)
        {
            _provider = provider;
            if (provider == null) throw new ArgumentNullException("provider");
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method == _queryableOfType)
            {
                _provider.AddResultTransformer(new OfTypeEnumerableTransformer<T>());
                return base.Visit(m.Arguments[0]);
            }
            return base.VisitMethodCall(m);
        }
    }

    public class OfTypeEnumerableTransformer<T> : IResultTransformer
    {
        public object Transform(object input)
        {
            if (input == null) throw new ArgumentNullException("input");

            var enumerable = input as IEnumerable;
            if (enumerable == null) throw new InvalidOperationException(string.Format("input must be IEnumerable, was {0}", input.GetType()));
            return enumerable.OfType<T>();
        }
    }
}