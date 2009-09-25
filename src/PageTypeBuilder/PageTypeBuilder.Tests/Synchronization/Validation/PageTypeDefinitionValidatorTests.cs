using System;
using System.Collections.Generic;
using EPiServer.Core;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Synchronization;
using PageTypeBuilder.Synchronization.Validation;
using Rhino.Mocks;
using Xunit;

namespace PageTypeBuilder.Tests.Synchronization.Validation
{
    public class PageTypeDefinitionValidatorTests
    {
        [Fact]
        public void Constructor_SetsPropertiesValidtorProperty()
        {
            PageTypeDefinitionValidator pageTypeValidator = new PageTypeDefinitionValidator(null);

            Assert.NotNull(pageTypeValidator.PropertiesValidator);
        }

        [Fact]
        public void Constructor_SetsPageDefinitionTypeMapperProperty()
        {
            PageDefinitionTypeMapper mapper = new PageDefinitionTypeMapper(null);

            PageTypeDefinitionValidator pageTypeValidator = new PageTypeDefinitionValidator(mapper);

            Assert.NotNull(pageTypeValidator.PropertiesValidator.PageDefinitionTypeMapper);
        }

        [Fact]
        public void GivenListOfPageTypeDefinitions_ValidatePageTypeDefinitions_ValidatesPageTypesHaveGuidOrUniqueNamee()
        {
            MockRepository fakes = new MockRepository();
            List<PageTypeDefinition> definitions = new List<PageTypeDefinition>();
            PageTypeDefinition definition = new PageTypeDefinition
            {
                Type = typeof(TypedPageData),
                Attribute = new PageTypeAttribute()
            };
            definitions.Add(definition);
            PageTypeDefinitionValidator definitionValidator = fakes.PartialMock<PageTypeDefinitionValidator>((PageDefinitionTypeMapper)null);
            definitionValidator.Stub(validator => validator.ValidatePageTypesHaveGuidOrUniqueName(definitions));
            definitionValidator.Replay();

            definitionValidator.ValidatePageTypeDefinitions(definitions);

            definitionValidator.AssertWasCalled(validator => validator.ValidatePageTypesHaveGuidOrUniqueName(definitions));
        }

        [Fact]
        public void GivenListOfPageTypeDefinitions_ValidatePageTypeDefinitions_ValidatesEachPageType()
        {
            MockRepository fakes = new MockRepository();
            List<PageTypeDefinition> definitions = new List<PageTypeDefinition>();
            PageTypeDefinition definition = new PageTypeDefinition
            {
                Type = typeof(TypedPageData),
                Attribute = new PageTypeAttribute()
            };
            definitions.Add(definition);
            PageTypeDefinitionValidator definitionValidator = fakes.PartialMock<PageTypeDefinitionValidator>((PageDefinitionTypeMapper)null);
            definitionValidator.Stub(validator => validator.ValidatePageTypeDefinition(definition));
            definitionValidator.Replay();

            definitionValidator.ValidatePageTypeDefinitions(definitions);

            definitionValidator.AssertWasCalled(validator => validator.ValidatePageTypeDefinition(definition));
        }

        [Fact]
        public void GivenDefinitionWithTypeThatDoesNotInheritFromTypedPageData_ValidatePageTypeDefinitions_ThrowsApplicationException()
        {
            List<PageTypeDefinition> definitions = new List<PageTypeDefinition>();
            PageTypeDefinition invalidTypeDefinition = new PageTypeDefinition
            {
                Type = typeof(PageData),
                Attribute = new PageTypeAttribute()
            };
            definitions.Add(invalidTypeDefinition);
            PageTypeDefinitionValidator definitionValidator = new PageTypeDefinitionValidator(null);

            Exception exception =
                Record.Exception(
                    () => definitionValidator.ValidatePageTypeDefinitions(definitions));

            Assert.NotNull(exception);
            Type exceptionType = exception.GetType();
            Assert.Equal<Type>(typeof(ApplicationException), exceptionType);
        }

        [Fact]
        public void ValidatePageTypeDefinitions_ValidatesProperties()
        {
            PageTypeDefinition pageTypeDefinition = new PageTypeDefinition();
            MockRepository fakes = new MockRepository();
            PageTypeDefinitionValidator pageTypeValidator = fakes.PartialMock<PageTypeDefinitionValidator>((PageDefinitionTypeMapper)null);
            pageTypeValidator.Stub(validator => validator.ValidateInheritsFromBasePageType(pageTypeDefinition));
            pageTypeValidator.PropertiesValidator = fakes.Stub<PageTypeDefinitionPropertiesValidator>((PageDefinitionTypeMapper)null);
            pageTypeValidator.PropertiesValidator.Stub(validator => validator.ValidatePageTypeProperties(pageTypeDefinition));
            pageTypeValidator.PropertiesValidator.Replay();

            pageTypeValidator.ValidatePageTypeDefinition(pageTypeDefinition);

            pageTypeValidator.PropertiesValidator.AssertWasCalled(validator => validator.ValidatePageTypeProperties(pageTypeDefinition));
        }

        [Fact]
        public void GivenTypesThatInheritFromTypedPageData_ValidatePageTypeDefinitions_DoesNotThrowException()
        {
            List<PageTypeDefinition> definitions = new List<PageTypeDefinition>();
            PageTypeDefinition validTypeDefinition = new PageTypeDefinition
            {
                Type = typeof(TypedPageData),
                Attribute = new PageTypeAttribute()
            };
            definitions.Add(validTypeDefinition);
            PageTypeDefinitionValidator definitionValidator = new PageTypeDefinitionValidator(null);

            definitionValidator.ValidatePageTypeDefinitions(definitions);
        }

        [Fact]
        public void GivenTwoPageTypesWithSameNameAndNoGuid_ValidatePageTypeDefinitions_ThrowsException()
        {
            List<PageTypeDefinition> definitions = new List<PageTypeDefinition>();
            PageTypeDefinition validTypeDefinition = new PageTypeDefinition
            {
                Type = typeof(TypedPageData),
                Attribute = new PageTypeAttribute()
            };
            definitions.Add(validTypeDefinition);
            definitions.Add(validTypeDefinition);
            PageTypeDefinitionValidator definitionValidator = new PageTypeDefinitionValidator(null);

            Exception exception = Record.Exception(() => definitionValidator.ValidatePageTypeDefinitions(definitions));

            Assert.NotNull(exception);
        }
    }
}
