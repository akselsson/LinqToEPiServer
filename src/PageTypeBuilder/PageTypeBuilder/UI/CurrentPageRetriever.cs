using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web.PageExtensions;

namespace PageTypeBuilder.UI
{
    public class CurrentPageRetriever
    {
        private static CurrentPageRetriever _instance;

        public T GetCurrentPage<T>(ICurrentPage currentPageHandler) where T : TypedPageData
        {
            PageData page = DataFactory.Instance.GetPage(currentPageHandler.CurrentPage.PageLink);

            if (!page.InheritsFromType<T>())
                throw new ApplicationException(
                    string.Format("The current page is not of type {0}.", typeof(T).Name));

            return (T)page;
        }

        public static CurrentPageRetriever Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new CurrentPageRetriever();

                return _instance;
            }
        }
    }
}
