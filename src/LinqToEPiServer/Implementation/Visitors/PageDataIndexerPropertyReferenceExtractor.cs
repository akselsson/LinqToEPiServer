using System;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer.Core;
using IQToolkit;
using LinqToEPiServer.Implementation.Expressions;
using PageTypeBuilder;

namespace LinqToEPiServer.Implementation.Visitors
{
    public interface IPropertyReferenceExtractor
    {
        PropertyReference GetPropertyReference(Expression expression);
        bool AppliesTo(Expression expression);
    }

    public abstract class MemberPropertyReferenceExtractorBase : IPropertyReferenceExtractor
    {
        public PropertyReference GetPropertyReference(Expression expression)
        {
            var e = MemberExpressionExtractor.GetFirstMemberExpression(expression);
            if (e == null) throw new InvalidOperationException(string.Format("Expression must be memberexpression, was {0}. Expression : {1}", expression.NodeType, expression));
            return GetPropertyReferencFromMember(e);
        }

        protected abstract PropertyReference GetPropertyReferencFromMember(MemberExpression e);

        public bool AppliesTo(Expression expression)
        {
            var e = MemberExpressionExtractor.GetFirstMemberExpression(expression);
            if (e == null)
                return false;
            return AppliesToMember(e);
        }

        protected abstract bool AppliesToMember(MemberExpression e);

        class MemberExpressionExtractor : ExpressionVisitor
        {
            private MemberExpression _firstMemberExpression;

            public static MemberExpression GetFirstMemberExpression(Expression e)
            {
                var extractor = new MemberExpressionExtractor();
                extractor.Visit(e);
                return extractor._firstMemberExpression;
            }

            protected override Expression VisitMemberAccess(MemberExpression m)
            {
                _firstMemberExpression = m;
                return m;
            }
        }
    }

    public class PageTypeBuilderProppertyReferenceExtractor : MemberPropertyReferenceExtractorBase
    {
        protected override PropertyReference GetPropertyReferencFromMember(MemberExpression e)
        {
            return new PropertyReference(e.Member.Name, e.Type);
        }

        protected override bool AppliesToMember(MemberExpression e)
        {
            return typeof(TypedPageData).IsAssignableFrom(e.Member.DeclaringType);
        }
    }

    public class PageDataMemberPropertyReferenceExtractor : MemberPropertyReferenceExtractorBase
    {
        protected override PropertyReference GetPropertyReferencFromMember(MemberExpression e)
        {
            string memberName = e.Member.Name;
            if (!memberName.StartsWith("Page"))
                memberName = "Page" + memberName;
            return new PropertyReference(memberName, e.Type);
        }

        protected override bool AppliesToMember(MemberExpression e)
        {
            return e.Member.DeclaringType == typeof(PageData);
        }
    }

    public class PageDataIndexerPropertyReferenceExtractor : ExpressionVisitor, IPropertyReferenceExtractor
    {
        private readonly MethodInfo _getValue = ReflectionHelper.MethodOf<PageData>(pd => pd.GetValue(""));
        private readonly MethodInfo _indexer = ReflectionHelper.MethodOf<PageData, object>(pd => pd[""]);

        private string _propertyName;
        private Type _type = typeof(object);

        private string PropertyName
        {
            get { return _propertyName; }
        }

        private Type Type
        {
            get { return _type; }
        }

        public PropertyReference GetPropertyReference(Expression expression)
        {
            Visit(expression);
            return new PropertyReference(PropertyName, Type);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method == _getValue)
                ExtractPropertyNameFromFirstArgument(m);
            else if (m.Method == _indexer)
                ExtractPropertyNameFromFirstArgument(m);
            else if(m.Method.DeclaringType == typeof(PageData))
                throw new NotSupportedException(string.Format("Could not get PageData property access. Not supported method {0} in {1}", m.Method, m));
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
            _propertyName = (string)ConstantValueExtractor.GetValue(m.Arguments[0]);
        }

        public bool AppliesTo(Expression expression)
        {
            Visit(expression);
            return PropertyName != null;
        }
    }
}