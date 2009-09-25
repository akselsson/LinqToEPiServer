using System.Collections.Generic;
using EPiServer.DataAbstraction;
using PageTypeBuilder.Abstractions;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Synchronization.Validation;

namespace PageTypeBuilder.Synchronization
{
    public class PageTypeSynchronizer
    {
        private List<PageTypeDefinition> _pageTypeDefinitions;

        public PageTypeSynchronizer(PageTypeDefinitionLocator pageTypeDefinitionLocator)
        {
            PageTypeResolver = PageTypeResolver.Instance;
            TabLocator = new TabLocator();
            TabDefinitionUpdater = new TabDefinitionUpdater();
            _pageTypeDefinitions = pageTypeDefinitionLocator.GetPageTypeDefinitions();
            PageTypeUpdater = new PageTypeUpdater(_pageTypeDefinitions);
            PageTypePropertyUpdater = new PageTypePropertyUpdater();
            PageTypeDefinitionValidator = new PageTypeDefinitionValidator(new PageDefinitionTypeMapper(new PageDefinitionTypeFactory()));
        }

        internal void SynchronizePageTypes()
        {
            UpdateTabDefinitions();

            List<PageTypeDefinition> pageTypeDefinitions = _pageTypeDefinitions;

            ValidatePageTypeDefinitions(pageTypeDefinitions);

            CreateNonExistingPageTypes(pageTypeDefinitions);

            AddPageTypesToResolver(pageTypeDefinitions);

            UpdatePageTypes(pageTypeDefinitions);

            UpdatePageTypePropertyDefinitions(pageTypeDefinitions);
        }

        protected internal virtual void UpdateTabDefinitions()
        {
            List<Tab> definedTabs = TabLocator.GetDefinedTabs();
            TabDefinitionUpdater.UpdateTabDefinitions(definedTabs);
        }

        protected internal virtual void ValidatePageTypeDefinitions(List<PageTypeDefinition> pageTypeDefinitions)
        {
            PageTypeDefinitionValidator.ValidatePageTypeDefinitions(pageTypeDefinitions);
        }

        protected internal virtual void CreateNonExistingPageTypes(List<PageTypeDefinition> pageTypeDefinitions)
        {
            foreach (PageTypeDefinition definition in pageTypeDefinitions)
            {
                PageType pageType = PageTypeUpdater.GetExistingPageType(definition);

                if (pageType == null)
                    PageTypeUpdater.CreateNewPageType(definition);
            }
        }

        protected internal virtual void AddPageTypesToResolver(List<PageTypeDefinition> pageTypeDefinitions)
        {
            foreach (PageTypeDefinition definition in pageTypeDefinitions)
            {
                PageType pageType = PageTypeUpdater.GetExistingPageType(definition);
                PageTypeResolver.AddPageType(pageType.ID, definition.Type);
            }
        }

        protected internal virtual void UpdatePageTypes(List<PageTypeDefinition> pageTypeDefinitions)
        {
            foreach (PageTypeDefinition definition in pageTypeDefinitions)
            {
                PageTypeUpdater.UpdatePageType(definition);
            }
        }

        protected internal virtual void UpdatePageTypePropertyDefinitions(List<PageTypeDefinition> pageTypeDefinitions)
        {
            foreach (PageTypeDefinition definition in pageTypeDefinitions)
            {
                PageType pageType = PageTypeUpdater.GetExistingPageType(definition);
                PageTypePropertyUpdater.UpdatePageTypePropertyDefinitions(pageType, definition);
            }
        }
        
        protected internal virtual PageTypeResolver PageTypeResolver { get; set; }

        protected internal virtual TabLocator TabLocator { get; set; }

        protected internal virtual TabDefinitionUpdater TabDefinitionUpdater { get; set; }

        protected internal virtual PageTypeUpdater PageTypeUpdater { get; set; }

        protected internal virtual PageTypePropertyUpdater PageTypePropertyUpdater { get; set; }

        protected internal virtual PageTypeDefinitionValidator PageTypeDefinitionValidator { get; set; }
    }
}