using EPiServer;

namespace PageTypeBuilder.UI
{
    public abstract class TemplatePage<T> : TemplatePage where T : TypedPageData
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
