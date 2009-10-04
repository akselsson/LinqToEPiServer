using System;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors
{
    public class PageDataIndexerPropertyReferenceExtractor : ExpressionVisitor, IPropertyReferenceExtractor
    {
        private readonly MethodInfo _getValue = MethodInfoHelper.MethodOf<PageData>(pd => pd.GetValue(""));
        private readonly MethodInfo _indexer = MethodInfoHelper.MethodOf<PageData, object>(pd => pd[""]);

        private string _propertyName;
        private Type _type;

        #region IPropertyReferenceExtractor Members

        public PropertyReference GetPropertyReference(Expression expression)
        {
            Reset();
            Visit(expression);
            return new PropertyReference(_propertyName, _type);
        }


        public bool AppliesTo(Expression expression)
        {
            Visit(expression);
            return _propertyName != null;
        }

        #endregion

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method == _getValue)
                ExtractPropertyNameFromFirstArgument(m);
            else if (m.Method == _indexer)
                ExtractPropertyNameFromFirstArgument(m);
            else if (m.Method.DeclaringType == typeof (PageData))
                throw new NotSupportedException(
                    string.Format("Could not get PageData property access. Not supported method {0} in {1}", m.Method, m));
            return m;
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.TypeAs:
                    ExtractType(u);
                    break;
            }
            return base.VisitUnary(u);
        }

        private void ExtractType(Expression expression)
        {
            _type = expression.Type;
        }

        private void ExtractPropertyNameFromFirstArgument(MethodCallExpression m)
        {
            _propertyName = (string) ConstantValueExtractor.GetValue(m.Arguments[0]);
        }

        private void Reset()
        {
            _propertyName = null;
            _type = typeof (object);
        }
    }
}