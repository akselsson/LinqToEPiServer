using System;
using EPiServer.Core;

namespace LinqToEPiServer.Implementation.Expressions
{
    public class PropertyReference
    {
        public PropertyReference(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            PropertyName = name;
            MapTypeToPropertyDataType(type);
        }

        public PropertyReference(string name, PropertyDataType propertyDataType)
        {
            if (name == null) throw new ArgumentNullException("name");
            PropertyName = name;
            Type = propertyDataType;
        }

        public string PropertyName { get; private set; }
        public PropertyDataType? Type { get; private set; }

        public override string ToString()
        {
            return string.Format("PropertyName: {0}, Type: {1}", PropertyName, Type);
        }

        private void MapTypeToPropertyDataType(Type type)
        {
            if (type == typeof (object))
            {
                Type = null;
                return;
            }

            PropertyDataType propertyDataType;
            if (TypeToPropertyDataTypeMapper.TryMap(type, out propertyDataType))
            {
                Type = propertyDataType;
            }
            else
            {
                throw new NotSupportedException(string.Format("Can not map type {0} to PropertyDataType", type));
            }
        }
    }
}