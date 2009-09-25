using System;
using System.Collections.Generic;
using System.Reflection;
using PageTypeBuilder.Discovery;
using Xunit;

namespace PageTypeBuilder.Tests.Discovery
{
    public class GetPageTypeDefinitionsTests
    {
        [Fact]
        public void GetPageTypeDefinition_ReturnsListOfNonAbstractTypesWithAttributeInApplicationDomain()
        {
            PageTypeDefinitionLocator definitionLocator = new PageTypeDefinitionLocator();

            List<PageTypeDefinition> definitions = definitionLocator.GetPageTypeDefinitions();

            Assert.Equal<int>(1, definitions.Count);
        }
    }
}