using System;
using System.Security.Principal;
using System.Threading;
using System.Transactions;
using EPiServer.PlugIn;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;
using PageTypeBuilder;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    [TestFixture]
    public class IntegrationTestsBase : EPiSpecBase
    {
        private TransactionScope _transaction;
        private IPrincipal _originalPrincipal;
        private static bool _databaseIsRestored;

        [TestFixtureSetUp]
        public void init_database()
        {
            if(_databaseIsRestored)
                return;
            IntegrationTestDatabase.Restore();
            _databaseIsRestored = true;
        }

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
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("admin"), new[] { "administrators" });
            Plugins.LocateFromReferencedAssembly();
            PageDefinitionTypePlugInAttribute.Start();
            Initializer.Start();
        }

        protected override void after_each_test()
        {
            Thread.CurrentPrincipal = _originalPrincipal;
            _transaction.Dispose();
            base.after_each_test();
        }
    }
}