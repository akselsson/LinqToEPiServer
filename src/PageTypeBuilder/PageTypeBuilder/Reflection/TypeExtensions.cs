using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PageTypeBuilder.Reflection
{
    public static class TypeExtensions
    {
        public static PropertyInfo[] GetPublicOrPrivateProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static IEnumerable<Type> AssignableTo(this IEnumerable<Type> types, Type superType)
        {
            return types.Where(superType.IsAssignableFrom);
        }

        public static IEnumerable<Type> Concrete(this IEnumerable<Type> types)
        {
            return types.Where(type => !type.IsAbstract);
        }

        public static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        public static bool CanBeNull(this Type type)
        {
            return !type.IsValueType || type.IsNullableType();
        }
    }
}
