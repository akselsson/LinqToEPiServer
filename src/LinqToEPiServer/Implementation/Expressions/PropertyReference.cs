using System;
using System.Collections.Generic;
using EPiServer.Core;

namespace LinqToEPiServer.Implementation.Expressions
{
    public class PropertyReference
    {
        private static readonly Dictionary<Type, PropertyDataType> TypeMap
          = new Dictionary<Type, PropertyDataType>
                  {
                      {typeof (string), PropertyDataType.String},
                      {typeof (int), PropertyDataType.Number},
                      {typeof (int?), PropertyDataType.Number},
                      {typeof (DateTime), PropertyDataType.Date},
                      {typeof (DateTime?), PropertyDataType.Date},
                      {typeof (bool), PropertyDataType.Boolean},
                      {typeof (bool?), PropertyDataType.Boolean},
                      {typeof (double), PropertyDataType.FloatNumber},
                      {typeof (double?), PropertyDataType.FloatNumber},
                      {typeof (float), PropertyDataType.FloatNumber},
                      {typeof (float?), PropertyDataType.FloatNumber},
                      {typeof (PageReference), PropertyDataType.PageReference},
                  };

        public PropertyReference(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            PropertyName = name;
            ValueType = type;

            PropertyDataType pdt;
            if (TypeMap.TryGetValue(type, out pdt))
                Type = pdt;
        }

        public PropertyReference(string name, Type clrType, PropertyDataType propertyDataType)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (clrType == null) throw new ArgumentNullException("clrType");
            PropertyName = name;
            ValueType = clrType;
            Type = propertyDataType;
        }

        public string PropertyName { get; private set; }
        public Type ValueType { get; private set; }
        public PropertyDataType? Type { get; private set; }

        public override string ToString()
        {
            return string.Format("PropertyName: {0}, ValueType: {1}, Type: {2}", PropertyName, ValueType, Type);
        }
    }
}