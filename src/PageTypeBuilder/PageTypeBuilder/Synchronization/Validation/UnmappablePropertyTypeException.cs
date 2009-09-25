using System;

namespace PageTypeBuilder.Synchronization.Validation
{
    public class UnmappablePropertyTypeException : Exception
    {
        public UnmappablePropertyTypeException(string message) : base(message)
        {
        }
    }
}
