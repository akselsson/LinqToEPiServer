using System.Linq.Expressions;

namespace LinqToEPiServer.Implementation.Visitors
{
    public interface IExpressionRewriter
    {
        Expression Rewrite(Expression e);
    }
}