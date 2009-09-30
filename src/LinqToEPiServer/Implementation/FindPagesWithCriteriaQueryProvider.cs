using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer.Implementation.Visitors;
using LinqToEPiServer.Implementation.Visitors.Rewriters;

namespace LinqToEPiServer.Implementation
{
    public class FindPagesWithCriteriaQueryProvider : QueryProvider
    {
        private readonly IList<IExpressionRewriter> _rewriters = new List<IExpressionRewriter>
                                                                     {
                                                                         new ComparisonFlipper(),
                                                                         new NegationFlattener(),
                                                                         new EmptySelectRemover(),
                                                                         new QuoteStripper(),
                                                                         new WhereCombiner()
                                                                     };

        private readonly PageReference _startPoint;
        private IQueryExecutor _executor;

        public FindPagesWithCriteriaQueryProvider(PageReference startPoint, IQueryExecutor executor)
        {
            _startPoint = startPoint;
            _executor = executor;
        }

        public IQueryExecutor Executor
        {
            get { return _executor; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _executor = value;
            }
        }

        public override object Execute(Expression expression)
        {
            PropertyCriteriaCollection criteria = GetCriteria(expression);
            return _executor.FindPagesWithCriteria(_startPoint, criteria.ToArray());
        }

        public override string GetQueryText(Expression expression)
        {
            PropertyCriteriaCollection criteria = GetCriteria(expression);
            //TODO: Criteria ToString
            return string.Join(", ", criteria.Select(c => c.ToString()).ToArray());
        }

        private PropertyCriteriaCollection GetCriteria(Expression expression)
        {
            Expression rewritten = Rewrite(expression);
            return PropertyCriteriaExtractor.GetCriteria(rewritten);
        }

        private Expression Rewrite(Expression expression)
        {
            Expression rewritten = expression;
            foreach (IExpressionRewriter rewriter in _rewriters)
            {
                rewritten = rewriter.Rewrite(rewritten);
            }
            return rewritten;
        }

        public void AddRewriter(IExpressionRewriter rewriter)
        {
            _rewriters.Add(rewriter);
        }
    }
}