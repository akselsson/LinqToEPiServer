using System.Collections.Generic;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Synchronization;
using Rhino.Mocks;
using Xunit;

namespace PageTypeBuilder.Tests.Synchronization.PageTypeSynchronizerTests
{
    public class SynchronizePageTypesTests
    {
        [Fact]
        public void SynchronizePageTypes_CallsPageTypeDefinitionLocatorGetPageTypeDefinitions()
        {
            List<PageTypeDefinition> pageTypeDefinitions = new List<PageTypeDefinition>();
            PageTypeDefinitionLocator pageTypeDefinitionLocator = CreatePageTypeDefinitionLocatorStub(pageTypeDefinitions);
            PageTypeSynchronizer pageTypeSynchronizer = GetPageTypePartiallyMockedSynchronizer(pageTypeDefinitionLocator);

            pageTypeSynchronizer.SynchronizePageTypes();

            pageTypeDefinitionLocator.AssertWasCalled(locator => locator.GetPageTypeDefinitions(), options => options.Repeat.AtLeastOnce());
        }

        private PageTypeDefinitionLocator CreatePageTypeDefinitionLocatorStub(List<PageTypeDefinition> pageTypeDefinitions)
        {
            MockRepository fakes = new MockRepository();
            PageTypeDefinitionLocator definitionLocator = fakes.Stub<PageTypeDefinitionLocator>();
            definitionLocator.Stub(updater => updater.GetPageTypeDefinitions()).Return(pageTypeDefinitions);
            definitionLocator.Replay();

            return definitionLocator;
        }

        private PageTypeSynchronizer GetPageTypePartiallyMockedSynchronizer(PageTypeDefinitionLocator definitionLocator)
        {
            MockRepository fakes = new MockRepository();
            PageTypeSynchronizer pageTypeSynchronizer = fakes.PartialMock<PageTypeSynchronizer>(definitionLocator);
            pageTypeSynchronizer.Stub(synchronizer => synchronizer.UpdateTabDefinitions());
            pageTypeSynchronizer.Stub(synchronizer => synchronizer.ValidatePageTypeDefinitions(Arg<List<PageTypeDefinition>>.Is.Anything));
            pageTypeSynchronizer.Stub(synchronizer => synchronizer.CreateNonExistingPageTypes(Arg<List<PageTypeDefinition>>.Is.Anything));
            pageTypeSynchronizer.Stub(synchronizer => synchronizer.UpdatePageTypes(Arg<List<PageTypeDefinition>>.Is.Anything));
            pageTypeSynchronizer.Stub(synchronizer => synchronizer.UpdatePageTypePropertyDefinitions(Arg<List<PageTypeDefinition>>.Is.Anything));
            pageTypeSynchronizer.Replay();

            return pageTypeSynchronizer;
        }

        [Fact]
        public void SynchronizePageTypes_UpdatesTabs()
        {
            List<PageTypeDefinition> pageTypeDefinitions = new List<PageTypeDefinition>();
            PageTypeDefinitionLocator pageTypeDefinitionLocator = CreatePageTypeDefinitionLocatorStub(pageTypeDefinitions);
            PageTypeSynchronizer pageTypeSynchronizer = GetPageTypePartiallyMockedSynchronizer(pageTypeDefinitionLocator);

            pageTypeSynchronizer.SynchronizePageTypes();

            pageTypeSynchronizer.AssertWasCalled(synchronizer => synchronizer.UpdateTabDefinitions());
        }

        [Fact]
        public void SynchronizePageTypes_ValidatesPageTypeDefinitions()
        {
            List<PageTypeDefinition> pageTypeDefinitions = new List<PageTypeDefinition>();
            PageTypeDefinitionLocator pageTypeDefinitionLocator = CreatePageTypeDefinitionLocatorStub(pageTypeDefinitions);
            PageTypeSynchronizer pageTypeSynchronizer = GetPageTypePartiallyMockedSynchronizer(pageTypeDefinitionLocator);

            pageTypeSynchronizer.SynchronizePageTypes();

            pageTypeSynchronizer.AssertWasCalled(synchronizer => synchronizer.ValidatePageTypeDefinitions(pageTypeDefinitions));
        }

        [Fact]
        public void SynchronizePageTypes_CreatesNonExistingPageTypes()
        {
            List<PageTypeDefinition> pageTypeDefinitions = new List<PageTypeDefinition>();
            PageTypeDefinitionLocator pageTypeDefinitionLocator = CreatePageTypeDefinitionLocatorStub(pageTypeDefinitions);
            PageTypeSynchronizer pageTypeSynchronizer = GetPageTypePartiallyMockedSynchronizer(pageTypeDefinitionLocator);

            pageTypeSynchronizer.SynchronizePageTypes();

            pageTypeSynchronizer.AssertWasCalled(synchronizer => synchronizer.CreateNonExistingPageTypes(pageTypeDefinitions));
        }

        [Fact]
        public void SynchronizePageTypes_AddsPageTypesToResolver()
        {
            List<PageTypeDefinition> pageTypeDefinitions = new List<PageTypeDefinition>();
            PageTypeDefinitionLocator pageTypeDefinitionLocator = CreatePageTypeDefinitionLocatorStub(pageTypeDefinitions);
            PageTypeSynchronizer pageTypeSynchronizer = GetPageTypePartiallyMockedSynchronizer(pageTypeDefinitionLocator);

            pageTypeSynchronizer.SynchronizePageTypes();

            pageTypeSynchronizer.AssertWasCalled(synchronizer => synchronizer.AddPageTypesToResolver(pageTypeDefinitions));
        }

        [Fact]
        public void SynchronizePageTypes_UpdatesPageTypes()
        {
            List<PageTypeDefinition> pageTypeDefinitions = new List<PageTypeDefinition>();
            PageTypeDefinitionLocator pageTypeDefinitionLocator = CreatePageTypeDefinitionLocatorStub(pageTypeDefinitions);
            PageTypeSynchronizer pageTypeSynchronizer = GetPageTypePartiallyMockedSynchronizer(pageTypeDefinitionLocator);

            pageTypeSynchronizer.SynchronizePageTypes();

            pageTypeSynchronizer.AssertWasCalled(synchronizer => synchronizer.UpdatePageTypes(pageTypeDefinitions));
        }

        [Fact]
        public void SynchronizePageTypes_UpdatesPageTypePropertyDefinitionsForPageTypes()
        {
            List<PageTypeDefinition> pageTypeDefinitions = new List<PageTypeDefinition>();
            PageTypeDefinitionLocator pageTypeDefinitionLocator = CreatePageTypeDefinitionLocatorStub(pageTypeDefinitions);
            PageTypeSynchronizer pageTypeSynchronizer = GetPageTypePartiallyMockedSynchronizer(pageTypeDefinitionLocator);

            pageTypeSynchronizer.SynchronizePageTypes();

            pageTypeSynchronizer.AssertWasCalled(synchronizer => synchronizer.UpdatePageTypePropertyDefinitions(pageTypeDefinitions));
        }
    }
}
