using System;
using System.Linq.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.Rewriters
{
    /// <summary>
    /// Rewrites comparisons so that the parameter branch is on the left side of the binary expression
    /// </summary>
    public class ComparisonFlipper : ExpressionRewriterBase
    {
        protected override void Reset()
        {
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (ParameterSearcher.ContainsParameter(b.Left))
            {
                return base.VisitBinary(b);
            }
            if (ParameterSearcher.ContainsParameter(b.Right))
            {
                BinaryExpression flipped = Flip(b);
                return base.VisitBinary(flipped);
            }
            throw new NotSupportedException(string.Format("At least one branch must contain parmeter, was {0}", b));
        }

        private static BinaryExpression Flip(BinaryExpression expression)
        {
            ExpressionType flippedType = GetFlippedNodeType(expression);
            return Expression.MakeBinary(
                flippedType,
                expression.Right,
                expression.Left,
                expression.IsLiftedToNull,
                expression.Method);
        }

        private static ExpressionType GetFlippedNodeType(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    return ExpressionType.Equal;
                case ExpressionType.NotEqual:
                    return ExpressionType.NotEqual;
                case ExpressionType.LessThan:
                    return ExpressionType.GreaterThan;
                case ExpressionType.GreaterThan:
                    return ExpressionType.LessThan;
                default:
                    throw new NotSupportedException(
                        string.Format(
                            "Can not flip expressions of type {0}. Ensure that parameter branch is on the left side of the comparison for expression {1}",
                            expression.NodeType, expression));
            }
        }
    }
}