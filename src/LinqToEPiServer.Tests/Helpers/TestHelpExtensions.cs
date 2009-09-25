using System;
using System.Collections.Generic;
using System.Diagnostics;
using EPiServer;
using EPiServer.Core;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.Helpers
{
    public static class TestHelpExtensions
    {
        public static IEnumerable<PageData> Descendants(this PageReference root)
        {
            foreach (var pageReference in DataFactory.Instance.GetDescendents(root))
            {
                yield return DataFactory.Instance.GetPage(pageReference);
            }
        }

        public static void MeasureTime(this Action action, string description)
        {
            Console.Write(description);
            Console.Write(": ");
            var timer = Stopwatch.StartNew();
            action();
            Console.WriteLine(timer.Elapsed);
        }
    }
}