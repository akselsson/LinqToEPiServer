using System.Linq;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Visitors.Rewriters;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.QueryTranslation
{
    public class WhereCombinerSpec : EPiTestBase
    {
        private IQueryable<PageData> _queryable;

        private ExpressionTransformerTester the_query(IQueryable query)
        {
            return new ExpressionTransformerTester(query.Expression, new WhereCombiner());
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
                .should_be_transformed_to(_queryable.Where(i => i == null && i != null));
        }

        [Test]
        public void should_rewrite_three_where()
        {
            the_query(_queryable.Where(i => i == null).Where(i => i != null).Where(i=>i==null))
                .should_be_transformed_to(_queryable.Where(i => i == null && (i != null && i==null)));
        }

        [Test]
        public void int_query_should_not_be_transformed()
        {
            the_query(_queryable.Select(pd=>pd.PageTypeID).Where(i=>i!=1).Where(i=>i==1))
                .should_not_be_transformed();
        }
    }
}