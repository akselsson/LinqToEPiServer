using System;

namespace LinqToEPiServer.Implementation.Expressions
{
    public class PropertyReference
    {
        public PropertyReference(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            PropertyName = name;
            ValueType = type;
        }

        public string PropertyName { get; private set; }
        public Type ValueType { get; private set; }

        public override string ToString()
        {
            return String.Format("Property named {0} with type {1}", PropertyName, ValueType);
        }
    }
}