using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer;
using EPiServer.Filters;
using IQToolkit;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class PredicateVisitor : ExpressionVisitor
    {
        private readonly IList<IPropertyReferenceExtractor> _extractors;
        private readonly PropertyCriteriaCollection _criteria = new PropertyCriteriaCollection();
        private readonly CriteriaFactory _criteriaFactory = new CriteriaFactory();

        private PredicateVisitor(IList<IPropertyReferenceExtractor> extractors)
        {
            if (extractors == null) throw new ArgumentNullException("extractors");
            _extractors = extractors;
        }

        protected PropertyCriteriaCollection Criteria
        {
            get { return _criteria; }
        }

        public static PropertyCriteriaCollection ConvertToCriteriaCollection(Expression expression, IList<IPropertyReferenceExtractor> extractors)
        {
            var visitor = new PredicateVisitor(extractors);
            return visitor.ConvertToCriteriaCollection(expression);
        }

        private PropertyCriteriaCollection ConvertToCriteriaCollection(Expression expression)
        {
            Visit(expression);
            return Criteria;
        }

        private IEnumerable<PropertyCriteriaCollection> ConvertAllToCriteriaCollections(IEnumerable<Expression> expressions)
        {
            return expressions.Select(e => new PredicateVisitor(_extractors).ConvertToCriteriaCollection(e)).ToList();
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (!ParameterSearcher.ContainsParameter(m.Object))
            {
                throw new NotSupportedException(
                    string.Format("Only methods on the query parameter are supported, was {0}", m));
            }
            switch (m.Method.Name)
            {
                case "Contains":
                    AddCriteriaForMethod(m, CompareCondition.Contained);
                    break;
                case "StartsWith":
                    AddCriteriaForMethod(m, CompareCondition.StartsWith);
                    break;
                case "EndsWith":
                    AddCriteriaForMethod(m, CompareCondition.EndsWith);
                    break;
                case "Equals":
                    return ConvertEqualsMethodToBinaryEquals(m);
                default:
                    throw new InvalidOperationException(
                        string.Format("Method {0} can not be mapped to a criteria. Expression: {1}", m.Method, m));
            }
            return m;
        }


        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Quote:
                    return base.VisitUnary(u);
                case ExpressionType.Convert:
                    return ConvertUnaryToBinaryEquals(u);
                case ExpressionType.Not:
                    _criteria.AddRange(ConvertNotExpressionToCriteriaCollection(u));
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unknown expression type {0} in expression {1}",
                                                                  u.NodeType, u));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    AddCriteriaForBinaryExpression(b, CompareCondition.Equal);
                    break;
                case ExpressionType.NotEqual:
                    AddCriteriaForBinaryExpression(b, CompareCondition.NotEqual);
                    break;
                case ExpressionType.LessThan:
                    AddCriteriaForBinaryExpression(b, CompareCondition.LessThan);
                    break;
                case ExpressionType.GreaterThan:
                    AddCriteriaForBinaryExpression(b, CompareCondition.GreaterThan);
                    break;
                case ExpressionType.AndAlso:
                    AddAnd(b.Left, b.Right);
                    break;
                case ExpressionType.OrElse:
                    AddOr(b.Left, b.Right);
                    break;
                default:
                    throw new NotSupportedException(string.Format("Not supported expressiontype: {0} in {1}",
                                                                  b.NodeType, b));
            }
            return b;
        }

        private Expression ConvertEqualsMethodToBinaryEquals(MethodCallExpression m)
        {
            if (m.Arguments.Count > 1)
                throw new NotSupportedException(
                    string.Format(
                        "Can not convert equals method in {0} to criteria condition. Try overload with one parameter", m));
            Expression left = m.Object;
            Expression right = m.Arguments[0];
            BinaryExpression binaryEquals = Expression.Equal(left, right);
            return VisitBinary(binaryEquals);
        }


        private PropertyCriteriaCollection ConvertNotExpressionToCriteriaCollection(UnaryExpression u)
        {
            if (u.NodeType != ExpressionType.Not)
                throw new InvalidOperationException(string.Format("Expression must be Not, was {0}", u));
            PropertyCriteriaCollection innerCriteria = ConvertToCriteriaCollection(u.Operand,_extractors);
            if (innerCriteria.Count > 1)
                throw new NotSupportedException(string.Format("Can not negate more than one criteria at once, was {0}",
                                                              u));
            foreach (PropertyCriteria criteria in innerCriteria)
            {
                NegateCriteria(criteria);
            }
            return innerCriteria;
        }

        private Expression ConvertUnaryToBinaryEquals(UnaryExpression u)
        {
            if (u.Type != typeof (bool))
            {
                throw new InvalidOperationException(
                    string.Format("Can not convert unary expression of type {0} to binary equals: {1}", u.Type, u));
            }

            Expression left = u;
            Expression right = Expression.Constant(true);
            BinaryExpression binaryEquals = Expression.Equal(left, right);
            return VisitBinary(binaryEquals);
        }

        private void NegateCriteria(PropertyCriteria criteria)
        {
            criteria.Condition = NegateCriteriaCondition(criteria.Condition);
        }

        private void AddAnd(params Expression[] expressions)
        {
            IEnumerable<PropertyCriteriaCollection> criterions = ConvertAllToCriteriaCollections(expressions);
            EnsureOnlyOneBranchContainsOr(criterions);
            AddAll(criterions);
        }

        private void AddOr(params Expression[] expressions)
        {
            IEnumerable<PropertyCriteriaCollection> criterions = ConvertAllToCriteriaCollections(expressions);
            EnsureOnlyOneBranchContainsOr(criterions);
            EnsureThatNoBranchContainsAnd(criterions);
            MakeOr(criterions);
            AddAll(criterions);
        }


        private void AddAll(IEnumerable<PropertyCriteriaCollection> collections)
        {
            foreach (PropertyCriteriaCollection criterion in collections)
            {
                _criteria.AddRange(criterion);
            }
        }

        private void EnsureOnlyOneBranchContainsOr(IEnumerable<PropertyCriteriaCollection> branches)
        {
            int criterionWithOr = branches.Count(c => c.Any(crit => crit.Required == false));
            if (criterionWithOr > 1)
                throw new NotSupportedException("Only one Or-branch is supported");
        }

        private void EnsureThatNoBranchContainsAnd(IEnumerable<PropertyCriteriaCollection> collection)
        {
            int branchesWithAnd = collection.Where(c => c.Count > 1).Count(c => c.Any(crit => crit.Required));
            if (branchesWithAnd > 0)
                throw new NotSupportedException("Can not nest And in Or-expressions");
        }

        private void MakeOr(IEnumerable<PropertyCriteriaCollection> criterions)
        {
            foreach (PropertyCriteriaCollection criterion in criterions)
            {
                MakeOr(criterion);
            }
        }

        private void MakeOr(PropertyCriteriaCollection collection)
        {
            foreach (PropertyCriteria criteria in collection)
            {
                criteria.Required = false;
            }
        }

        private static CompareCondition NegateCriteriaCondition(CompareCondition condition)
        {
            switch (condition)
            {
                case CompareCondition.Equal:
                    condition = CompareCondition.NotEqual;
                    break;
                case CompareCondition.NotEqual:
                    condition = CompareCondition.Equal;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Can not negate {0}", condition));
            }
            return condition;
        }


        private void AddCriteriaForMethod(MethodCallExpression comparisonMethod, CompareCondition condition)
        {
            object value = ConstantValueExtractor.GetValue(comparisonMethod.Arguments[0]);
            PropertyReference propertyAccess = GetPropertyReference(comparisonMethod.Object);
            var comparison = new PropertyComparison(propertyAccess, condition) {ComparisonValue = value};
            AddCriteria(comparison);
        }

        private PropertyReference GetPropertyReference(Expression param)
        {
            foreach (var extractor in _extractors)
            {
                if (extractor.AppliesTo(param))
                    return extractor.GetPropertyReference(param);
            }
            throw new NotSupportedException(string.Format("Parameter reference {0} is not supported", param));
        }

        private void AddCriteriaForBinaryExpression(BinaryExpression b, CompareCondition condition)
        {
            object value = ConstantValueExtractor.GetValue(b.Right);
            PropertyReference propertyAccess = GetPropertyReference(b.Left);
            var comparison = new PropertyComparison(propertyAccess, condition) {ComparisonValue = value};
            AddCriteria(comparison);
        }

        private void AddCriteria(PropertyComparison comparison)
        {
            PropertyCriteria criteria = _criteriaFactory.GetCriteria(comparison);
            _criteria.Add(criteria);
        }
    }
}