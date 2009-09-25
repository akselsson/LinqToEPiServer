using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToEPiServer.Implementation
{
    public class ReflectionHelper
    {
        public static MethodInfo MethodOf<T>(Expression<Action<T>> methodCall)
        {
            if (methodCall == null) throw new ArgumentNullException("methodCall");
            if(!(methodCall.Body is MethodCallExpression)) throw new ArgumentException("Expression must be a method call","methodCall");

            var expression = (MethodCallExpression)methodCall.Body;
            return expression.Method;
        }
    }
}