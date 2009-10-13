using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EPiServer.PlugIn;

namespace LinqToEPiServer.Tests.Helpers
{
    public static class Plugins
    {
        private const BindingFlags PrivateStaticField =
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField;

        private static readonly FieldInfo CachedAssembliesField =
            typeof (PlugInLocator).GetField("_cachedAssemblies", PrivateStaticField);

        public static void LocateFromReferencedAssembly()
        {
            SetPluginAssemblies(AllReferencedAssemblies);
        }

        public static void Reset()
        {
            SetPluginAssemblies(null);
        }

        private static void SetPluginAssemblies(IEnumerable<AssemblyName> assemblies)
        {
            CachedAssembliesField.SetValue(null, GetPluginHashtableFrom(assemblies));
        }

        private static Hashtable GetPluginHashtableFrom(IEnumerable<AssemblyName> assemblies)
        {
            var ret = new Hashtable();
            foreach (AssemblyName assemblyName in assemblies)
            {
                var assemblyInfo = new AssemblyTypeInfo(assemblyName);
                if (assemblyInfo.HasPlugInAttributes || assemblyInfo.HasPlugIns)
                {
                    ret.Add(assemblyName.Name, assemblyInfo);
                }
            }
            return ret;
        }

        private static AssemblyName[] AllReferencedAssemblies
        {
            get { return Assembly.GetExecutingAssembly().GetReferencedAssemblies(); }
        }
    }
}