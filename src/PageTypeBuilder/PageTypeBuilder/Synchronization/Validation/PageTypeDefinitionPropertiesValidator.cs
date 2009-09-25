using System;
using System.Linq;
using System.Reflection;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Reflection;

namespace PageTypeBuilder.Synchronization.Validation
{
    public class PageTypeDefinitionPropertiesValidator
    {
        public PageTypeDefinitionPropertiesValidator(PageDefinitionTypeMapper pageDefinitionTypeMapper)
        {
            PageDefinitionTypeMapper = pageDefinitionTypeMapper;
        }

        protected internal virtual void ValidatePageTypeProperties(PageTypeDefinition definition)
        {
            PropertyInfo[] properties = definition.Type.GetPublicOrPrivateProperties();

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!propertyInfo.HasAttribute(typeof(PageTypePropertyAttribute)))
                    continue;

                ValidatePageTypeProperty(propertyInfo);
            }
        }

        protected internal virtual void ValidatePageTypeProperty(PropertyInfo propertyInfo)
        {
            ValidateCompilerGeneratedPropertyIsVirtual(propertyInfo);

            ValidatePageTypePropertyAttribute(propertyInfo);

            ValidatePageTypePropertyType(propertyInfo);
        }

        protected internal virtual void ValidateCompilerGeneratedPropertyIsVirtual(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.GetterOrSetterIsCompilerGenerated())
                return;

            if (propertyInfo.IsVirtual())
                return;

            string errorMessage = "{0} in {1} must be virtual as it is compiler generated and has {2}.";
            errorMessage = string.Format(errorMessage, propertyInfo.Name,
                                         propertyInfo.DeclaringType.Name, typeof(PageTypePropertyAttribute).Name);
            throw new Exception(errorMessage);
        }

        protected internal virtual void ValidatePageTypePropertyAttribute(PropertyInfo propertyInfo)
        {
            PageTypePropertyAttribute propertyAttribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            ValidatePageTypeAttributeTabProperty(propertyInfo, propertyAttribute);
        }

        protected internal virtual void ValidatePageTypeAttributeTabProperty(PropertyInfo propertyInfo, PageTypePropertyAttribute attribute)
        {
            if (attribute.Tab == null)
                return;

            if (!typeof(Tab).IsAssignableFrom(attribute.Tab))
            {

                string errorMessage =
                    "{0} in {1} has a {2} with Tab property set to type that does not inherit from {3}.";
                errorMessage = string.Format(errorMessage, propertyInfo.Name,
                                             propertyInfo.DeclaringType.Name, typeof(PageTypePropertyAttribute).Name,
                                             typeof(Tab));
                throw new Exception(errorMessage);
            }

            if (attribute.Tab.IsAbstract)
            {
                string errorMessage =
                    "{0} in {1} has a {2} with Tab property set to a type that is abstract.";
                errorMessage = string.Format(errorMessage, propertyInfo.Name,
                                             propertyInfo.DeclaringType.Name, typeof(PageTypePropertyAttribute).Name);
                throw new Exception(errorMessage);
            }
        }

        protected internal virtual void ValidatePageTypePropertyType(PropertyInfo propertyInfo)
        {
            PageTypePropertyAttribute pageTypePropertyAttribute = 
                (PageTypePropertyAttribute)
                    propertyInfo.GetCustomAttributes(typeof(PageTypePropertyAttribute), false).First();

            if (PageDefinitionTypeMapper.GetPageDefinitionType(
                propertyInfo.DeclaringType.Name, propertyInfo.Name, propertyInfo.PropertyType, pageTypePropertyAttribute) == null)
            {
                string errorMessage = "Unable to map the type for the property {0} in {1} to a suitable EPiServer CMS property.";
                errorMessage = string.Format(errorMessage, propertyInfo.Name, propertyInfo.DeclaringType.Name);
                throw new UnmappablePropertyTypeException(errorMessage);
            }
        }

        internal PageDefinitionTypeMapper PageDefinitionTypeMapper { get; set; }


    }
}