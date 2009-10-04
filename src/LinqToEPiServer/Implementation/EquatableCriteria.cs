using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;

namespace LinqToEPiServer.Tests.Helpers
{
    public class EquatableCriteria : PropertyCriteria
    {

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("{ Condition = ");
            builder.Append(Condition);
            builder.Append(", IsNull = ");
            builder.Append(IsNull);
            builder.Append(", Name = ");
            builder.Append(Name);
            builder.Append(", Required = ");
            builder.Append(Required);
            builder.Append(", Type = ");
            builder.Append(Type);
            builder.Append(", Value = ");
            builder.Append(Value);
            builder.Append(" }");
            return builder.ToString();
        }

        public override bool Equals(object value)
        {
            var type = value as EquatableCriteria;
            return (type != null) && EqualityComparer<CompareCondition>.Default.Equals(type.Condition, Condition) && EqualityComparer<bool>.Default.Equals(type.IsNull, IsNull) && EqualityComparer<string>.Default.Equals(type.Name, Name) && EqualityComparer<bool>.Default.Equals(type.Required, Required) && EqualityComparer<PropertyDataType>.Default.Equals(type.Type, Type) && EqualityComparer<string>.Default.Equals(type.Value, Value);
        }

        public override int GetHashCode()
        {
            int num = 0x7a2f0b42;
            num = (-1521134295*num) + EqualityComparer<CompareCondition>.Default.GetHashCode(Condition);
            num = (-1521134295*num) + EqualityComparer<bool>.Default.GetHashCode(IsNull);
            num = (-1521134295*num) + EqualityComparer<string>.Default.GetHashCode(Name);
            num = (-1521134295*num) + EqualityComparer<bool>.Default.GetHashCode(Required);
            num = (-1521134295*num) + EqualityComparer<PropertyDataType>.Default.GetHashCode(Type);
            return (-1521134295*num) + EqualityComparer<string>.Default.GetHashCode(Value);
        }

        public static EquatableCriteria[] MakeEquatable(IEnumerable<PropertyCriteria> criteria)
        {
            return criteria.Select(c =>MakeEquatable(c)).ToArray();
        }

        public static EquatableCriteria MakeEquatable(PropertyCriteria criteria)
        {
            return new EquatableCriteria
                {
                    Condition = criteria.Condition,
                    IsNull = criteria.IsNull,
                    Name = criteria.Name,
                    Required = criteria.Required,
                    Type = criteria.Type,
                    Value = criteria.Value
                };
        }
    }
}