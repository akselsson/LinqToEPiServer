using System;
using System.Linq;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer;
using LinqToEPiServer.Implementation;
using PageTypeBuilder;

namespace LinqToEpiServer.PageTypeBuilder
{
    public class PageTypeBuilderRepository : IPageTypeBuilderRepository
    {
        private readonly IQueryExecutor _executor;

        public PageTypeBuilderRepository(IQueryExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            _executor = executor;
        }

        public IQueryable<T> FindDescendantsOf<T>(PageReference reference) where T : TypedPageData
        {
            var provider = new FindPagesWithCriteriaQueryProvider(reference, _executor);
            provider.AddPropertyReferenceExtractor(new PageTypeBuilderPropertyReferenceExtractor());
            provider.AddResultTransformer(new FilterByType<T>());
            return new Query<T>(provider);
        }
    }
}