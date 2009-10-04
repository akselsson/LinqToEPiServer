using EPiServer.Core;
using PageTypeBuilder;

namespace LinqToEPiServer.Tests.Model
{
    [PageType("31D0E083-767F-4A3F-BBB2-D3CF98CA1EA6")]
    public class PerformanceTestPage : TypedPageData
    {
        [PageTypeProperty(Type = typeof(PropertyString))]
        public virtual string Text { get; set; }
        [PageTypeProperty]
        public virtual int Number { get; set; }
    }

    [PageType("29BC37E9-67BC-4C1C-AF97-2E356759EAD1")]
    public class QueryPage : TypedPageData
    {
        [PageTypeProperty(Type = typeof(PropertyString))]
        public virtual string Text { get; set; }
    }
}
