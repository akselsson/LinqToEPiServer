using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EPiServer.DataAbstraction;
using PageTypeBuilder.Discovery;
using Xunit;

namespace PageTypeBuilder.Tests.Discovery
{
    public class PageTypePropertyLocatorTests
    {
        [Fact]
        public void GivenTypeWithOnePageTypePropertyAttribute_GetPageTypePropertyDefinitions_ReturnsListWithOnePropertyDefinition()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("TestAssembly"),
                                                                                            AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Module", "Module.dll");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("PageTypeType");
            string propertyName = TestValueUtility.CreateRandomString();
            Type propertyType = typeof(string);
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);
            ConstructorInfo pageTypePropertyAttributeConstructor = typeof(PageTypePropertyAttribute).GetConstructor(new Type[0]);
            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(pageTypePropertyAttributeConstructor, new object[0]);
            propertyBuilder.SetCustomAttribute(customAttributeBuilder);
            Type type = typeBuilder.CreateType();
            PageType pageType = new PageType();
            PageTypePropertyDefinitionLocator definitionLocator = new PageTypePropertyDefinitionLocator();

            List<PageTypePropertyDefinition> propertyDefinitions = definitionLocator.GetPageTypePropertyDefinitions(pageType, type);

            Assert.Equal<int>(1, propertyDefinitions.Count);
        }
    }
}