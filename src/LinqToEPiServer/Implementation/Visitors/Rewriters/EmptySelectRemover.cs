using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TIgnored = System.Object;

namespace LinqToEPiServer.Implementation.Visitors.Rewriters
{
    public class EmptySelectRemover : ExpressionRewriterBase
    {
        private static readonly MethodInfo Select = MethodInfoHelper
            .MethodOf<IQueryable<TIgnored>>(o => o.Select(o1 => 1))
            .GetGenericMethodDefinition();

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (IsSelect(m))
            {
                return TryRewriteSelect(m);
            }
            return base.VisitMethodCall(m);
        }

        private static bool IsSelect(MethodCallExpression m)
        {
            return m.Method.HasSameGenericMethodDefinitionAs(Select);
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