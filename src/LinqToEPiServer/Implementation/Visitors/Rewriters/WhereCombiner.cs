using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer.Core;

namespace LinqToEPiServer.Implementation.Visitors.Rewriters
{
    public class WhereCombiner : ExpressionRewriterBase
    {
        private static readonly MethodInfo QueryableWhere =
            MethodInfoHelper.MethodOf<IQueryable<PageData>>(pd => pd.Where(x => true));

        private readonly QuoteStripper _quoteStripper = new QuoteStripper();

        private Expression NotModified(MethodCallExpression m)
        {
            return base.VisitMethodCall(m);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (!IsWhere(m.Method))
                return NotModified(m);

            return RewriteWhere(m);
        }

        private static bool IsWhere(MethodInfo method)
        {
            return method.HasSameGenericMethodDefinitionAs(QueryableWhere);
        }

        private Expression RewriteWhere(MethodCallExpression outerWhere)
        {
            var innerWhere = outerWhere.Arguments[0] as MethodCallExpression;

            if (innerWhere == null)
                return NotModified(outerWhere);

            if (!IsWhere(innerWhere.Method))
                return NotModified(outerWhere);

            Expression combined = Combine(outerWhere, innerWhere);
            return base.Visit(combined);
        }

        private Expression Combine(MethodCallExpression outerWhere, MethodCallExpression innerWhere)
        {
            var queryable = innerWhere.Arguments[0];
            Expression innerPredicate = ExtractPredicateBody(innerWhere);
            Expression outerPredicate = ExtractPredicateBody(outerWhere);
            ParameterExpression[] parameters = ExtractLambdaParameters(innerWhere);

            var combined = Expression.AndAlso(innerPredicate, outerPredicate);
            var rewrittenLambda = Expression.Lambda(combined, parameters);
            var rewrittenMethod = Expression.Call(outerWhere.Object, outerWhere.Method, queryable, rewrittenLambda);
            return rewrittenMethod;
        }

        private ParameterExpression[] ExtractLambdaParameters(MethodCallExpression innerWhere)
        {
            return ExtractLambda(innerWhere).Parameters.ToArray();
        }

        private Expression ExtractPredicateBody(MethodCallExpression whereExpression)
        {
            LambdaExpression predicate = ExtractLambda(whereExpression);
            return predicate.Body;
        }

        private LambdaExpression ExtractLambda(MethodCallExpression whereExpression)
        {
            Expression predicateExpression = whereExpression.Arguments[1];
            predicateExpression = StripQuotes(predicateExpression);
            var lambda = predicateExpression as LambdaExpression;
            if (lambda == null)
                throw new InvalidOperationException(string.Format("Expected Lambda was {0}, {1}",
                                                                  predicateExpression.NodeType, predicateExpression));
            return lambda;
        }

        private Expression StripQuotes(Expression argument)
        {
            return _quoteStripper.Rewrite(argument);
        }
    }
}