using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using EPiServer;
using EPiServer.BaseLibrary;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Globalization;
using EPiServer.Implementation;
using LinqToEPiServer.Tests.Fakes;

namespace LinqToEPiServer.Tests.Helpers
{
    public class EPiTester
    {
        static EPiTester()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEpiServerAssembly;
        }

        public EPiTester()
        {
            Cache = new InMemoryCache();
            var section = new TestEPiServerSection();
            section.AddSite(new SiteElement {SiteId = "test"});
            _epiServerSection = section;
        }

        private readonly Configuration _configuration =
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        private string _connectionString = @"Data Source=.;Initial Catalog=dbR2ExampleSite;Integrated Security=true";
        private readonly EPiServerSection _epiServerSection;

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public IRuntimeCache Cache
        {
            get { return RuntimeCache.Inner; }
            set { RuntimeCache.Inner = value; }
        }

        public void Init()
        {
            _configuration.Sections.Remove("episerver");
            _configuration.Sections.Add("episerver", _epiServerSection);
            EPiServerSection.CurrentConfiguration = _configuration;
            Settings.InitializeAllSettings();
            Settings.Instance = EPiServerSection.Instance.Sites[0].SiteSettings;

            Context.Current = new NullContext();
            ContentLanguage.PreferredCulture = CultureInfo.CurrentCulture;
            DataAccessBase.DatabaseFactory =
                new SqlDatabaseFactory(new ConnectionStringSettings("EPiServerDB", _connectionString,
                                                                    "System.Data.SqlClient"));

            ClassFactory.Instance = new DefaultBaseLibraryFactory("test");
            ClassFactory.Instance.RegisterClass(typeof (IRuntimeCache), typeof (RuntimeCache));

            DataFactoryCache.Initialize(DataFactory.Instance);
            CacheManager.Clear();
        }

        public void Shutdown()
        {
        }

        private static Assembly ResolveEpiServerAssembly(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("EPiServer, "))
            {
                return typeof (DataFactory).Assembly;
            }
            return null;
        }

        public static void SetDefaultProvider(PageProviderBase provider)
        {
            provider.Initialize("default", new NameValueCollection());
            DataFactory.Instance.ProviderMap.RemovePageProvider("");
            DataFactory.Instance.ProviderMap.AddPageProvider(provider);
        }
    }
}