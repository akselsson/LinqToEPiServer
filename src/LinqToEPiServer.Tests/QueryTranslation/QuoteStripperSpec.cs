using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LinqToEPiServer.Implementation.Visitors.Rewriters;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.QueryTranslation
{
    public class QuoteStripperSpec : EPiTestBase
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
