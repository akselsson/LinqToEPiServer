using System;
using EPiServer;
using EPiServer.Core;
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

        public PropertyCriteria GetCriteria()
        {
            return new PropertyCriteria
            {
                Condition = CompareCondition,
                IsNull = ComparisonValueIsNull,
                Name = PropertyName,
                Required = true,
                Type = GetPropertyDataType(),
                Value = ComparisonValueString
            };
        }

        private string PropertyName
        {
            get { return Property.PropertyName; }
        }

        private bool ComparisonValueIsNull
        {
            get { return ComparisonValue == null; }
        }

        private string ComparisonValueString
        {
            get { return ComparisonValue != null ? Convert.ToString(ComparisonValue) : null; }
        }

        private PropertyDataType GetPropertyDataType()
        {
            return _property.Type ?? GetPropertyDataTypeFromComparisonValue();
        }

        private PropertyDataType GetPropertyDataTypeFromComparisonValue()
        {
            if (ComparisonValue == null)
                throw new InvalidOperationException("Can not get PropertyDataType from null ComparisonValue");

            PropertyDataType ret;
            if (TypeToPropertyDataTypeMapper.TryMap(ComparisonValue.GetType(), out ret))
                return ret;
            throw new NotSupportedException(String.Format("Unable to map {0} of type {1} to PropertyDataType",
                                                          ComparisonValue, ComparisonValue.GetType()));
        }


        
    }
}