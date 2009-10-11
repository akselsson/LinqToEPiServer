using System.Linq;

namespace LinqToEPiServer.Tests.Helpers
{
    public static class QueryableExtensions
    {
        public static void Execute(this IQueryable queryable)
        {
            queryable.Provider.Execute(queryable.Expression);
        }
    }
}