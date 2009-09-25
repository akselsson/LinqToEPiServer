using System;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Visitors;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.Helpers
{
    class ExpressionTransformerTester
    {
        private readonly Expression _original;
        private readonly Expression _transformed;

        public ExpressionTransformerTester(Expression expression, IExpressionRewriter transformer)
        {
            _original = expression;
            _transformed = transformer.Rewrite(expression);
        }

        public void should_be_transformed_to(Expression<Func<PageData, bool>> predicate)
        {
            should_be_transformed_to((Expression)predicate);
        }

        private void should_be_transformed_to(Expression expression)
        {
            Assert.AreEqual(expression.ToString(), _transformed.ToString());
        }

        public void should_be_transformed_to(IQueryable query)
        {
            should_be_transformed_to(query.Expression);
        }


        public void should_not_be_transformed()
        {
            Assert.AreEqual(_original.ToString(),_transformed.ToString());
        }
    }
}