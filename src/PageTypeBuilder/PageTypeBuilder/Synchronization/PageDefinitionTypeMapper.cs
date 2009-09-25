using System;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.SpecializedProperties;
using PageTypeBuilder.Abstractions;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Synchronization.Validation;

namespace PageTypeBuilder.Synchronization
{
    public class PageDefinitionTypeMapper
    {
        public PageDefinitionTypeMapper(PageDefinitionTypeFactory pageDefinitionTypeFactory)
        {
            PageDefinitionTypeFactory = pageDefinitionTypeFactory;
        }

        private PageDefinitionTypeFactory PageDefinitionTypeFactory { get; set; }

        protected internal virtual PageDefinitionType GetPageDefinitionType(PageTypePropertyDefinition definition)
        {
            return GetPageDefinitionType(
                definition.PageType.Name, definition.Name, definition.PropertyType, definition.PageTypePropertyAttribute);
        }

        protected internal virtual PageDefinitionType GetPageDefinitionType(
            string pageTypeName, string propertyName, 
            Type propertyType, PageTypePropertyAttribute pageTypePropertyAttribute)
        {
            Type pagePropertyType = GetPropertyType(propertyType, pageTypePropertyAttribute);

            if (pagePropertyType == null)
            {
                string errorMessage = "Unable to find a valid EPiServer property type for the property {0} in the page type {1}";
                errorMessage = string.Format(errorMessage, propertyName, pageTypeName);
                throw new UnmappablePropertyTypeException(errorMessage);
            }

            PageDefinitionType pageDefinitionType;

            if (TypeIsNativePropertyType(pagePropertyType))
            {
                int nativeTypeID = GetNativeTypeID(pagePropertyType);
                pageDefinitionType = PageDefinitionTypeFactory.GetPageDefinitionType(nativeTypeID);
            }
            else
            {
                string pageDefinitionTypeName = pagePropertyType.FullName;
                string assemblyName = pagePropertyType.Assembly.GetName().Name;
                pageDefinitionType = PageDefinitionTypeFactory.GetPageDefinitionType(pageDefinitionTypeName, assemblyName);
            }
            return pageDefinitionType;
        }

        protected internal virtual Type GetPropertyType(Type propertyType, PageTypePropertyAttribute pageTypePropertAttribute)
        {
            Type pagePropertyType = pageTypePropertAttribute.Type;
            if (pagePropertyType == null)
            {
                pagePropertyType = GetDefaultPropertyType(propertyType);
            }
            return pagePropertyType;
        }

        protected internal virtual Type GetDefaultPropertyType(Type propertyType)
        {
            Type pagePropertyType = null;

            if (propertyType == typeof(string))
            {
                pagePropertyType = typeof(PropertyXhtmlString);
            }
            else if (propertyType == typeof(int) || propertyType == typeof(int?))
            {
                pagePropertyType = typeof(PropertyNumber);
            }
            else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
            {
                pagePropertyType = typeof(PropertyBoolean);
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                pagePropertyType = typeof(PropertyDate);
            }
            else if (propertyType == typeof(float) || propertyType == typeof(float?))
            {
                pagePropertyType = typeof(PropertyFloatNumber);
            }
            else if (propertyType == typeof(PageReference))
            {
                pagePropertyType = typeof(PropertyPageReference);
            }
            else if (propertyType == typeof(PageType))
            {
                pagePropertyType = typeof(PropertyPageType);
            }
            return pagePropertyType;
        }

        private bool TypeIsNativePropertyType(Type pagePropertyType)
        {
            return NativePropertyTypes.Contains(pagePropertyType);
        }

        protected internal virtual int GetNativeTypeID(Type pagePropertyType)
        {
            int? nativeTypeID = null;
            for (int typeID = 0; typeID < NativePropertyTypes.Length; typeID++)
            {
                if (NativePropertyTypes[typeID] == pagePropertyType)
                {
                    nativeTypeID = typeID;
                }
            }

            if (!nativeTypeID.HasValue)
            {
                string errorMessage = "Unable to retrieve native type ID. Type {0} is not a native type.";
                errorMessage = string.Format(errorMessage, pagePropertyType.FullName);
                throw new Exception(errorMessage);
            }

            return nativeTypeID.Value;
        }

        internal Type[] NativePropertyTypes
        {
            get
            {
                Type[] nativeProperties = new Type[9];
                nativeProperties[0] = typeof(PropertyBoolean);
                nativeProperties[1] = typeof(PropertyNumber);
                nativeProperties[2] = typeof(PropertyFloatNumber);
                nativeProperties[3] = typeof(PropertyPageType);
                nativeProperties[4] = typeof(PropertyPageReference);
                nativeProperties[5] = typeof(PropertyDate);
                nativeProperties[6] = typeof(PropertyString);
                nativeProperties[7] = typeof(PropertyLongString);
                nativeProperties[8] = typeof(PropertyCategory);

                return nativeProperties;
            }
        }
    }
}
