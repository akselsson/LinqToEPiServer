using System;
using EPiServer;
using EPiServer.Core;

namespace LinqToEPiServer.Tests.Helpers
{
    public class ConsoleLoggingQueryExecutor : IQueryExecutor
    {
        private IQueryExecutor _inner;

        public ConsoleLoggingQueryExecutor(IQueryExecutor inner)
        {
            _inner = inner;
        }

        public PageDataCollection FindPagesWithCriteria(PageReference start, params PropertyCriteria[] criteria)
        {
            foreach (var criterion in criteria)
            {
                Console.WriteLine(EquatableCriteria.MakeEquatable(criterion));
            }
            return _inner.FindPagesWithCriteria(start, criteria);
        }
    }
}