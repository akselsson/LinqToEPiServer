using System;
using System.Linq.Expressions;
using IQToolkit;

namespace LinqToEPiServer.Implementation.Visitors.Rewriters
{
    public class ExpressionRewriterBase : ExpressionVisitor, IExpressionRewriter
    {
        private bool _isRunning;

        #region IExpressionRewriter Members

        public virtual Expression Rewrite(Expression e)
        {
            if (_isRunning)
                throw new InvalidOperationException("Rewrite is not reentrant");
            _isRunning = true;
            Reset();
            Expression expression = Visit(e);
            _isRunning = false;
            return expression;
        }

        #endregion

        protected virtual void Reset()
        {
        }
    }
}