using System;
using System.Linq.Expressions;
using IQToolkit;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyReferenceExtractors
{
    public abstract class MemberPropertyReferenceExtractorBase : IPropertyReferenceExtractor
    {
        public PropertyReference GetPropertyReference(Expression expression)
        {
            MemberExpression e = MemberExpressionExtractor.GetFirstMemberExpression(expression);
            if (e == null)
                throw new InvalidOperationException(
                    string.Format("Expression must be memberexpression, was {0}. Expression : {1}", expression.NodeType,
                                  expression));
            return GetPropertyReferencFromMember(e);
        }

        public bool AppliesTo(Expression expression)
        {
            MemberExpression e = MemberExpressionExtractor.GetFirstMemberExpression(expression);
            if (e == null)
                return false;
            return AppliesToMember(e);
        }

        protected abstract bool AppliesToMember(MemberExpression e);
        protected abstract PropertyReference GetPropertyReferencFromMember(MemberExpression e);

        private class MemberExpressionExtractor : ExpressionVisitor
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
}