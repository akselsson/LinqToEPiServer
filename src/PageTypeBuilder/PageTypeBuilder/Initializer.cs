using EPiServer;
using EPiServer.PlugIn;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Synchronization;

namespace PageTypeBuilder
{
    public class Initializer : PlugInAttribute
    {
        private static object _lockObject = new object();
        private static bool _subscribtionsAreAdded;

        public static void Start()
        {
            lock (_lockObject)
            {
                SubscribeToEvents();
                SynchronizePageTypes();
                
            }
        }

        private static void SynchronizePageTypes()
        {
            PageTypeResolver.Instance = null;
            PageTypeSynchronizer synchronizer = new PageTypeSynchronizer(new PageTypeDefinitionLocator());
            synchronizer.SynchronizePageTypes();
        }

        private static void SubscribeToEvents()
        {
            if(_subscribtionsAreAdded)
                return;
            DataFactory.Instance.LoadedPage += DataFactory_LoadedPage;
            DataFactory.Instance.LoadedChildren += DataFactory_LoadedChildren;
            DataFactory.Instance.LoadedDefaultPageData += DataFactory_LoadedPage;
            _subscribtionsAreAdded = true;
        }

        static void DataFactory_LoadedPage(object sender, PageEventArgs e)
        {
            e.Page = PageTypeResolver.Instance.ConvertToTyped(e.Page);
        }

        static void DataFactory_LoadedChildren(object sender, ChildrenEventArgs e)
        {
            for (int i = 0; i < e.Children.Count; i++)
            {
                e.Children[i] = PageTypeResolver.Instance.ConvertToTyped(e.Children[i]);
            }
        }
    }
}