using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.Rewriters
{
    public class NegationFlattener : ExpressionRewriterBase
    {
        private readonly HashSet<Expression> _transformed = new HashSet<Expression>();
        private readonly ExpressionNegater _negater = new ExpressionNegater();

        protected override Expression VisitUnary(UnaryExpression u)
        {
            if (_transformed.Contains(u))
                return base.VisitUnary(u);

            if (u.NodeType != ExpressionType.Not)
                return base.VisitUnary(u);

            return Negate(u.Operand);
        }


        private Expression Negate(Expression expression)
        {
            Expression transformed = _negater.Rewrite(expression);
            _transformed.Add(transformed);
            return Visit(transformed);
        }


        protected override void Reset()
        {
            _transformed.Clear();
        }

        #region Nested type: ExpressionNegater

        private class ExpressionNegater : ShallowExpressionRewriterBase
        {
            private bool _expressionWasNegated;


            public override Expression Rewrite(Expression expression)
            {
                Expression transformed = base.Rewrite(expression);
                return _expressionWasNegated ? transformed : Expression.Not(transformed);
            }

            protected override void Reset()
            {
                base.Reset();
                _expressionWasNegated = false;
            }

            protected override Expression VisitUnary(UnaryExpression u)
            {
                if (u.NodeType == ExpressionType.Not)
                {
                    _expressionWasNegated = true;
                    return StopProcessing(u.Operand);
                }

                return StopProcessing(u);
            }

            protected override Expression VisitBinary(BinaryExpression b)
            {
                return StopProcessing(NegateBinary(b));
            }

            private Expression NegateBinary(BinaryExpression b)
            {
                _expressionWasNegated = true;
                switch (b.NodeType)
                {
                    case ExpressionType.AndAlso:
                        return Expression.OrElse(Expression.Not(b.Left), Expression.Not(b.Right));
                    case ExpressionType.OrElse:
                        return Expression.AndAlso(Expression.Not(b.Left), Expression.Not(b.Right));
                    case ExpressionType.Equal:
                        return Expression.NotEqual(b.Left, b.Right);
                    case ExpressionType.NotEqual:
                        return Expression.Equal(b.Left, b.Right);
                    case ExpressionType.LessThan:
                        return Expression.GreaterThanOrEqual(b.Left, b.Right);
                    case ExpressionType.GreaterThan:
                        return Expression.LessThanOrEqual(b.Left, b.Right);
                    case ExpressionType.LessThanOrEqual:
                        return Expression.GreaterThan(b.Left, b.Right);
                    case ExpressionType.GreaterThanOrEqual:
                        return Expression.LessThan(b.Left, b.Right);
                    default:
                        return Expression.Not(b);
                }
            }
        }

        #endregion
    }
}