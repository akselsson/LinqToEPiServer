using System;
using System.Collections;
using LinqToEpiServer.PageTypeBuilder;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.UnitTests.PageTypeBuilder
{
    public class FilterByTypeSpec : SpecBase
    {
        private FilterByType<string> system_under_test;

        protected override void establish_context()
        {
            base.establish_context();
            system_under_test = new FilterByType<string>();
        }

        [Test]
        public void when_input_is_null_should_throw()
        {
            Assert.Throws<ArgumentNullException>(() => system_under_test.Transform(null));
        }

        [Test]
        public void should_throw_when_input_is_not_enumerable()
        {
            Assert.Throws<InvalidOperationException>(() => system_under_test.Transform(new object()));
        }

        [Test]
        public void should_remove_objects_not_of_correct_type()
        {
            var actual = (IEnumerable) system_under_test.Transform(new[] {"test", new object()});
            var expected = new[] {"test"};
            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}