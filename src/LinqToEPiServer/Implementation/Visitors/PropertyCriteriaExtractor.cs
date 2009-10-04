using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer;
using EPiServer.Core;
using IQToolkit;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class PropertyCriteriaExtractor : ExpressionVisitor
    {
        private readonly IList<IPropertyReferenceExtractor> _extractors;
        private static readonly MethodInfo QueryablePageDataWhere = ReflectionHelper.MethodOf<IQueryable<PageData>>(q => q.Where(pd => true));

        private readonly PropertyCriteriaCollection _criteria = new PropertyCriteriaCollection();

        public PropertyCriteriaExtractor(IEnumerable<IPropertyReferenceExtractor> extractors)
        {
            if (extractors == null) throw new ArgumentNullException("extractors");
            _extractors = extractors.ToList();
        }

        protected PropertyCriteriaCollection Criteria
        {
            get { return _criteria; }
        }

        public PropertyCriteriaCollection GetCriteria(Expression expression)
        {
            Visit(expression);
            return Criteria;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            var method = m.Method;

            if (method.HasSameGenericMethodDefinitionAs(QueryablePageDataWhere))
            {
                AddCriteriaFromWhere(m);
                return Visit(m.Arguments[0]);
            }
            throw new NotSupportedException(
                string.Format(
                    "Method {0} is not supported. Try enumerating the result set with AsEnumerable first. Expression: {1}",
                    method, m)
                );
        }

        private void AddCriteriaFromWhere(MethodCallExpression m)
        {
            if (_criteria.Count > 0)
                throw new NotSupportedException("Multiple where clauses are not supported");
            var predicate = m.Arguments[1];
            var criterion = PredicateVisitor.ConvertToCriteriaCollection(predicate,_extractors);
            _criteria.AddRange(criterion);
        }
    }
}