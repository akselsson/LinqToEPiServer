using System;
using System.Linq.Expressions;
using IQToolkit;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class ConstantValueExtractor
    {
        public static object GetValue(Expression e)
        {
            var parameterAsConstant = e as ConstantExpression;
            ConstantExpression expression = parameterAsConstant ?? TryEvaluateToConstant(e);
            if (expression == null)
            {
                throw new NotSupportedException("Expression must be constant, was " + e);
            }
            return expression.Value;
        }

        private static ConstantExpression TryEvaluateToConstant(Expression e)
        {
            return PartialEvaluator.Eval(e) as ConstantExpression;
        }
    }
}