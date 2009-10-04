using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer.Implementation.Visitors;
using LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors;
using LinqToEPiServer.Implementation.Visitors.Rewriters;
using LinqToEPiServer.Tests.Helpers;

namespace LinqToEPiServer.Implementation
{
    public class FindPagesWithCriteriaQueryProvider : QueryProvider
    {
        private readonly IList<IPropertyReferenceExtractor> _propertyReferenceExtractors =
            new List<IPropertyReferenceExtractor>
                {
                    new PageDataMemberPropertyReferenceExtractor(),
                    new PageDataIndexerPropertyReferenceExtractor()
                };

        private readonly IList<IExpressionRewriter> _rewriters =
            new List<IExpressionRewriter>
                {
                    new ComparisonFlipper(),
                    new NegationFlattener(),
                    new EmptySelectRemover(),
                    new QuoteStripper(),
                    new WhereCombiner()
                };

        private readonly PageReference _startPoint;
        private readonly IList<IResultTransformer> _transformers = new List<IResultTransformer>();
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
            PageDataCollection result = FindPagesMatching(criteria);
            object transformed = TransformResult(result);
            return transformed;
        }

        private PageDataCollection FindPagesMatching(IEnumerable<PropertyCriteria> criteria)
        {
            return _executor.FindPagesWithCriteria(_startPoint, criteria.ToArray());
        }

        public override string GetQueryText(Expression expression)
        {
            var criteria = GetCriteria(expression);
            var equatableCriterias = EquatableCriteria.MakeEquatable(criteria);
            var criteriaStrings = equatableCriterias.Select(c => c.ToString()).ToArray();
            return string.Join(Environment.NewLine, criteriaStrings);
        }

        private PropertyCriteriaCollection GetCriteria(Expression expression)
        {
            Expression rewritten = RewriteExpression(expression);
            var extractor = new PropertyCriteriaExtractor(_propertyReferenceExtractors);
            return extractor.ConvertToCriteria(rewritten);
        }

        private Expression RewriteExpression(Expression expression)
        {
            Expression rewritten = expression;
            foreach (IExpressionRewriter rewriter in _rewriters)
            {
                rewritten = rewriter.Rewrite(rewritten);
            }
            return rewritten;
        }

        private object TransformResult(object result)
        {
            foreach (IResultTransformer transformer in _transformers)
            {
                result = transformer.Transform(result);
            }
            return result;
        }

        public void AddRewriter(IExpressionRewriter rewriter)
        {
            _rewriters.Add(rewriter);
        }

        public void AddPropertyReferenceExtractor(IPropertyReferenceExtractor extractor)
        {
            _propertyReferenceExtractors.Add(extractor);
        }

        public void AddResultTransformer(IResultTransformer transformer)
        {
            _transformers.Add(transformer);
        }

    }
}