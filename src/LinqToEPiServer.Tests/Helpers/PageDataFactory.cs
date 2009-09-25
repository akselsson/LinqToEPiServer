using System;
using EPiServer;
using EPiServer.Core;
using PageTypeBuilder;

namespace LinqToEPiServer.Tests.Helpers
{
    public class PageDataFactory : IPageDataFactory
    {
        public T CreatePage<T>(PageReference parent) where T: PageData
        {
            int? pageTypeId = PageTypeResolver.Instance.GetPageTypeID(typeof (T));
            if(pageTypeId == null)
                throw new ArgumentException(string.Format("Can not find PageType for {0}", typeof(T)));
            return (T) DataFactory.Instance.GetDefaultPageData(parent, pageTypeId.Value);
        }
    }
}