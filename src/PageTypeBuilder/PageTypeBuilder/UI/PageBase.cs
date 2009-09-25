using EPiServer;

namespace PageTypeBuilder.UI
{
    public abstract class PageBase<T> : PageBase where T : TypedPageData
    {
        public PageBase(int options) : base(options) { }

        public PageBase(int options, int disable) : base(options, disable) { }

        public new T CurrentPage
        {
            get
            {
                return CurrentPageRetriever.Instance.GetCurrentPage<T>(CurrentPageHandler);
            }
        }
    }
}
