using EPiServer.Core;

namespace LinqToEPiServer.Tests.Helpers
{
    public interface IPageDataFactory
    {
        T CreatePage<T>(PageReference parent) where T: PageData;
    }
}