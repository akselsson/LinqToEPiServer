using System;
using System.Collections;
using System.Reflection;
using EPiServer.PlugIn;

namespace LinqToEPiServer.Tests.Helpers
{
    public static class Plugins
    {
        private const BindingFlags PrivateStaticField = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField;
        private static readonly FieldInfo CachedAssembliesField = typeof(PlugInLocator).GetField("_cachedAssemblies", PrivateStaticField);

        public static void StartAll()
        {
            foreach (var type in PlugInLocator.FindPlugInAttributes())
            {
                var start = type.GetMethod("Start", BindingFlags.Static | BindingFlags.Public, null, new Type[0], null);
                if (start != null)
                    start.Invoke(null, new object[0]);
            }
        }

        public static void LocateFromReferencedAssembly()
        {
            CachedAssembliesField.SetValue(null, FindAllPluginAssemblies());
        }

        private static Hashtable FindAllPluginAssemblies()
        {
            var ret = new Hashtable();
            foreach (var assemblyName in AllReferencedAssemblies)
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