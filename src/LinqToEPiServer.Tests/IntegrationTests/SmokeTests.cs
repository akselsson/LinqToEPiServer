using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using NUnit.Framework;
using PageTypeBuilder;
using System;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    public class SmokeTests : IntegrationTestsBase
    {
        [Test]
        public void root_page_exists()
        {
            Assert.IsNotNull(DataFactory.Instance.GetPage(PageReference.RootPage));
        }

        [Test]
        public void root_page_has_one_child()
        {
            Assert.AreEqual(1, DataFactory.Instance.GetDescendents(PageReference.RootPage).Count);
        }

        [Test]
        public void it_is_possible_to_add_a_page()
        {
            var page = DataFactory.Instance.GetDefaultPageData(PageReference.RootPage,
                                                               PageType.List().First().ID);
            page.PageName = "test";
            DataFactory.Instance.Save(page, SaveAction.Publish);
            Assert.AreEqual("test", DataFactory.Instance.GetPage(page.PageLink).PageName);
        }

        [Test]
        public void it_is_possible_to_add_a_page2()
        {
            var page = DataFactory.Instance.GetDefaultPageData(PageReference.RootPage,
                                                               PageType.List().First().ID);
            page.PageName = "test";
            DataFactory.Instance.Save(page, SaveAction.Publish);
            Assert.AreEqual("test", DataFactory.Instance.GetPage(page.PageLink).PageName);
        }

        [Test]
        public void console_out()
        {
            Console.WriteLine("testar");
        }

        [Test]
        public void failing()
        {
            Assert.Fail("meddelande");
        }
    }
}