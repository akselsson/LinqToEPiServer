using System.Reflection;
using PageTypeBuilder;
using PageTypeBuilder.Configuration;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Synchronization;

namespace LinqToEPiServer.Tests.Helpers
{
    public class PageTypeBuilderHelper
    {
        private const BindingFlags InternalMethod = BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance;

        private static readonly PropertyInfo PageTypeResolverInstanceProperty =
            typeof(PageTypeResolver).GetProperty("Instance");

        private static readonly MethodInfo SynchronzePageTypesMethod =
            typeof(PageTypeSynchronizer).GetMethod("SynchronizePageTypes", InternalMethod);

        private static bool _firstInit = true;

        public static void Init()
        {
            // Hack: Internal property needs to be reset
            PageTypeResolverInstanceProperty.SetValue(null, null, null);
            Initializer.Start();
            if (!_firstInit)
            {
                var synchronizer = new PageTypeSynchronizer(new PageTypeDefinitionLocator(),
                                                            new PageTypeBuilderConfiguration());
                // HACK: Requried because SynchronizePageTypes is internal.
                SynchronzePageTypesMethod.Invoke(synchronizer, new object[0]);
            }
            _firstInit = false;
        }
    }
}