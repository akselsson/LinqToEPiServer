using System;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class PageDataIndexerPropertyReferenceExtractor : ExpressionVisitor, IPropertyReferenceExtractor
    {
        private readonly MethodInfo _getValue = ReflectionHelper.MethodOf<PageData>(pd => pd.GetValue(""));
        private readonly MethodInfo _indexer = ReflectionHelper.MethodOf<PageData, object>(pd => pd[""]);

        private string _propertyName;
        private Type _type = typeof (object);

        private string PropertyName
        {
            get { return _propertyName; }
        }

        private Type Type
        {
            get { return _type; }
        }

        #region IPropertyReferenceExtractor Members

        public PropertyReference GetPropertyReference(Expression expression)
        {
            Visit(expression);
            return new PropertyReference(PropertyName, Type);
        }

        public bool AppliesTo(Expression expression)
        {
            Visit(expression);
            return PropertyName != null;
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
    }
}