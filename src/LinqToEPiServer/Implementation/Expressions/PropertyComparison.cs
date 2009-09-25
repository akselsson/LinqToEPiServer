using System;
using EPiServer.Filters;

namespace LinqToEPiServer.Implementation.Expressions
{
    public class PropertyComparison
    {
        private readonly CompareCondition _compareCondition;
        private readonly PropertyReference _property;

        public PropertyComparison(PropertyReference property, CompareCondition compareCondition)
        {
            if (property == null) throw new ArgumentNullException("property");
            _property = property;
            _compareCondition = compareCondition;
        }

        public CompareCondition CompareCondition
        {
            get { return _compareCondition; }
        }

        public PropertyReference Property
        {
            get { return _property; }
        }

        public object ComparisonValue { get; set; }

        public Type PropertyValueType
        {
            get { return Property.ValueType; }
        }

        public string PropertyName
        {
            get { return Property.PropertyName; }
        }
    }
}