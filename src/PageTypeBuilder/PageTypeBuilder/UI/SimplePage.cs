using EPiServer;

namespace PageTypeBuilder.UI
{
    public abstract class SimplePage<T> : SimplePage where T : TypedPageData
    {
        public new T CurrentPage
        {
            get
            {
                return CurrentPageRetriever.Instance.GetCurrentPage<T>(CurrentPageHandler);
            }
        }
    }
}
