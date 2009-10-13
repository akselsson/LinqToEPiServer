using System.Linq.Expressions;
using LinqToEPiServer.Implementation.Visitors.Rewriters;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public class QuoteStripperSpec : SpecBase
    {
        [Test]
        public void should_remove_outer_quote()
        {
            ConstantExpression inner = Expression.Constant(1);
            UnaryExpression quote = Expression.Quote(inner);

            var result = new QuoteStripper().Rewrite(quote);

            Assert.AreEqual(inner,result);
        }
    }
}