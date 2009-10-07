using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer;
using IQToolkit;
using TIgnored = System.Object;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class PropertyCriteriaExtractor : ExpressionVisitor
    {
        private static readonly MethodInfo QueryableWhere = MethodInfoHelper
            .MethodOf<IQueryable<TIgnored>>(q => q.Where(o => true))
            .GetGenericMethodDefinition();

        private readonly PropertyCriteriaCollection _criteria;
        private readonly IList<IPropertyReferenceExtractor> _extractors;

        public PropertyCriteriaExtractor(IEnumerable<IPropertyReferenceExtractor> extractors)
        {
            if (extractors == null) throw new ArgumentNullException("extractors");
            _extractors = extractors.ToList();
            _criteria = new PropertyCriteriaCollection();
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

        private void AddCriteriaFromWhereMethod(MethodCallExpression m)
        {
            if (!IsFirstProcessedWhereClause)
                throw new NotSupportedException("Multiple where clauses are not supported");
            Expression predicate = m.Arguments[1];
            PropertyCriteriaCollection criteria = PredicateVisitor.ConvertToCriteriaCollection(predicate, _extractors);
            _criteria.AddRange(criteria);
        }

        private bool IsFirstProcessedWhereClause
        {
            get { return _criteria.Count == 0; }
        }

        private static bool IsWhere(MethodInfo method)
        {
            return method.HasSameGenericMethodDefinitionAs(QueryableWhere);
        }
    }
}