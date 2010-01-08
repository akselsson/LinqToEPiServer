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
            StartTransaction();
            RunAsAdmin();
            InitPageDefinitions();
            InitPageTypeBuilder();
        }

        private void StartTransaction()
        {
            _transaction = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes(20));
        }

        private void RunAsAdmin()
        {
            _originalPrincipal = Thread.CurrentPrincipal;
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("admin"), new[] { "administrators" });
        }

        private static void InitPageDefinitions()
        {
            PluginHelper.LocatePluginsFromReferencedAssemblies();
            PageDefinitionTypePlugInAttribute.Start();
        }

        private static void InitPageTypeBuilder()
        {
            PageTypeBuilderHelper.Init();
        }

        protected override void after_each_test()
        {
            ResetPrincipal();
            RollbackTransaction();
            base.after_each_test();
        }

        private void ResetPrincipal()
        {
            Thread.CurrentPrincipal = _originalPrincipal;
        }

        private void RollbackTransaction()
        {
            _transaction.Dispose();
        }
    }
}