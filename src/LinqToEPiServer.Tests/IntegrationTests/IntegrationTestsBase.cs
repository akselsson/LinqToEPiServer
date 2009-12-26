using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Transactions;
using EPiServer.PlugIn;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;
using PageTypeBuilder;
using PageTypeBuilder.Configuration;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Synchronization;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    [TestFixture]
    public class IntegrationTestsBase : SpecRequiringEPiRuntime
    {
        private TransactionScope _transaction;
        private IPrincipal _originalPrincipal;
        private static bool _pageTypeBuilderInitialisedOnce = false;

        protected override void setup_epi(EPiTester context)
        {
            context.ConnectionString = IntegrationTestDatabase.ConnectionString;
            base.setup_epi(context);
        }

        protected override void before_each_test()
        {
            base.before_each_test();
            _transaction = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes(20));
            _originalPrincipal = Thread.CurrentPrincipal;
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("admin"), new[] {"administrators"});
            EnableCustomPropertyTypes();
            InitPageTypeBuilder();
        }

        private void EnableCustomPropertyTypes()
        {
            Plugins.LocateFromReferencedAssembly();
            PageDefinitionTypePlugInAttribute.Start();
        }

        private void InitPageTypeBuilder()
        {
            typeof (PageTypeResolver).GetProperty("Instance").SetValue(null, null, null);
            Initializer.Start();
            if (_pageTypeBuilderInitialisedOnce)
            {
                var synchronizer = new PageTypeSynchronizer(new PageTypeDefinitionLocator(),
                                                            new PageTypeBuilderConfiguration());
                // HACK: Requried because SynchronizePageTypes is internal.
                typeof (PageTypeSynchronizer).InvokeMember("SynchronizePageTypes",
                                                           BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                                                           BindingFlags.Instance,
                                                           null,
                                                           synchronizer,
                                                           new object[0]);
            }
            _pageTypeBuilderInitialisedOnce = true;
        }

        protected override void after_each_test()
        {
            Thread.CurrentPrincipal = _originalPrincipal;
            _transaction.Dispose();
            base.after_each_test();
        }
    }
}