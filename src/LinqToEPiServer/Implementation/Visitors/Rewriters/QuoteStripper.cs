using System.Linq.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.Rewriters
{
    public class QuoteStripper : ExpressionRewriterBase
    {
        protected override Expression VisitUnary(UnaryExpression u)
        {
            if (u.NodeType == ExpressionType.Quote)
            {
                return base.Visit(u.Operand);
            }
            return base.VisitUnary(u);
        }
    }
}