using System.Linq.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.Rewriters
{
    public class EmptySelectRemover : ExpressionRewriterBase
    {
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.Name == "Select")
            {
                return TryRewriteSelect(m);
            }
            return base.VisitMethodCall(m);
        }

        private Expression TryRewriteSelect(MethodCallExpression expression)
        {
            var selector = expression.Arguments[1] as UnaryExpression;
            if (selector == null)
                return base.VisitMethodCall(expression);

            var lambda = selector.Operand as LambdaExpression;
            if (lambda == null)
                return base.VisitMethodCall(expression);

            if (lambda.Body == lambda.Parameters[0])
                return base.Visit(expression.Arguments[0]);

            return base.VisitMethodCall(expression);
        }
    }
}