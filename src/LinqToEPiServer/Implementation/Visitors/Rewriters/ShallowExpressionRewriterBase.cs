using System.Linq.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.Rewriters
{
    public class ShallowExpressionRewriterBase : ExpressionRewriterBase
    {
        protected int _remainingTreeDepth;

        protected override void Reset()
        {
            _remainingTreeDepth = 1;
        }

        protected static Expression StopProcessing(Expression u)
        {
            return u;
        }

        protected override Expression Visit(Expression exp)
        {
            if (_remainingTreeDepth <= 0)
                return StopProcessing(exp);
            _remainingTreeDepth--;
            return base.Visit(exp);
        }
    }
}