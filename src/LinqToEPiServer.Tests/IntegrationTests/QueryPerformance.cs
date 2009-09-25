using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Filters;
using LinqToEPiServer.Linq;
using LinqToEPiServer.Tests.Helpers;
using LinqToEPiServer.Tests.Model;
using NUnit.Framework;
using PageTypeBuilder;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    [Ignore]
    public class QueryPerformance : IntegrationTestsBase
    {
        private PageDataRepository _repository;
        private PageDataFactory _pageDataFactory;

        protected override void establish_context()
        {
            Initializer.Start();
            _repository = new PageDataRepository(new DataFactoryQueryExecutor());
            _pageDataFactory = new PageDataFactory();

        }

        public static void measure_query_time(string name, int targetCount, Func<PageDataCollection> generator)
        {
            Action action = () => Assert.AreEqual(targetCount, generator().Count);
            Warmup(action);

            CacheManager.Clear();
            action.MeasureTime(name + " Empty cache");

            Warmup(action);
            action.MeasureTime(name + " Cached");
        }

        private static void Warmup(Action action)
        {
            action();
        }

        private void add_pages(int rootCount, int childCount)
        {
            int currentChildNumber = 0;
            for (int i = 0; i < rootCount; i++)
            {
                var parent = _pageDataFactory.CreatePage<PerformanceTestPage>(PageReference.RootPage);
                parent.PageName = "parent_" + i;
                parent.Number = i%100;
                DataFactory.Instance.Save(parent, SaveAction.Publish);
                for (int j = 0; j < childCount; j++)
                {
                    var child = _pageDataFactory.CreatePage<PerformanceTestPage>(parent.PageLink);
                    child.PageName = "child_" + i;
                    child.Number = currentChildNumber%100;
                    child.Text = new string('a', 1024);
                    currentChildNumber++;
                    DataFactory.Instance.Save(child, SaveAction.Publish);
                }
            }
        }

        [Test, Combinatorial]
        public void find_x_pages_with_hit_percentage_y(
            [Values(100, 1000, 5000)] int pageCount,
            [Values(0.1, 1, 10)] double hitPercentage)
        {
            const int rootCount = 50;
            int childCount = pageCount/rootCount;
            int targetCount = Convert.ToInt32(pageCount*hitPercentage/100);

            Console.WriteLine("Find {2} ({1}%) of {0} pages", pageCount, hitPercentage, targetCount);
            TestHelpExtensions.MeasureTime(() => add_pages(rootCount, childCount), "Creation");

            var criterias = new PropertyCriteriaCollection
                                {
                                    {"PageName", "child", CompareCondition.StartsWith},
                                    new PropertyCriteria
                                        {
                                            Condition = CompareCondition.LessThan,
                                            Name = "Number",
                                            Required = true,
                                            Type = PropertyDataType.Number,
                                            Value = hitPercentage.ToString()
                                        }
                                };
            measure_query_time("FindPagesWithCriteria",
                               targetCount,
                               () => DataFactory.Instance.FindPagesWithCriteria(PageReference.RootPage, criterias));

            measure_query_time("Linq2EPiServer",
                               targetCount,
                               () => _repository.FindDescendantsOf(PageReference.RootPage)
                                         .Where(p => p.PageName.StartsWith("child") && (int) p["Number"] < (int)hitPercentage)
                                         .ToPageDataCollection());

            measure_query_time("Linq2Objects",
                               targetCount,
                               () => PageReference.RootPage
                                         .Descendants()
                                         .Where(p => p.PageName.StartsWith("child") && (int) p["Number"] < (int)hitPercentage)
                                         .ToPageDataCollection());
        }
    }
}