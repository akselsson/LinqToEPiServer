using System.Linq;
using System.Linq.Expressions;
using EPiServer.Core;
using IQToolkit;

namespace LinqToEPiServer.Implementation
{
    public class PageDataQuery : Query<PageData>
    {
        public PageDataQuery(IQueryProvider provider)
            : base(provider)
        {
        }

        public PageDataQuery(QueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }
    }
}