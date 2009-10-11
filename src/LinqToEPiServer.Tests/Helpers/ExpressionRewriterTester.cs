using System;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer.Implementation.Visitors;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.Helpers
{
    class ExpressionRewriterTester
    {
        private readonly Expression _original;
        private readonly Expression _rewritten;

        public ExpressionRewriterTester(Expression expression, IExpressionRewriter transformer)
        {
            _original = expression;
            _rewritten = transformer.Rewrite(expression);
        }

        public void should_be_rewritten_to(Expression<Func<PageData, bool>> predicate)
        {
            should_be_rewritten_to((Expression)predicate);
        }

        private void should_be_rewritten_to(Expression expression)
        {
            Assert.AreEqual(expression.ToString(), _rewritten.ToString());
        }

        public void should_be_rewritten_to(IQueryable query)
        {
            should_be_rewritten_to(query.Expression);
        }


        public void should_not_be_rewritten()
        {
            Assert.AreEqual(_original.ToString(),_rewritten.ToString());
        }
    }
}