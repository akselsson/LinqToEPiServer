﻿using System;
using System.Collections.Generic;
using EPiServer.DataAbstraction;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Synchronization;
using Rhino.Mocks;
using Xunit;

namespace PageTypeBuilder.Tests.Synchronization.PageTypePropertyUpdaterTests
{
    public class UpdatePageTypePropertyDefinitionsTests
    {
        [Fact]
        public void GivenPageType_UpdatePageTypePropertyDefinitions_CallsGetPageTypePropertyDefinitions()
        {
            List<PageTypePropertyDefinition> definitions = new List<PageTypePropertyDefinition>();
            PageTypePropertyUpdater pageTypePropertyUpdater = CreatePageTypePropertyUpdater(definitions);
            PageType pageType = new PageType();
            PageTypeDefinition pageTypeDefinition = new PageTypeDefinition();

            pageTypePropertyUpdater.UpdatePageTypePropertyDefinitions(pageType, pageTypeDefinition);

            pageTypePropertyUpdater.PageTypePropertyDefinitionLocator.AssertWasCalled(
                locator => locator.GetPageTypePropertyDefinitions(
                               pageType, pageTypeDefinition.Type));
        }

        private PageTypePropertyUpdater CreatePageTypePropertyUpdater(
            List<PageTypePropertyDefinition> definitionsToReturnFromGetPageTypePropertyDefinitions)
        {
            MockRepository fakes = new MockRepository();
            PageTypePropertyUpdater pageTypePropertyUpdater = fakes.PartialMock<PageTypePropertyUpdater>();
            PageTypePropertyDefinitionLocator definitionLocator = fakes.Stub<PageTypePropertyDefinitionLocator>();
            definitionLocator.Stub(
                locator => locator.GetPageTypePropertyDefinitions(
                               Arg<PageType>.Is.Anything, Arg<Type>.Is.Anything))
                .Return(definitionsToReturnFromGetPageTypePropertyDefinitions);
            definitionLocator.Replay();
            pageTypePropertyUpdater.Replay();
            pageTypePropertyUpdater.PageTypePropertyDefinitionLocator = definitionLocator;

            return pageTypePropertyUpdater;
        }

        [Fact]
        public void GivenPageType_UpdatePageTypePropertyDefinitions_CallsGetExistingPageDefinition()
        {
            List<PageTypePropertyDefinition> definitions = new List<PageTypePropertyDefinition>();
            PageTypePropertyDefinition pageTypePropertyDefinition = PageTypePropertyUpdaterTestsUtility.CreatePageTypePropertyDefinition();
            definitions.Add(pageTypePropertyDefinition);
            PageTypePropertyUpdater pageTypePropertyUpdater = CreatePageTypePropertyUpdater(definitions);
            PageType pageType = new PageType();
            PageTypeDefinition pageTypeDefinition = new PageTypeDefinition();
            pageTypePropertyUpdater.Stub(utility => utility.GetExistingPageDefition(
                                                        pageType, pageTypePropertyDefinition)).Return(new PageDefinition());
            pageTypePropertyUpdater.Stub(
                utility => utility.UpdatePageDefinition(
                               Arg<PageDefinition>.Is.Anything, Arg<PageTypePropertyDefinition>.Is.Anything));
            pageTypePropertyUpdater.Replay();

            pageTypePropertyUpdater.UpdatePageTypePropertyDefinitions(pageType, pageTypeDefinition);

            pageTypePropertyUpdater.AssertWasCalled(
                utility => utility.GetExistingPageDefition(
                               pageType, pageTypePropertyDefinition));
        }

        [Fact]
        public void GivenNoExistingPageDefinition_UpdatePageTypePropertyDefinitions_CallsCreateNewPageDefition()
        {
            List<PageTypePropertyDefinition> definitions = new List<PageTypePropertyDefinition>();
            PageTypePropertyDefinition pageTypePropertyDefinition = PageTypePropertyUpdaterTestsUtility.CreatePageTypePropertyDefinition();
            definitions.Add(pageTypePropertyDefinition);
            PageTypePropertyUpdater pageTypePropertyUpdater = CreatePageTypePropertyUpdater(definitions);
            PageType pageType = new PageType();
            PageTypeDefinition pageTypeDefinition = new PageTypeDefinition();
            pageTypePropertyUpdater.Stub(utility => utility.GetExistingPageDefition(
                                                        pageType, pageTypePropertyDefinition)).Return(null);
            pageTypePropertyUpdater.Stub(
                utility => utility.CreateNewPageDefition(pageTypePropertyDefinition)).Return(new PageDefinition());
            pageTypePropertyUpdater.Stub(
                utility => utility.UpdatePageDefinition(
                               Arg<PageDefinition>.Is.Anything, Arg<PageTypePropertyDefinition>.Is.Anything));
            pageTypePropertyUpdater.Replay();

            pageTypePropertyUpdater.UpdatePageTypePropertyDefinitions(pageType, pageTypeDefinition);

            pageTypePropertyUpdater.AssertWasCalled(
                utility => utility.CreateNewPageDefition(pageTypePropertyDefinition));
        }

        [Fact]
        public void GivenPageType_UpdatePageTypePropertyDefinitions_CallsUpdatePageDefinition()
        {
            List<PageTypePropertyDefinition> definitions = new List<PageTypePropertyDefinition>();
            PageTypePropertyDefinition pageTypePropertyDefinition = PageTypePropertyUpdaterTestsUtility.CreatePageTypePropertyDefinition();
            definitions.Add(pageTypePropertyDefinition);
            PageTypePropertyUpdater pageTypePropertyUpdater = CreatePageTypePropertyUpdater(definitions);
            PageType pageType = new PageType();
            PageTypeDefinition pageTypeDefinition = new PageTypeDefinition();
            PageDefinition existingPageDefinition = new PageDefinition();
            pageTypePropertyUpdater.Stub(utility => utility.GetExistingPageDefition(
                                                        pageType, pageTypePropertyDefinition)).Return(existingPageDefinition);
            pageTypePropertyUpdater.Stub(
                utility => utility.UpdatePageDefinition(
                               Arg<PageDefinition>.Is.Anything, Arg<PageTypePropertyDefinition>.Is.Anything));
            pageTypePropertyUpdater.Replay();

            pageTypePropertyUpdater.UpdatePageTypePropertyDefinitions(pageType, pageTypeDefinition);

            pageTypePropertyUpdater.AssertWasCalled(
                utility => utility.UpdatePageDefinition(
                               existingPageDefinition, pageTypePropertyDefinition));
        }
    }
}