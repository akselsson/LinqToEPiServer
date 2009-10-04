using System;
using System.Linq;
using System.Reflection;
using EPiServer.Core;
using LinqToEPiServer.Implementation;
using NUnit.Framework;
using PageTypeBuilder;

namespace LinqToEPiServer.Tests.UnitTests
{
    public class ReflectionHelperSpec : EPiTestBase
    {
        [Test]
        public void should_get_indexer()
        {
            var indexer = MethodInfoHelper.MethodOf<PageData,object>(pd=>pd["test"]);
            Assert.AreEqual("get_Item",indexer.Name);
        }

        [Test]
        public void should_get_method()
        {
            var method = MethodInfoHelper.MethodOf<PageData>(pd => pd.GetValue("test"));
            Assert.AreEqual("GetValue",method.Name);
        }

        [Test]
        public void where_pagedata_should_have_same_definition_as_where_typedpagedata()
        {
            var wherePageData = MethodInfoHelper.MethodOf<IQueryable<PageData>>(q => q.Where(pd => true));
            var whereBasePage = MethodInfoHelper.MethodOf<IQueryable<TypedPageData>>(q => q.Where(pd => true));

            Assert.IsTrue(whereBasePage.HasSameGenericMethodDefinitionAs(wherePageData));
        }

        [Test]
        public void where_pagedata_should_have_same_definition_as_where_definition()
        {
            var wherePageData = MethodInfoHelper.MethodOf<IQueryable<PageData>>(q => q.Where(pd => true));
            var genericMethodDefinition = wherePageData.GetGenericMethodDefinition();

            Assert.IsTrue(genericMethodDefinition.HasSameGenericMethodDefinitionAs(wherePageData));
        }

        [Test]
        public void where_definition_should_have_same_definition_as_where_page_data()
        {
            var wherePageData = MethodInfoHelper.MethodOf<IQueryable<PageData>>(q => q.Where(pd => true));
            var genericMethodDefinition = wherePageData.GetGenericMethodDefinition();

            Assert.IsTrue(genericMethodDefinition.HasSameGenericMethodDefinitionAs(wherePageData));
        }

        [Test]
        public void non_generic_method_should_have_same_definition_as_itself()
        {
            var method = MethodInfoHelper.MethodOf<string>(s => s.Equals("a"));
            Assert.IsTrue(method.HasSameGenericMethodDefinitionAs(method));
        }
    }
}
