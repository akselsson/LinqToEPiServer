using System;
using System.Linq;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer;
using LinqToEPiServer.Implementation;
using PageTypeBuilder;
using EPiServer;
using EPiServer.DataAccess;

namespace LinqToEpiServer.PageTypeBuilder
{
    public class PageTypeBuilderRepository : PageDataRepository, IPageTypeBuilderRepository
    {
        private readonly IQueryExecutor _executor;
        private readonly PageTypeResolver _resolver = PageTypeResolver.Instance;
        private readonly DataFactory _dataFactory = DataFactory.Instance;

        public PageTypeBuilderRepository(IQueryExecutor executor) : base(executor)
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

        public T Add<T>(PageReference parent, Action<T> buildup) where T: PageData
        {
            int pageTypeID = ResolvePageType<T>();
            var defaultPageData = (T)_dataFactory.GetDefaultPageData(parent,pageTypeID);
            buildup(defaultPageData);
            var pageLink = _dataFactory.Save(defaultPageData, SaveAction.Publish);
            return (T)_dataFactory.GetPage(pageLink);
            
        }

        private int ResolvePageType<T>()
        {
            int? pageTypeId = _resolver.GetPageTypeID(typeof (T));
            if (pageTypeId == null)
            {
                throw new InvalidOperationException("Could not get page type id for " + typeof(T).FullName);
            }
            return pageTypeId.Value;
        }
    }
}