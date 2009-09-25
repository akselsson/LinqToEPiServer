// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IQToolkit.Data.Common
{
    public static class Aggregator
    {
        /// <summary>
        /// Get a function that coerces a sequence of one type into another type.
        /// This is primarily used for aggregators stored in ProjectionExpression's, which are used to represent the 
        /// final transformation of the entire result set of a query.
        /// </summary>
        /// <param name="expectedType"></param>
        /// <param name="projector"></param>
        /// <returns></returns>
        public static LambdaExpression GetAggregator(Type expectedType, Type actualType)
        {
            //Type actualType = typeof(IEnumerable<>).MakeGenericType(elementType);
            Type actualElementType = TypeHelper.GetElementType(actualType);
            if (!expectedType.IsAssignableFrom(actualType))
            {
                Type expectedElementType = TypeHelper.GetElementType(expectedType);
                ParameterExpression p = Expression.Parameter(actualType, "p");
                Expression body = null;
                if (expectedType.IsAssignableFrom(actualElementType))
                {
                    body = Expression.Call(typeof(Enumerable), "SingleOrDefault", new Type[] { actualElementType }, p);
                }
                else if (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    body = Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { expectedElementType }, CoerceElement(expectedElementType, p));
                }
                else if (expectedType.IsArray && expectedType.GetArrayRank() == 1)
                {
                    body = Expression.Call(typeof(Enumerable), "ToArray", new Type[] { expectedElementType }, CoerceElement(expectedElementType, p));
                }
                else if (expectedType.IsAssignableFrom(typeof(List<>).MakeGenericType(actualElementType)))
                {
                    // List<T> can be assigned to expectedType
                    body = Expression.Call(typeof(Enumerable), "ToList", new Type[] { expectedElementType }, CoerceElement(expectedElementType, p));
                }
                else
                {
                    // some other collection type that has a constructor that takes IEnumerable<T>
                    ConstructorInfo ci = expectedType.GetConstructor(new Type[] { actualType });
                    if (ci != null)
                    {
                        body = Expression.New(ci, p);
                    }
                }
                if (body != null)
                {
                    return Expression.Lambda(body, p);
                }
            }
            return null;
        }

        private static Expression CoerceElement(Type expectedElementType, Expression expression)
        {
            Type elementType = TypeHelper.GetElementType(expression.Type);
            if (expectedElementType != elementType && (expectedElementType.IsAssignableFrom(elementType) || elementType.IsAssignableFrom(expectedElementType)))
            {
                return Expression.Call(typeof(Enumerable), "Cast", new Type[] { expectedElementType }, expression);
            }
            return expression;
        }
    }
}