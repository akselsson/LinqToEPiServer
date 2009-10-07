using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToEPiServer.Implementation
{
    public static class MethodInfoHelper
    {
        public static MethodInfo MethodOf<T>(Expression<Action<T>> methodCall)
        {
            if (methodCall == null) throw new ArgumentNullException("methodCall");
            if(!(methodCall.Body is MethodCallExpression)) throw new ArgumentException("Expression must be a method call","methodCall");

            var expression = (MethodCallExpression)methodCall.Body;
            return expression.Method;
        }

        public static MethodInfo MethodOf<TTarget,TOutput>(Expression<Func<TTarget,TOutput>> methodCall)
        {
            if (methodCall == null) throw new ArgumentNullException("methodCall");
            if (!(methodCall.Body is MethodCallExpression)) throw new ArgumentException(string.Format("Expression must be a method call, was {0}", methodCall.NodeType), "methodCall");

            var expression = (MethodCallExpression)methodCall.Body;
            return expression.Method;
        }

        public static bool HasSameGenericMethodDefinitionAs(this MethodInfo first, MethodInfo second)
        {
            if (!first.IsGenericMethod)
                return first == second;
            if (!second.IsGenericMethod)
                return first == second;
            
            var firstDefinition = first.GetGenericMethodDefinition();
            var secondDefinition = second.GetGenericMethodDefinition();
            
            return firstDefinition == secondDefinition;
        }
    }
}