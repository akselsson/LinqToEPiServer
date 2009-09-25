using System;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;

namespace LinqToEPiServer.Tests
{
    [TestFixture]
    public class EPiTestBase
    {
        private EPiTester _epiContext;

        [SetUp]
        public void init()
        {
            _epiContext = new EPiTester();
            setup_epi(_epiContext);
            _epiContext.Init();
            before_each_test();
            establish_context();
            because();
        }

        protected virtual void setup_epi(EPiTester context)
        {
            
        }

        protected virtual void because()
        {
            
        }

        protected virtual void establish_context()
        {
            
        }

        protected virtual void before_each_test()
        {
            
        }

        protected virtual void after_each_test()
        {
            
        }

        [TearDown]
        public void terminate()
        {
            after_each_test();
        }
    }
}