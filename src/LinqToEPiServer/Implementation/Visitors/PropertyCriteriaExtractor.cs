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
        private static readonly MethodInfo QueryablePageDataWhere =
            MethodInfoHelper.MethodOf<IQueryable<PageData>>(q => q.Where(pd => true));

        private readonly PropertyCriteriaCollection _criteria;
        private readonly IList<IPropertyReferenceExtractor> _extractors;

        public PropertyCriteriaExtractor(IEnumerable<IPropertyReferenceExtractor> extractors)
        {
            if (extractors == null) throw new ArgumentNullException("extractors");
            _extractors = extractors.ToList();
            _criteria = new PropertyCriteriaCollection();
        }

        private bool IsFirstProcessedWhereClause
        {
            get { return _criteria.Count == 0; }
        }

        public PropertyCriteriaCollection ConvertToCriteria(Expression expression)
        {
            _criteria.Clear();
            Visit(expression);
            return _criteria;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            MethodInfo method = m.Method;

            if (!IsWhere(method))
            {
                throw new NotSupportedException(
                    string.Format("Method {0} is not supported. Try enumerating the result set with AsEnumerable first. Expression: {1}", method, m)
                    );
            }

            Expression queryable = m.Arguments[0];
            AddCriteriaFromWhereMethod(m);
            return Visit(queryable);
        }

        private static bool IsWhere(MethodInfo method)
        {
            return method.HasSameGenericMethodDefinitionAs(QueryablePageDataWhere);
        }

        private void AddCriteriaFromWhereMethod(MethodCallExpression m)
        {
            if (!IsFirstProcessedWhereClause)
                throw new NotSupportedException("Multiple where clauses are not supported");
            Expression predicate = m.Arguments[1];
            PropertyCriteriaCollection criteria = PredicateVisitor.ConvertToCriteriaCollection(predicate, _extractors);
            _criteria.AddRange(criteria);
        }
    }
}