using System;
using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using LinqToEPiServer.Implementation.Expressions;
using LinqToEPiServer.Implementation.Visitors.PropertyDataTypeMappers;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class CriteriaFactory
    {
        private static readonly Dictionary<Type, Func<object, string>> ConverterMap
            = new Dictionary<Type, Func<object, string>>
                  {
                      {typeof (PageType), o => ((PageType) o).ID.ToString()}
                  };

        private static readonly IPropertyDataTypeMapper[] PropertyDataTypeMappers =
            new IPropertyDataTypeMapper[]
                {
                    new PropertyNameDataTypeMapper(),
                    new PropertyReferenceValueDataTypeMapper(),
                    new ComparisonValuePropertyDataTypeMapper()
                };

        public PropertyCriteria GetCriteria(PropertyComparison comparison)
        {
            return new PropertyCriteria
                       {
                           Condition = comparison.CompareCondition,
                           IsNull = comparison.ComparisonValue == null,
                           Name = comparison.PropertyName,
                           Required = true,
                           Type = GetDataType(comparison),
                           Value = ConvertToString(comparison.ComparisonValue)
                       };
        }

        private string ConvertToString(object value)
        {
            if (value == null)
                return null;
            Func<object, string> converter;
            if (ConverterMap.TryGetValue(value.GetType(), out converter))
            {
                return converter(value);
            }
            return Convert.ToString(value);
        }


        private static PropertyDataType GetDataType(PropertyComparison propertyComparison)
        {
            PropertyDataType type;
            foreach (IPropertyDataTypeMapper parser in PropertyDataTypeMappers)
            {
                if (parser.TryMap(propertyComparison, out type))
                {
                    return type;
                }
            }
            throw new NotSupportedException(String.Format("Can not find PropertyDataType for {0} or {1}:",
                                                          propertyComparison.Property,
                                                          propertyComparison.ComparisonValue));
        }
    }
}