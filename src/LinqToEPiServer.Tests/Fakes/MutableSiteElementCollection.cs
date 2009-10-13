using EPiServer.Configuration;

namespace LinqToEPiServer.Tests.Fakes
{
    class MutableSiteElementCollection : SiteElementCollection
    {
        public void Add(SiteElement e)
        {
            BaseAdd(e);
        }
    }
}