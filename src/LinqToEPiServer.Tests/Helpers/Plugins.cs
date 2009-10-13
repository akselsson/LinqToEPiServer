using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EPiServer.PlugIn;

namespace LinqToEPiServer.Tests.Helpers
{
    public static class Plugins
    {
        private const BindingFlags PrivateStaticField = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField;
        private static readonly FieldInfo CachedAssembliesField = typeof(PlugInLocator).GetField("_cachedAssemblies", PrivateStaticField);

        public static void LocateFromReferencedAssembly()
        {
            CachedAssembliesField.SetValue(null, GetPluginAssembliesFrom(AllReferencedAssemblies));
        }

        public static void Reset()
        {
            CachedAssembliesField.SetValue(null,null);
        }

        private static Hashtable GetPluginAssembliesFrom(IEnumerable<AssemblyName> assemblies)
        {
            var ret = new Hashtable();
            foreach (var assemblyName in assemblies)
            {
                var assemblyInfo = new AssemblyTypeInfo(assemblyName);
                if (assemblyInfo.HasPlugInAttributes || assemblyInfo.HasPlugIns)
                {
                    ret.Add(assemblyName.Name, assemblyInfo);
                }
            }
            return ret;
        }

        static AssemblyName[] AllReferencedAssemblies
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            }
        }
    }
}