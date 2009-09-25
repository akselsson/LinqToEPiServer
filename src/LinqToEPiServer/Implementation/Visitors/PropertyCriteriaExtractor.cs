using System;
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
        private static readonly MethodInfo QueryablePageDataWhere = ReflectionHelper.MethodOf<IQueryable<PageData>>(q => q.Where(pd => true));

        private readonly PropertyCriteriaCollection _criteria = new PropertyCriteriaCollection();

        protected PropertyCriteriaCollection Criteria
        {
            get { return _criteria; }
        }

        public static PropertyCriteriaCollection GetCriteria(Expression expression)
        {
            var visitor = new PropertyCriteriaExtractor();
            visitor.Visit(expression);
            return visitor.Criteria;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            
            if (m.Method == QueryablePageDataWhere)
            {
                AddCriteriaFromWhere(m);
                return Visit(m.Arguments[0]);
            }
            throw new NotSupportedException(
                string.Format(
                    "Method {0} is not supported. Try enumerating the result set with AsEnumerable first. Expression: {1}",
                    m.Method, m)
                );
        }

        private void AddCriteriaFromWhere(MethodCallExpression m)
        {
            if (_criteria.Count > 0)
                throw new NotSupportedException("Multiple where clauses are not supported");
            _criteria.AddRange(PredicateVisitor.ConvertToCriteriaCollection(m.Arguments[1]));
        }
    }
}