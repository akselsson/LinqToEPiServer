using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using LinqToEPiServer.Implementation;
using LinqToEPiServer.Implementation.Helpers;
using LinqToEPiServer.Tests.Fakes;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.Helpers
{
    public static class AssertionExtensions
    {
        public static void should_return(this IQueryable<PageData> query, params PageReference[] pages)
        {
            int[] actual = query.AsEnumerable().Select(pd => pd.PageLink.ID).ToArray();
            int[] expected = pages.Select(pr => pr.ID).ToArray();
            CollectionAssert.AreEquivalent(expected, actual);
        }

        public static void should_be_translated_to(this IQueryable query, params PropertyCriteria[] expected)
        {
            PropertyCriteria[] actual = Translate(query);
            CollectionAssert.AreEqual(expected.EquatableWithFormatting(), actual.EquatableWithFormatting());
        }

        public static void should_be_equivalent_to(this IQueryable<PageData> query, IQueryable<PageData> expected)
        {
            PropertyCriteria[] translatedExpected = Translate(expected);
            query.should_be_translated_to(translatedExpected);
        }

        public static void should_not_be_supported(this IQueryable<PageData> query)
        {
            Assert.Throws<NotSupportedException>(() => query.Execute());
        }

        public static void should_not_be_supported(this IQueryable query)
        {
            Assert.Throws<NotSupportedException>(() => query.Execute());
        }

        private static PropertyCriteria[] Translate(IQueryable query)
        {
            Assert.IsInstanceOf<FindPagesWithCriteriaQueryProvider>(query.Provider);
            var executor = new StubQueryExecutor();
            
            var queryProvider = (FindPagesWithCriteriaQueryProvider)query.Provider;
            queryProvider.Executor = executor;
            query.Execute();
            return executor.Last;
        }
    }
}