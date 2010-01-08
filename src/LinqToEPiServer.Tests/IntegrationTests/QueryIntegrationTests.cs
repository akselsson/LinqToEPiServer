using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using NUnit.Framework;
using LinqToEPiServer.Tests.Helpers;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    public class QueryIntegrationTests : IntegrationTestsBase
    {
        private PageReference _root;
        private PageReference _start;
        private PageReference _child;
        private PageReference _child2;
        private PageDataRepository _repository;

        protected override void establish_context()
        {
            base.establish_context();

            _repository = new PageDataRepository(new DataFactoryQueryExecutor());

            var pageType = PageType.List().First();

            _root = PageReference.RootPage;
            var start = DataFactory.Instance.GetDefaultPageData(_root, pageType.ID);
            start.PageName = "start";
            start.URLSegment = "start";
            _start = DataFactory.Instance.Save(start, SaveAction.Publish);

            var child = DataFactory.Instance.GetDefaultPageData(start.PageLink, pageType.ID);
            child.PageName = "child";
            child.URLSegment = "child";
            _child = DataFactory.Instance.Save(child, SaveAction.Publish);

            var child2 = DataFactory.Instance.GetDefaultPageData(start.PageLink, pageType.ID);
            child2.PageName = "child";
            child2.URLSegment = "child";
            child2.StopPublish = DateTime.Today;
            _child2 = DataFactory.Instance.Save(child2, SaveAction.Publish);
        }

        [Test]
        public void and_or_with_PageLink_PageName_PageStopPublish()
        {
            IQueryable<PageData> query = _repository.FindDescendantsOf(_root).Where(pd => (pd.PageLink == _start || pd.PageName == "child") && (DateTime?)pd["PageStopPublish"] == null);
            query.should_return(_start, _child);
        }

        [Test]
        public void PageStopPublish_not_null()
        {
            var query = _repository.FindDescendantsOf(_root).Where(pd => (DateTime?)pd["PageStopPublish"] != null);
            query.should_return(_child2);
        }
    }
}
