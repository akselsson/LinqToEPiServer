using EPiServer;

namespace PageTypeBuilder.UI
{
    public abstract class EditPage<T> : EditPage where T : TypedPageData
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
