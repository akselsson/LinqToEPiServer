using System;
using System.Linq;
using System.Reflection;
using EPiServer.DataAbstraction;
using Rhino.Mocks;
using Xunit;
using PageTypeBuilder.Abstractions;
using PageTypeBuilder.Synchronization;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Reflection;
using PageTypeBuilder.Synchronization.Validation;

namespace PageTypeBuilder.Tests.Synchronization.Validation
{
    public class PageTypeDefinitionPropertiesValidatorTests
    {
        [Fact]
        public void Constructor_SetsPropertiesValidtorProperty()
        {
            PageDefinitionTypeMapper mapper = new PageDefinitionTypeMapper(null);

            PageTypeDefinitionPropertiesValidator validator = new PageTypeDefinitionPropertiesValidator(mapper);

            Assert.NotNull(validator.PageDefinitionTypeMapper);
        }

        [Fact]
        public void GivenPageTypeDefinition_ValidatePageTypeProperties_CallsValidatePageTypePropertyForEachPropertyWithPageTypePropertyAttribute()
        {
            PageTypeDefinition definition = new PageTypeDefinition
                                                {
                                                    Type = typeof(TestPageType),
                                                    Attribute = new PageTypeAttribute()
                                                };
            MockRepository fakes = new MockRepository();
            PageTypeDefinitionPropertiesValidator propertiesValidator = fakes.PartialMock<PageTypeDefinitionPropertiesValidator>((PageDefinitionTypeMapper)null);
            propertiesValidator.Stub(
                validator => validator.ValidatePageTypeProperty(
                                 Arg<PropertyInfo>.Is.Anything));
            propertiesValidator.Replay();

            propertiesValidator.ValidatePageTypeProperties(definition);

            int expectedNumberOfCalls = (from property in definition.Type.GetPublicOrPrivateProperties()
                                         where property.HasAttribute(typeof(PageTypePropertyAttribute))
                                         select property).Count();
            propertiesValidator.AssertWasCalled(validator => 
                                                validator.ValidatePageTypeProperty(
                                                    Arg<PropertyInfo>.Is.NotNull), options => options.Repeat.Times(expectedNumberOfCalls));
        }

        [Fact]
        public void GivenPropertyInfo_ValidatePageTypeProperty_ValidatesCompilerGeneratedPropertyIsVirtual()
        {
            PropertyInfo propertyInfo = CreateFakePropertyInfo();
            PageTypeDefinitionPropertiesValidator propertiesValidator = 
                CreateValidatorWithFakeValidatePageTypePropertyMethodCalls(propertyInfo);

            propertiesValidator.ValidatePageTypeProperty(propertyInfo);

            propertiesValidator.AssertWasCalled(validator => validator.ValidateCompilerGeneratedPropertyIsVirtual(propertyInfo));
        }

        private PropertyInfo CreateFakePropertyInfo()
        {
            MockRepository fakes = new MockRepository();
            return fakes.Stub<PropertyInfo>();
        }

        private PageTypeDefinitionPropertiesValidator CreateValidatorWithFakeValidatePageTypePropertyMethodCalls(PropertyInfo propertyInfo)
        {
            MockRepository fakes = new MockRepository();
            PageTypeDefinitionPropertiesValidator propertiesValidator = fakes.PartialMock<PageTypeDefinitionPropertiesValidator>((PageDefinitionTypeMapper)null);
            propertiesValidator.Stub(validator => validator.ValidateCompilerGeneratedPropertyIsVirtual(propertyInfo));
            propertiesValidator.Stub(validator => validator.ValidatePageTypePropertyAttribute(propertyInfo));
            propertiesValidator.Stub(validator => validator.ValidatePageTypePropertyType(propertyInfo));
            propertiesValidator.Replay();
            return propertiesValidator;
        }

        [Fact]
        public void GivenPropertyInfo_ValidatePageTypeProperty_ValidatesPageTypePropertyAttributes()
        {
            PropertyInfo propertyInfo = CreateFakePropertyInfo();
            PageTypeDefinitionPropertiesValidator propertiesValidator = 
                CreateValidatorWithFakeValidatePageTypePropertyMethodCalls(propertyInfo);

            propertiesValidator.ValidatePageTypeProperty(propertyInfo);

            propertiesValidator.AssertWasCalled(validator => validator.ValidatePageTypePropertyAttribute(propertyInfo));
        }

        [Fact]
        public void GivenNonVirtualProperty_ValidateCompilerGeneratedPropertyIsVirtual_DoesNotThrowsException()
        {
            PropertyInfo nonVirtualCompilerGeneratedProperty = typeof(TestPageType).GetProperty("NotGeneratedProperty");
            PageTypeDefinitionPropertiesValidator propertiesValidator = new PageTypeDefinitionPropertiesValidator(null);


            Exception exception = Record.Exception(() =>
                                                       {
                                                           propertiesValidator.ValidateCompilerGeneratedPropertyIsVirtual(nonVirtualCompilerGeneratedProperty);
                                                       });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenVirtualCompilerGeneratedProperty_ValidateCompilerGeneratedPropertyIsVirtual_DoesNotThrowsException()
        {
            PropertyInfo nonVirtualCompilerGeneratedProperty = typeof(TestPageType).GetProperty("CompilerGeneratedProperty");
            PageTypeDefinitionPropertiesValidator propertiesValidator = new PageTypeDefinitionPropertiesValidator(null);


            Exception exception = Record.Exception(() =>
               {
                   propertiesValidator.ValidateCompilerGeneratedPropertyIsVirtual(nonVirtualCompilerGeneratedProperty);
               });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenNonVirtualCompilerGeneratedProperty_ValidateCompilerGeneratedPropertyIsVirtual_ThrowsException()
        {
            PropertyInfo nonVirtualCompilerGeneratedProperty = typeof(TestPageType).GetProperty("CompilerGeneratedNonVirtualProperty");
            PageTypeDefinitionPropertiesValidator propertiesValidator = new PageTypeDefinitionPropertiesValidator(null);


            Exception exception = Record.Exception(() =>
               {
                   propertiesValidator.ValidateCompilerGeneratedPropertyIsVirtual(nonVirtualCompilerGeneratedProperty);
               });

            Assert.NotNull(exception);
        }

        [Fact]
        public void GivenPropertyInfo_ValidatePageTypePropertyAttribute_ValidatesPageTypeAttributeTabProperty()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("StringTestProperty");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            MockRepository fakes = new MockRepository();
            PageTypeDefinitionPropertiesValidator propertiesValidator = fakes.PartialMock<PageTypeDefinitionPropertiesValidator>((PageDefinitionTypeMapper)null);
            propertiesValidator.Stub(validator => 
                validator.ValidatePageTypeAttributeTabProperty(Arg<PropertyInfo>.Is.NotNull, Arg<PageTypePropertyAttribute>.Is.NotNull));
            propertiesValidator.Replay();

            propertiesValidator.ValidatePageTypePropertyAttribute(propertyInfo);

            propertiesValidator.AssertWasCalled(validator => 
                validator.ValidatePageTypeAttributeTabProperty(propertyInfo,attribute));
        }

        [Fact]
        public void GivenAttributeWithTabPropertyNotSet_ValidatePageTypeAttributeTabProperty_DoesNotThrowException()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("StringTestProperty");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            PageTypeDefinitionPropertiesValidator propertiesValidator = new PageTypeDefinitionPropertiesValidator(null);


            Exception exception = Record.Exception(() =>
            {
                propertiesValidator.ValidatePageTypeAttributeTabProperty(propertyInfo, attribute);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenAttributeWithValidTabProperty_ValidatePageTypeAttributeTabProperty_DoesNotThrowException()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("PropertyWithValidTab");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            PageTypeDefinitionPropertiesValidator propertiesValidator = new PageTypeDefinitionPropertiesValidator(null);


            Exception exception = Record.Exception(() =>
            {
                propertiesValidator.ValidatePageTypeAttributeTabProperty(propertyInfo, attribute);
            });
            
            Assert.Null(exception);
        }

        [Fact]
        public void GivenAttributeWithTabPropertySetToTypeNotInheritingFromTab_ValidatePageTypeAttributeTabProperty_ThrowsException()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("PropertyWithTabSetToTypeNotInheritingFromTab");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            PageTypeDefinitionPropertiesValidator propertiesValidator = new PageTypeDefinitionPropertiesValidator(null);


            Exception exception = Record.Exception(() =>
            {
                propertiesValidator.ValidatePageTypeAttributeTabProperty(propertyInfo, attribute);
            });

            Assert.NotNull(exception);
        }

        [Fact]
        public void GivenAttributeWithTabPropertySetToAbstractSubClassOfTab_ValidatePageTypeAttributeTabProperty_ThrowsException()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("PropertyWithTabSetAbstractTabSubClass");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            PageTypeDefinitionPropertiesValidator propertiesValidator = new PageTypeDefinitionPropertiesValidator(null);


            Exception exception = Record.Exception(() =>
            {
                propertiesValidator.ValidatePageTypeAttributeTabProperty(propertyInfo, attribute);
            });

            Assert.NotNull(exception);
        }

        [Fact]
        public void GivenPropertyInfo_ValidatePageTypeProperty_ValidatesPageTypePropertyType()
        {
            PropertyInfo propertyInfo = CreateFakePropertyInfo();
            PageTypeDefinitionPropertiesValidator propertiesValidator =
                CreateValidatorWithFakeValidatePageTypePropertyMethodCalls(propertyInfo);

            propertiesValidator.ValidatePageTypeProperty(propertyInfo);

            propertiesValidator.AssertWasCalled(validator => validator.ValidatePageTypePropertyType(propertyInfo));
        }

        [Fact]
        public void GivenAttributeWithNoTypeSpecifiedAndOfMappableType_ValidatePageTypePropertyType_DoesNotThrowException()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("StringTestProperty");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            MockRepository fakes = new MockRepository();
            PageDefinitionTypeFactory pageDefinitionTypeFactory = fakes.Stub<PageDefinitionTypeFactory>();
            pageDefinitionTypeFactory.Stub(factory =>
                factory.GetPageDefinitionType("EPiServer.SpecializedProperties.PropertyXhtmlString", "EPiServer")).Return(new PageDefinitionType());
            pageDefinitionTypeFactory.Replay();
            PageTypeDefinitionPropertiesValidator propertiesValidator =
                new PageTypeDefinitionPropertiesValidator(new PageDefinitionTypeMapper(pageDefinitionTypeFactory));

            Exception exception = Record.Exception(() =>
            {
                propertiesValidator.ValidatePageTypePropertyType(propertyInfo);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenAttributeWithNoTypeSpecifiedAndOfUnmappableType_ValidatePageTypePropertyType_ThrowsException()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("PropertyWithInvalidTypeAndNoTypeSpecified");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            PageTypeDefinitionPropertiesValidator propertiesValidator = new PageTypeDefinitionPropertiesValidator(new PageDefinitionTypeMapper(null));

            Exception exception = Record.Exception(() =>
            {
                propertiesValidator.ValidatePageTypePropertyType(propertyInfo);
            });

            Assert.NotNull(exception);
            Assert.Equal<Type>(typeof(UnmappablePropertyTypeException), exception.GetType());
        }

        [Fact]
        public void GivenAttributeWithMappableTypeSpecified_ValidatePageTypePropertyType_DoesNotThrowException()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("PropertyWithValidTypeSpecified");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            MockRepository fakes = new MockRepository();
            PageDefinitionTypeFactory pageDefinitionTypeFactory = fakes.Stub<PageDefinitionTypeFactory>();
            pageDefinitionTypeFactory.Stub(factory =>
                factory.GetPageDefinitionType("EPiServer.SpecializedProperties.PropertyXhtmlString", "EPiServer")).Return(new PageDefinitionType());
            pageDefinitionTypeFactory.Replay();
            PageTypeDefinitionPropertiesValidator propertiesValidator =
                new PageTypeDefinitionPropertiesValidator(new PageDefinitionTypeMapper(pageDefinitionTypeFactory));

            Exception exception = Record.Exception(() =>
            {
                propertiesValidator.ValidatePageTypePropertyType(propertyInfo);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenAttributeWithMappableTypeAndUnmappableTypeSpecified_ValidatePageTypePropertyType_ThrowsException()
        {
            PropertyInfo propertyInfo = typeof(TestPageType).GetProperty("PropertyWithInvalidTypeSpecified");
            PageTypePropertyAttribute attribute = propertyInfo.GetCustomAttributes<PageTypePropertyAttribute>().First();
            MockRepository fakes = new MockRepository();
            PageDefinitionTypeFactory pageDefinitionTypeFactory = fakes.Stub<PageDefinitionTypeFactory>();
            pageDefinitionTypeFactory.Stub(factory =>
                factory.GetPageDefinitionType("System.Text.StringBuilder", "mscorlib")).Return(null);
            pageDefinitionTypeFactory.Replay();
            PageTypeDefinitionPropertiesValidator propertiesValidator = 
                new PageTypeDefinitionPropertiesValidator(new PageDefinitionTypeMapper(pageDefinitionTypeFactory));


            Exception exception = Record.Exception(() =>
            {
                propertiesValidator.ValidatePageTypePropertyType(propertyInfo);
            });

            Assert.NotNull(exception);
            Assert.Equal<Type>(typeof(UnmappablePropertyTypeException), exception.GetType());
        }

    }
}