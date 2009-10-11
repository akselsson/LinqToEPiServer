using System.Linq;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Visitors.Rewriters;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public class WhereCombinerSpec : EPiTestBase
    {
        private IQueryable<PageData> _queryable;

        private ExpressionRewriterTester the_query(IQueryable query)
        {
            return new ExpressionRewriterTester(query.Expression, new WhereCombiner());
        }

        protected override void establish_context()
        {
            base.establish_context();
            _queryable = new PageData[] { }.AsQueryable();
        }

        [Test]
        public void should_rewrite_two_where()
        {
            the_query(_queryable.Where(i => i == null).Where(i => i != null))
                .should_be_rewritten_to(_queryable.Where(i => i == null && i != null));
        }

        [Test]
        public void should_rewrite_three_where()
        {
            the_query(_queryable.Where(i => i == null).Where(i => i != null).Where(i => i == null))
                .should_be_rewritten_to(_queryable.Where(i => i == null && (i != null && i == null)));
        }

        [Test]
        public void where_after_select_int_should_not_be_rewritten()
        {
            the_query(_queryable.Select(pd => pd.PageTypeID).Where(i => i != 1).Where(i => i == 1))
                .should_be_rewritten_to(_queryable.Select(pd => pd.PageTypeID).Where(i => i != 1 && i == 1));
        }

        [Test]
        public void where_separated_by_select_should_not_be_combined()
        {
            bool bool1 = true;
            bool bool2 = false;
            the_query(_queryable.Where(pd => bool1).Where(pd => bool2).Select(pd => pd).Where(pd => bool2).Where(pd => bool1))
                .should_be_rewritten_to(_queryable.Where(pd => bool1 && bool2).Select(pd => pd).Where(pd => bool2 && bool1));
        }
    }
}