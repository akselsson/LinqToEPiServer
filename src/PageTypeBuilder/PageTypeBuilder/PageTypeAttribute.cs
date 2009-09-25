using System;
using EPiServer.Filters;

namespace PageTypeBuilder
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PageTypeAttribute : Attribute 
    {
        internal const int DefaultSortOrder = 100;
        internal const bool DefaultAvailableInEditMode = true;
        internal const bool DefaultDefaultVisibleInMenu = true;
        internal const int DefaultDefaultSortIndex = -1;
        internal const int DefaultDefaultArchiveToPageID = -1;

        public PageTypeAttribute() :this(null) {}

        public PageTypeAttribute(string guid)
        {
            if (guid != null)
            {
                Guid = new Guid(guid);
            }

            SortOrder = DefaultSortOrder;
            AvailableInEditMode = DefaultAvailableInEditMode;
            DefaultVisibleInMenu = DefaultDefaultVisibleInMenu;
            DefaultSortIndex = DefaultDefaultSortIndex;
            DefaultArchiveToPageID = DefaultDefaultArchiveToPageID;
        }

        public Guid? Guid { get; private set; }

        public string Name { get; set; }

        public string Filename { get; set; }

        public int SortOrder { get; set; }

        public string Description { get; set; }

        public bool AvailableInEditMode { get; set; }

        public string DefaultPageName { get; set; }

        public int DefaultStartPublishOffsetMinutes { get; set; }

        public int DefaultStopPublishOffsetMinutes { get; set; }

        public bool DefaultVisibleInMenu { get; set; }

        public int DefaultSortIndex { get; set; }

        public FilterSortOrder DefaultChildSortOrder { get; set; }

        public int DefaultArchiveToPageID { get; set; }

        public int DefaultFrameID { get; set; }

        public Type[] AvailablePageTypes { get; set; }
    }
}