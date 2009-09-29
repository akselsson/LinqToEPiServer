using System;
using System.Security.Principal;
using System.Threading;
using System.Transactions;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    [TestFixture, Category("Integration")]
    public class IntegrationTestsBase : EPiTestBase
    {
        private TransactionScope _transaction;
        private IPrincipal _originalPrincipal;
        private static bool DatabaseIsRestored;
        [TestFixtureSetUp]
        public void init_database()
        {
            if(DatabaseIsRestored)
                return;
            IntegrationTestDatabase.Restore();
            DatabaseIsRestored = true;
        }

        protected override void setup_epi(EPiTester context)
        {
            context.ConnectionString = IntegrationTestDatabase.ConnectionString;
            base.setup_epi(context);
        }
        protected override void before_each_test()
        {
            _transaction = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes(20));
            _originalPrincipal = Thread.CurrentPrincipal;
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("admin"), new[] { "administrators" });
            base.before_each_test();
        }

        protected override void after_each_test()
        {
            Thread.CurrentPrincipal = _originalPrincipal;
            _transaction.Dispose();
            base.after_each_test();
        }
    }
}