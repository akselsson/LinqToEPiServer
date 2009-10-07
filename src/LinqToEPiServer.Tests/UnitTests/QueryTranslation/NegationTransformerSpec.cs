using System;
using System.Linq.Expressions;
using EPiServer.Core;
using LinqToEPiServer.Implementation.Visitors.Rewriters;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;
using System.Linq;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public class NegationTransformerSpec : EPiTestBase
    {
        [Test]
        public void simple_not()
        {
            the_expression(pd => !(pd["test"] == "test"))
                .should_be_transformed_to(pd => pd["test"] != "test");
        }

        [Test]
        public void and()
        {
            the_expression(pd=>!(pd["test"] == "test" && pd["test"] == "test"))
                .should_be_transformed_to(pd=>pd["test"] != "test" || pd["test"] != "test");
        }

        [Test]
        public void or()
        {
            the_expression(pd => !(pd["test"] == "test" && pd["test"] == "test"))
                .should_be_transformed_to(pd => pd["test"] != "test" || pd["test"] != "test");
        }
        [Test]
        public void string_equals()
        {
            the_expression(pd => !"test".Equals("a"))
                .should_not_be_transformed();
        }

        [Test]
        public void less_than()
        {
            int value = 1;
            the_expression(pd=>!(value<2))
                .should_be_transformed_to(pd=>value>=2);
        }

        [Test]
        public void greater_than()
        {
            int value = 1;
            the_expression(pd=>!(value>2))
                .should_be_transformed_to(pd=>value<=2);
        }

        [Test]
        [TestCase(ExpressionType.LessThanOrEqual,ExpressionType.GreaterThan)]
        [TestCase(ExpressionType.GreaterThanOrEqual,ExpressionType.LessThan)]
        [TestCase(ExpressionType.Equal,ExpressionType.NotEqual)]
        [TestCase(ExpressionType.NotEqual,ExpressionType.Equal)]
        [TestCase(ExpressionType.LessThan,ExpressionType.GreaterThanOrEqual)]
        [TestCase(ExpressionType.GreaterThan,ExpressionType.LessThanOrEqual)]
        public void int_comparisons(ExpressionType inputType, ExpressionType outputType)
        {
            var input = Expression.Not(Expression.MakeBinary(inputType,Expression.Constant(1), Expression.Constant(2)));
            var expected = Expression.MakeBinary(outputType,Expression.Constant(1), Expression.Constant(2));
            the_expression(MakeLambda(input)).should_be_transformed_to(MakeLambda(expected));
        }

        [Test]
        public void nested_or()
        {
            var value = 1;
            the_expression(pd=>!(value==1 || value ==2 || value == 3))
                .should_be_transformed_to(pd=>value!=1 && value != 2 && value != 3);
        }

        [Test]
        public void non_negated_nested()
        {
            var value = 1;
            the_expression(pd=>value == 1 && value == 2 && value == 3 || (value == 4 || value == 5))
                .should_not_be_transformed();
        }

        [Test]
        public void nested_negated_or()
        {
            var value = 1;
            the_expression(pd=>!(value == 1 || !(value == 2 || value == 3)))
                .should_be_transformed_to(pd=>value!= 1 && (value == 2 || value == 3));
        }

        [Test]
        public void nested_predicate_in_method_should_not_be_transformed()
        {
            the_expression(pd=>!pd.PageLanguages.Where(pl=>pl == "test").Any())
                .should_not_be_transformed();
        }

        [Test]
        public void nested_predicate_in_method_should_not_be_transformed_2()
        {
            var value = 1;
            the_expression(pd => !pd.CheckPublishedStatus(PagePublishedStatus.Published).Equals(2 == value))
                .should_not_be_transformed();
        }


        private Expression<Func<PageData, bool>> MakeLambda(Expression input)
        {
            return (Expression<Func<PageData, bool>>)Expression.Lambda(input, Expression.Parameter(typeof (PageData), "pd"));
        }

        private ExpressionTransformerTester the_expression(Expression<Func<PageData, bool>> predicate)
        {
            return new ExpressionTransformerTester(predicate, new NegationFlattener());
        }
    }
}