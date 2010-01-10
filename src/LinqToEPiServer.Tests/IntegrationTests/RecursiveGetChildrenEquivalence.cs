using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer;
using EPiServer.Core;
using LinqToEpiServer.PageTypeBuilder;
using LinqToEPiServer.Tests.Helpers;
using LinqToEPiServer.Tests.Model;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.IntegrationTests
{
    public class RecursiveGetChildrenEquivalence : IntegrationTestsBase
    {
        private PageReference _root;
        private PageTypeBuilderRepository _findPagesWithCriteriaRepository;
        private RecursiveGetChildrenRepository _getChildrenRepository;
        private DataFactory _pageSource;

        protected override void before_each_test()
        {
            base.before_each_test();
            _root = PageReference.RootPage;
            _pageSource = DataFactory.Instance;
            _getChildrenRepository = new RecursiveGetChildrenRepository();
            _findPagesWithCriteriaRepository = new PageTypeBuilderRepository(new DataFactoryQueryExecutor());
        }

        protected void assert_query_equivalence(int expectedRowCount, Expression<Func<PageData, bool>> predicate)
        {
            IQueryable<PageData> allPages = _getChildrenRepository.FindDescendantsOf(_root);
            List<PageData> recursiveGetChildrenResult = allPages.Where(predicate).ToList();
            List<PageData> linqToEPiServerResult =
                _findPagesWithCriteriaRepository.FindDescendantsOf(_root).Where(predicate).ToList();

            CollectionAssert.IsNotEmpty(recursiveGetChildrenResult, "Make sure at least one page matches query");
            CollectionAssert.AreNotEquivalent(allPages, recursiveGetChildrenResult,
                                              "Make sure at least one page is filtered out by query");

            Assert.AreEqual(expectedRowCount, recursiveGetChildrenResult.Count);

            recursiveGetChildrenResult.should_be_equivalent_to(linqToEPiServerResult);
        }

        protected void given_pages<T>(params Action<T>[] buildups) where T : PageData
        {
            foreach (var buildup in buildups)
            {
                _findPagesWithCriteriaRepository.Add(_root, buildup);
            }
        }

        public class NotSupported : RecursiveGetChildrenEquivalence
        {
            protected void query_should_throw<T>(Expression<Func<PageData, bool>> predicate) where T : Exception
            {
                Assert.Throws<T>(() => assert_query_equivalence(0, predicate));
            }

            [Test]
            public void date_with_cast()
            {
                var date = new DateTime(2001, 01, 01);
                given_pages<QueryPage>(
                    page => page.Date = date,
                    page => page.Date = date.AddDays(1)
                    );
                query_should_throw<NullReferenceException>(pd => (DateTime) pd["Date"] == date);
            }

            [Test]
            public void int_with_cast()
            {
                given_pages<QueryPage>(
                    page => page.Number = 1,
                    page => page.Number = null
                    );
                query_should_throw<NullReferenceException>(pd => (int) pd["Number"] == 1);
            }

            [Test]
            public void string_contains()
            {
                given_pages<QueryPage>(
                    page => page.Text = "included",
                    page => page.Text = "excluded"
                    );
                query_should_throw<NullReferenceException>(pd => ((string) pd["Text"]).Contains("included"));
            }

            [Test]
            public void string_ends_with()
            {
                given_pages<QueryPage>(
                    page => page.Text = "atest",
                    page => page.Text = "no match"
                    );
                query_should_throw<NullReferenceException>(pd => ((string) pd["Text"]).EndsWith("test"));
            }
        }

        public class Supported : RecursiveGetChildrenEquivalence
        {
            [Test]
            public void date_nullable()
            {
                var date = new DateTime(2001, 01, 01);
                given_pages<QueryPage>(
                    page => page.Date = date,
                    page => page.Date = date.AddDays(1)
                    );
                assert_query_equivalence(1, pd => pd["Date"] as DateTime? == date);
            }

            [Test]
            public void date_nullable_less_than()
            {
                var date = new DateTime(2001, 01, 01);
                given_pages<QueryPage>(
                    page => page.Date = date,
                    page => page.Date = date.AddDays(1),
                    page => page.Date = date.AddDays(-1),
                    page => page.Date = null
                    );
                assert_query_equivalence(1, pd => pd["Date"] as DateTime? < date);
            }

            [Test]
            public void int_nullable()
            {
                given_pages<QueryPage>(
                    page => page.Number = 1,
                    page => page.Number = 2,
                    page => page.Number = null
                    );
                assert_query_equivalence(1, pd => pd["Number"] as int? == 1);
            }

            [Test]
            public void int_nullable_less_than()
            {
                given_pages<QueryPage>(
                    page => page.Number = 1,
                    page => page.Number = 2,
                    page => page.Number = null
                    );
                assert_query_equivalence(1, pd => pd["Number"] as int? < 2);
            }
        }

        /*
                [Test]
                public void object_equal_method()
                {
                    system_under_test
                        .Where(pd => pd["MainBody"].Equals("test"))
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        });
                }

                

               
                [Test]
                public void int_less_than_with_reversed_argument_order()
                {
                    system_under_test
                        .Where(pd => 10 > (int)pd["int"])
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.LessThan,
                            IsNull = false,
                            Name = "int",
                            Required = true,
                            Type = PropertyDataType.Number,
                            Value = "10"
                        });
                }


                [Test]
                public void negated_equal()
                {
                    system_under_test
                        .Where(pd => !pd["MainBody"].Equals("test"))
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.NotEqual,
                            IsNull = false,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        });
                }

                [Test]
                public void not_equal()
                {
                    system_under_test
                        .Where(pd => pd["MainBody"] != "test")
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.NotEqual,
                            IsNull = false,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        });
                }

                [Test]
                public void null_string()
                {
                    system_under_test
                        .Where(pd => (string)pd["MainBody"] == null)
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = true,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = null
                        });
                }

                [Test]
                public void null_int()
                {
                    system_under_test
                        .Where(pd => (int?)pd["number"] == null)
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = true,
                            Name = "number",
                            Required = true,
                            Type = PropertyDataType.Number,
                            Value = null
                        });
                }

                [Test]
                public void not_null_string()
                {
                    system_under_test
                        .Where(pd => (string)pd["test"] != null)
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.NotEqual,
                            IsNull = true,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = null
                        });
                }

                [Test]
                public void string_starts_with()
                {
                    system_under_test
                        .Where(pd => ((string)pd["MainBody"]).StartsWith("test"))
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.StartsWith,
                            IsNull = false,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        });
                }

                [Test]
                public void string_constant()
                {
                    system_under_test
                        .Where(pd => pd["MainBody"] == "test")
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        });
                }

                [Test]
                public void query_comprehension_syntax()
                {
                    IQueryable<PageData> query = (from page in system_under_test
                                                  where page["MainIntro"] == "test"
                                                  select page);
                    query.should_be_translated_to(new PropertyCriteria
                    {
                        Condition = CompareCondition.Equal,
                        IsNull = false,
                        Name = "MainIntro",
                        Required = true,
                        Type = PropertyDataType.String,
                        Value = "test"
                    });
                }

                [Test]
                public void string_variable()
                {
                    string value = "the value";

                    system_under_test
                        .Where(pd => pd["MainBody"] == value)
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = value
                        });
                }

                [Test]
                public void double_and()
                {
                    system_under_test
                        .Where(pd => pd["MainBody"] == "test" &&
                                     pd["MainIntro"] == "test2" &&
                                     (int)pd["int"] == 2)
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        },
                                                 new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "MainIntro",
                                                     Required = true,
                                                     Type = PropertyDataType.String,
                                                     Value = "test2"
                                                 },
                                                 new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "int",
                                                     Required = true,
                                                     Type = PropertyDataType.Number,
                                                     Value = "2"
                                                 });
                }

                [Test]
                public void and()
                {
                    system_under_test
                        .Where(pd => pd["MainBody"] == "test" && pd["MainIntro"] == "test2")
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "MainBody",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        },
                                                 new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "MainIntro",
                                                     Required = true,
                                                     Type = PropertyDataType.String,
                                                     Value = "test2"
                                                 });
                }

                [Test]
                public void boolean_with_equal()
                {
                    system_under_test
                        .Where(pd => (bool)pd["test"] == true)
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.Boolean,
                            Value = "True"
                        });
                }

                [Test]
                public void boolean_unary()
                {
                    system_under_test
                        .Where(pd => (bool)pd["test"])
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.Boolean,
                            Value = "True"
                        });
                }

                [Test]
                public void negated_boolean_unary()
                {
                    system_under_test
                        .Where(pd => !(bool)pd["test"])
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.NotEqual,
                            IsNull = false,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.Boolean,
                            Value = "True"
                        });
                }

                [Test]
                public void float_number_with_double_constant()
                {
                    system_under_test
                        .Where(pd => (float)pd["float"] == 0.1d)
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "float",
                            Required = true,
                            Type = PropertyDataType.FloatNumber,
                            Value = "0,1"
                        });
                }

                [Test]
                public void float_number()
                {
                    system_under_test
                        .Where(pd => (float)pd["float"] == 0.1f)
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "float",
                            Required = true,
                            Type = PropertyDataType.FloatNumber,
                            Value = "0,1"
                        });
                }

                [Test]
                public void page_reference()
                {
                    system_under_test
                        .Where(pd => pd["page"] == PageReference.StartPage)
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "page",
                            Required = true,
                            Type = PropertyDataType.PageReference,
                            Value = PageReference.StartPage.ToString()
                        });
                }

                [Test]
                public void page_type_name()
                {
                    system_under_test
                        .Where(pd => pd.PageTypeName == "the name")
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "PageTypeName",
                            Required = true,
                            Type = PropertyDataType.PageType,
                            Value = "the name"
                        });
                }

                [Test]
                public void page_type_from_pagetypeid_property()
                {
                    system_under_test
                        .Where(pd => pd.PageTypeID == 11)
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "PageTypeID",
                            Required = true,
                            Type = PropertyDataType.PageType,
                            Value = "11"
                        });
                }

                [Test]
                public void PageTypeName_equal()
                {
                    system_under_test
                        .Where(pd => pd.PageTypeName.Equals("test"))
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "PageTypeName",
                            Required = true,
                            Type = PropertyDataType.PageType,
                            Value = "test"
                        });
                }

                [Test]
                public void double_cast_to_int()
                {
                    double d = 1d;
                    system_under_test
                        .Where(pd => (int)pd["test"] < (int)d)
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.LessThan,
                            IsNull = false,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.Number,
                            Value = "1"
                        });
                }

                [Test]
                public void GetValue_method()
                {
                    system_under_test
                        .Where(pd => pd.GetValue("test") == "test")
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        });
                }

                [Test]
                public void different_type_in_parameter_and_constant()
                {
                    int value = 1;
                    system_under_test
                        .Where(pd => (double)pd["test"] < value)
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.LessThan,
                            IsNull = false,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.FloatNumber,
                            Value = "1"
                        });
                }

                [Test]
                public void or()
                {
                    system_under_test
                        .Where(pd => pd["test"] == "test" || pd["test2"] == "test2")
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "test",
                            Required = false,
                            Type = PropertyDataType.String,
                            Value = "test"
                        }, new PropertyCriteria()
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "test2",
                            Required = false,
                            Type = PropertyDataType.String,
                            Value = "test2"
                        });
                }

                [Test]
                public void double_or()
                {
                    system_under_test
                        .Where(pd => pd["test"] == "test" || pd["test2"] == "test2" || pd["test3"] == "test3")
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "test",
                            Required = false,
                            Type = PropertyDataType.String,
                            Value = "test"
                        },
                                                 new PropertyCriteria()
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "test2",
                                                     Required = false,
                                                     Type = PropertyDataType.String,
                                                     Value = "test2"
                                                 },
                                                 new PropertyCriteria()
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "test3",
                                                     Required = false,
                                                     Type = PropertyDataType.String,
                                                     Value = "test3"
                                                 });
                }

                [Test]
                public void and_or_combined()
                {
                    system_under_test
                        .Where(pd =>
                               pd["test"] == "test" &&
                               (pd["test2"] == "test2" || pd["test3"] == "test3"))
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        },
                                                 new PropertyCriteria()
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "test2",
                                                     Required = false,
                                                     Type = PropertyDataType.String,
                                                     Value = "test2"
                                                 },
                                                 new PropertyCriteria()
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "test3",
                                                     Required = false,
                                                     Type = PropertyDataType.String,
                                                     Value = "test3"
                                                 });
                }


                [Test]
                public void multiple_nested_and()
                {
                    system_under_test
                        .Where(pd =>
                               (pd["test1"] == "test1" && pd["test2"] == "test2") &&
                               (pd["test2"] == "test2" && pd["test3"] == "test3"))
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "test1",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test1"
                        },
                                                 new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "test2",
                                                     Required = true,
                                                     Type = PropertyDataType.String,
                                                     Value = "test2"
                                                 },
                                                 new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "test2",
                                                     Required = true,
                                                     Type = PropertyDataType.String,
                                                     Value = "test2"
                                                 },
                                                 new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "test3",
                                                     Required = true,
                                                     Type = PropertyDataType.String,
                                                     Value = "test3"
                                                 }
                        );
                }

                [Test]
                public void UrlSegment_page_data_property_without_page_in_name()
                {
                    system_under_test
                        .Where(pd => pd.URLSegment == "test")
                        .should_be_translated_to(new PropertyCriteria
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = false,
                            Name = "PageURLSegment",
                            Required = true,
                            Type = PropertyDataType.String,
                            Value = "test"
                        });
                }


                [Test]
                public void empty_query()
                {
                    PageDataQuery empty = system_under_test;
                    empty.should_be_translated_to();
                }

                [Test]
                public void select_from_empty_query()
                {
                    var empty = from pd in system_under_test select pd;
                    empty.should_be_translated_to();
                }


                [Test]
                public void cast_with_as()
                {
                    system_under_test
                        .Where(pd => pd["test"] as int? == null)
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.Equal,
                            IsNull = true,
                            Name = "test",
                            Required = true,
                            Type = PropertyDataType.Number,
                            Value = null
                        });
                }


                [Test]
                public void multiple_and_in_not()
                {
                    system_under_test
                        .Where(pd => !(pd["test"] == "test" && pd["test"] == "test2"))
                        .should_be_translated_to(new PropertyCriteria()
                        {
                            Condition = CompareCondition.NotEqual,
                            IsNull = false,
                            Name = "test",
                            Required = false,
                            Type = PropertyDataType.String,
                            Value = "test"
                        },
                                                 new PropertyCriteria()
                                                 {
                                                     Condition = CompareCondition.NotEqual,
                                                     IsNull = false,
                                                     Name = "test",
                                                     Required = false,
                                                     Type = PropertyDataType.String,
                                                     Value = "test2"
                                                 });
                }

                [Test]
                public void multiple_where()
                {
                    IQueryable<PageData> actual = system_under_test
                        .Where(pd => pd.PageName == "test")
                        .Where(pd => pd.PageTypeID == 1);
                    IQueryable<PageData> transformed = system_under_test
                        .Where(pd => pd.PageName == "test" && pd.PageTypeID == 1);

                    actual.should_be_equivalent_to(transformed);
                }

                [Test]
                public void multiple_where_with_different_parameter_names()
                {
                    IQueryable<PageData> actual = system_under_test
                        .Where(pd1 => pd1.PageName == "test")
                        .Where(pd2 => pd2.PageTypeID == 1);
                    IQueryable<PageData> transformed = system_under_test
                        .Where(pd => pd.PageName == "test" && pd.PageTypeID == 1);

                    actual.should_be_equivalent_to(transformed);
                }
                 
                 */
    }
}