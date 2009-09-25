using System;
using System.Collections.Generic;
using System.Linq;
using PageTypeBuilder.Discovery;

namespace PageTypeBuilder.Synchronization.Validation
{
    public class PageTypeDefinitionValidator
    {
        Type baseTypeForPageTypes = typeof(TypedPageData);

        public PageTypeDefinitionValidator(PageDefinitionTypeMapper pageDefinitionTypeMapper)
        {
            PropertiesValidator = new PageTypeDefinitionPropertiesValidator(pageDefinitionTypeMapper);
        }

        public virtual void ValidatePageTypeDefinitions(List<PageTypeDefinition> pageTypeDefinitions)
        {
            ValidatePageTypesHaveGuidOrUniqueName(pageTypeDefinitions);

            foreach (PageTypeDefinition definition in pageTypeDefinitions)
            {
                ValidatePageTypeDefinition(definition);
            }
        }

        protected internal virtual void ValidatePageTypesHaveGuidOrUniqueName(List<PageTypeDefinition> pageTypeDefinitions)
        {
            IEnumerable<IGrouping<string, PageTypeDefinition>> definitionsWithNoGuidAndSameName
                = pageTypeDefinitions
                    .Where(definition => definition.Attribute.Guid == null)
                    .GroupBy(definition => definition.GetPageTypeName())
                    .Where(groupedDefinitions => groupedDefinitions.Count() > 1);
            if (definitionsWithNoGuidAndSameName.Count() > 0)
            {
                string pageTypeName = null;
                string typeNames = null;
                const string separator = " and ";
                foreach (PageTypeDefinition definition in definitionsWithNoGuidAndSameName.First())
                {
                    pageTypeName = definition.GetPageTypeName();
                    typeNames += definition.Type.Name + " and ";
                }
                typeNames = typeNames.Remove(typeNames.Length - separator.Length);
                string errorMessage = "There are multiple types with the same page type name. The name is {0} and the types are {1}.";
                errorMessage = string.Format(errorMessage, pageTypeName, typeNames);

                throw new Exception(errorMessage);
            }
        }

        public void ValidatePageTypeDefinition(PageTypeDefinition definition)
        {
            ValidateInheritsFromBasePageType(definition);

            PropertiesValidator.ValidatePageTypeProperties(definition);
        }

        protected internal virtual void ValidateInheritsFromBasePageType(PageTypeDefinition definition)
        {
            Type typeToCheck = definition.Type;
            if (!baseTypeForPageTypes.IsAssignableFrom(typeToCheck))
            {
                string errorMessage = "The type {0} has a {1} attribute but does not inherit from {2}";
                errorMessage = string.Format(errorMessage, typeToCheck.FullName, typeof(PageTypeAttribute).FullName,
                                             baseTypeForPageTypes.FullName);

                throw new ApplicationException(errorMessage);
            }
        }

        internal PageTypeDefinitionPropertiesValidator PropertiesValidator { get; set; }
    }
}