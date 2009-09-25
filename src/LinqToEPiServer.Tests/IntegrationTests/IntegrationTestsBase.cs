using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Transactions;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    [TestFixture,Category("Integration")]
    public class IntegrationTestsBase : EPiTestBase
    {
        private TransactionScope _transaction;
        private IPrincipal _originalPrincipal;
        private readonly FileInfo _databaseFile = new FileInfo(@"..\..\..\LinqToEPiServer.WebSite\App_Data\EPiServerDB.mdf");

        protected override void setup_epi(Helpers.EPiTester context)
        {
            context.ConnectionString = string.Format(@"Data Source=.;integrated security=true;MultipleActiveResultSets=True;AttachDbFilename={0}", _databaseFile.FullName);
            base.setup_epi(context);
        }
        protected override void before_each_test()
        {
            _transaction = new TransactionScope(TransactionScopeOption.RequiresNew,TimeSpan.FromMinutes(20));
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