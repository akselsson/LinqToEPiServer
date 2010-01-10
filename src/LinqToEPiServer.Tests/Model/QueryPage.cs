using System;
using EPiServer.Core;
using PageTypeBuilder;

namespace LinqToEPiServer.Tests.Model
{
    [PageType("29BC37E9-67BC-4C1C-AF97-2E356759EAD1")]
    public class QueryPage : TypedPageData
    {
        [PageTypeProperty(Type = typeof(PropertyString))]
        public virtual string Text { get; set; }

        [PageTypeProperty]
        public virtual string TextWithImplicitPropertyType { get; set; }

        [PageTypeProperty]
        public virtual DateTime? Date { get; set; }

        [PageTypeProperty(Type = typeof(PropertyPageReference))]
        public virtual PageReference LinkedPage { get; set; }

        [PageTypeProperty(Type = typeof(PropertyPageType))]
        public virtual int LinkedPageType { get; set; }

        [PageTypeProperty]
        public virtual int? Number { get; set; }
    }
}