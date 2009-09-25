using System;
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

        private static PropertyCriteriaCollection GetCriteria(Expression expression)
        {
            Expression rewritten = expression;
            rewritten = new ComparisonFlipper().Rewrite(rewritten);
            rewritten = new NegationFlattener().Rewrite(rewritten);
            rewritten = new EmptySelectRemover().Rewrite(rewritten);
            rewritten = new QuoteStripper().Rewrite(rewritten);
            rewritten = new WhereCombiner().Rewrite(rewritten);
            return PropertyCriteriaExtractor.GetCriteria(rewritten);
        }
    }
}