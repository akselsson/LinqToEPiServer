using System;
using EPiServer.DataAbstraction;
using EPiServer.Editor;

namespace PageTypeBuilder
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PageTypePropertyAttribute : Attribute
    {
        private const bool DefaultDisplayInEditMode = true;

        public PageTypePropertyAttribute()
        {
            DisplayInEditMode = DefaultDisplayInEditMode;    
        }

        public Type Type { get; set; }

        public string EditCaption { get; set; }

        public string HelpText { get; set; }

        public Type Tab { get; set; }

        public bool Required { get; set; }

        public bool Searchable { get; set; }

        public string DefaultValue { get; set; }

        public DefaultValueType DefaultValueType { get; set; }

        public bool UniqueValuePerLanguage { get; set; }

        public bool DisplayInEditMode { get; set; }

        public int SortOrder { get; set; }

        public EditorToolOption LongStringSettings { get; set; }

        public bool ClearAllLongStringSettings { get; set; }
    }
}