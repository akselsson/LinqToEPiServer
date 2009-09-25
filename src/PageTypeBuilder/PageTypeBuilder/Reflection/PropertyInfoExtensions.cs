﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PageTypeBuilder.Reflection
{
    public static class PropertyInfoExtensions
    {
        public static bool IsVirtual(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetGetMethod().IsVirtual && propertyInfo.GetSetMethod().IsVirtual;
        }

        public static bool GetterOrSetterIsCompilerGenerated(this PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            MethodInfo setMethod = propertyInfo.GetSetMethod();
            
            if(getMethod != null && getMethod.IsCompilerGenerated())
                return true;

            if(setMethod != null && setMethod.IsCompilerGenerated())
                return true;
            
            return false;
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this PropertyInfo propertyInfo)
        {
            object[] allAttributes = propertyInfo.GetCustomAttributes(typeof(T), false);
            
            List<T> attributes = new List<T>();
            foreach (T attribute in allAttributes)
            {
                attributes.Add(attribute);
            }
            
            return attributes;
        }
    }
}
