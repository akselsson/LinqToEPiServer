using System;
using EPiServer;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Helpers;

namespace LinqToEPiServer.Tests.Helpers
{
    public class ConsoleLoggingQueryExecutor : IQueryExecutor
    {
        private readonly IQueryExecutor _inner;

        public ConsoleLoggingQueryExecutor(IQueryExecutor inner)
        {
            _inner = inner;
        }

        public PageDataCollection FindPagesWithCriteria(PageReference start, params PropertyCriteria[] criteria)
        {
            foreach (var criterion in criteria)
            {
                Console.WriteLine(criterion.EquatableWithFormatting());
            }
            return _inner.FindPagesWithCriteria(start, criteria);
        }
    }
}