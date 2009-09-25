using System;
using System.Linq.Expressions;
using IQToolkit;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class PageDataPropertyReferenceExtractor : ExpressionVisitor
    {
        private const string GetValue = "GetValue";
        private const string Indexer = "get_Item";
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

        public static PropertyReference GetPropertyReference(Expression expression)
        {
            var visitor = new PageDataPropertyReferenceExtractor();
            visitor.Visit(expression);
            return new PropertyReference(visitor.PropertyName, visitor.Type);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case Indexer:
                case GetValue:
                    ExtractPropertyNameFromFirstArgument(m);
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("Method call must be to get_Item, was {0}. Expression {1}", m.Method.Name, m));
            }
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
                default:
                    throw new NotSupportedException(
                        string.Format("Could not get PageData property access. Not supported operand type {0} in {1}",
                                      u.NodeType, u));
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

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            string memberName = m.Member.Name;
            if (!memberName.StartsWith("Page"))
                memberName = "Page" + memberName;
            _propertyName = memberName;
            _type = m.Type;
            return m;
        }
    }
}