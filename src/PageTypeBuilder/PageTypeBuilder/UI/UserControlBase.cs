using System;
using EPiServer;
using EPiServer.Core;

namespace PageTypeBuilder.UI
{
    public abstract class UserControlBase<T> : UserControlBase where T : TypedPageData
    {
        public new T CurrentPage
        {
            get
            {
                PageData page = DataFactory.Instance.GetPage(PageBase.CurrentPage.PageLink);

                if (!page.InheritsFromType<T>())
                    throw new Exception(
                        string.Format("The current page is not of type {0}.", typeof(T).Name));

                return (T)page;
            }
        }
    }
}
