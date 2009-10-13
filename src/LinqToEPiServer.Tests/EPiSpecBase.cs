using System;
using LinqToEPiServer.Tests.Helpers;

namespace LinqToEPiServer.Tests
{
    public class EPiSpecBase : SpecBase
    {
        private EPiTester _epiContext;

        protected override void before_each_test()
        {
            _epiContext = new EPiTester();
            setup_epi(_epiContext);
            _epiContext.Init();
            base.before_each_test();
        }

        protected virtual void setup_epi(EPiTester context)
        {

        }
    }
}