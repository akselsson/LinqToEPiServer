using NUnit.Framework;

namespace LinqToEPiServer.Tests
{
    [TestFixture]
    public class SpecBase
    {
        [SetUp]
        public void init()
        {
            before_each_test();
            establish_context();
            because();
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