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
    public interface IResultTransformerContainer
    {
        void AddResultTransformer(IResultTransformer transformer);
    }

    public class FindPagesWithCriteriaQueryProvider : QueryProvider, IResultTransformerContainer
    {
        private readonly IList<IExpressionRewriter> _rewriters = new List<IExpressionRewriter>
                                                                     {
                                                                         new ComparisonFlipper(),
                                                                         new NegationFlattener(),
                                                                         new EmptySelectRemover(),
                                                                         new QuoteStripper(),
                                                                         new WhereCombiner()
                                                                     };
        private readonly IList<IResultTransformer> _transformers = new List<IResultTransformer>();

        private readonly PageReference _startPoint;
        private IQueryExecutor _executor;
        private readonly IList<IPropertyReferenceExtractor> _propertyReferenceExtractors = new List<IPropertyReferenceExtractor>()
                                                                                      {
                                                                                          new PageDataMemberPropertyReferenceExtractor(),
                                                                                          new PageDataIndexerPropertyReferenceExtractor()
                                                                                      };

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
            var result = FindPagesMatching(criteria);
            var transformed = Transform(result);
            return transformed;
        }

        private PageDataCollection FindPagesMatching(IEnumerable<PropertyCriteria> criteria)
        {
            return _executor.FindPagesWithCriteria(_startPoint, criteria.ToArray());
        }

        private object Transform(object result)
        {
            foreach (var transformer in _transformers)
            {
                result = transformer.Transform(result);
            }
            return result;
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
            var extractor = new PropertyCriteriaExtractor(_propertyReferenceExtractors);
            return extractor.GetCriteria(rewritten);
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

        public void AddResultTransformer(IResultTransformer transformer)
        {
            _transformers.Add(transformer);
        }

        public void AddPropertyReferenceExtractor(IPropertyReferenceExtractor extractor)
        {
            _propertyReferenceExtractors.Add(extractor);
        }
    }
}