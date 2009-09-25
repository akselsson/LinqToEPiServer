using System.Linq.Expressions;
using IQToolkit;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class ParameterSearcher : ExpressionVisitor
    {
        private bool _parameterWasFound;

        protected bool ParameterWasFound
        {
            get { return _parameterWasFound; }
        }

        public static bool ContainsParameter(Expression expression)
        {
            var searcher = new ParameterSearcher();
            searcher.Visit(expression);
            return searcher.ParameterWasFound;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            _parameterWasFound = true;
            return base.VisitParameter(p);
        }
    }
}