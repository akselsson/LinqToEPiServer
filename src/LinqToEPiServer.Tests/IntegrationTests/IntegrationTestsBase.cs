using System;
using System.Security.Principal;
using System.Threading;
using System.Transactions;
using EPiServer.PlugIn;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    [TestFixture]
    public class IntegrationTestsBase : SpecRequiringEPiRuntime
    {
        private TransactionScope _transaction;
        private IPrincipal _originalPrincipal;

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
            PluginHelper.LocatePluginsFromReferencedAssemblies();
            PageDefinitionTypePlugInAttribute.Start();
            PageTypeBuilderHelper.Init();
        }

        protected override void after_each_test()
        {
            Thread.CurrentPrincipal = _originalPrincipal;
            _transaction.Dispose();
            base.after_each_test();
        }
    }
}