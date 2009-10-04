using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;
using LinqToEPiServer.Implementation;
using LinqToEPiServer.Tests.Fakes;
using LinqToEPiServer.Tests.Helpers;
using NUnit.Framework;

namespace LinqToEPiServer.Tests.UnitTests.QueryTranslation
{
    public class QueryTranslationSpec : EPiTestBase
    {
        private StubQueryExecutor query_executor;
        protected PageDataQuery system_under_test;

        protected override void before_each_test()
        {
            query_executor = new StubQueryExecutor();
            system_under_test =
                new PageDataQuery(new FindPagesWithCriteriaQueryProvider(PageReference.StartPage, query_executor));
        }

        public class Supported : QueryTranslationSpec
        {
            [Test]
            public void string_contains()
            {
                system_under_test
                    .Where(pd => ((string) pd["MainBody"]).Contains("test"))
                    .should_be_translated_to(new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Contained,
                                                     IsNull = false,
                                                     Name = "MainBody",
                                                     Required = true,
                                                     Type = PropertyDataType.String,
                                                     Value = "test"
                                                 });
            }

            [Test]
            public void date_variable()
            {
                var date = new DateTime(2001, 01, 01);

                system_under_test
                    .Where(pd => (DateTime) pd["start"] == date)
                    .should_be_translated_to(new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "start",
                                                     Required = true,
                                                     Type = PropertyDataType.Date,
                                                     Value = "2001-01-01 00:00:00"
                                                 });
            }

            [Test]
            public void date_nullable()
            {
                var date = new DateTime(2001, 01, 01);

                system_under_test
                    .Where(pd=>pd["start"] as DateTime? == date)
                    .should_be_translated_to(new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "start",
                                                     Required = true,
                                                     Type = PropertyDataType.Date,
                                                     Value = "2001-01-01 00:00:00"
                                                 });
            }

            [Test]
            public void date_nullable_less_than()
            {
                var date = new DateTime(2001, 01, 01);

                system_under_test
                    .Where(pd => pd["start"] as DateTime? < date)
                    .should_be_translated_to(new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.LessThan,
                                                     IsNull = false,
                                                     Name = "start",
                                                     Required = true,
                                                     Type = PropertyDataType.Date,
                                                     Value = "2001-01-01 00:00:00"
                                                 });
            }

            [Test]
            public void date_with_new()
            {
                system_under_test
                    .Where(pd => (DateTime) pd["start"] == new DateTime(2001, 01, 01))
                    .should_be_translated_to(new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "start",
                                                     Required = true,
                                                     Type = PropertyDataType.Date,
                                                     Value = "2001-01-01 00:00:00"
                                                 });
            }

            [Test]
            public void double_negated_equal()
            {
                system_under_test
                    .Where(pd => !(pd["MainBody"] != "test"))
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
            public void string_ends_with()
            {
                system_under_test
                    .Where(pd => ((string) pd["MainBody"]).EndsWith("test"))
                    .should_be_translated_to(new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.EndsWith,
                                                     IsNull = false,
                                                     Name = "MainBody",
                                                     Required = true,
                                                     Type = PropertyDataType.String,
                                                     Value = "test"
                                                 });
            }

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
            public void int_property()
            {
                system_under_test
                    .Where(pd => (int) pd["int"] == 1)
                    .should_be_translated_to(new PropertyCriteria
                                                 {
                                                     Condition = CompareCondition.Equal,
                                                     IsNull = false,
                                                     Name = "int",
                                                     Required = true,
                                                     Type = PropertyDataType.Number,
                                                     Value = "1"
                                                 });
            }

            [Test]
            public void int_less_than()
            {
                system_under_test
                    .Where(pd => (int) pd["int"] < 10)
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
            public void int_less_than_with_reversed_argument_order()
            {
                system_under_test
                    .Where(pd => 10 > (int) pd["int"])
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
                    .Where(pd => (string) pd["MainBody"] == null)
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
                    .Where(pd => (int?) pd["number"] == null)
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
                    .Where(pd => (string) pd["test"] != null)
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
                    .Where(pd => ((string) pd["MainBody"]).StartsWith("test"))
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
                                 (int) pd["int"] == 2)
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
                    .Where(pd => (bool) pd["test"] == true)
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
                    .Where(pd => (bool) pd["test"])
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
                    .Where(pd => !(bool) pd["test"])
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
                    .Where(pd => (float) pd["float"] == 0.1d)
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
                    .Where(pd => (float) pd["float"] == 0.1f)
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
                    .Where(pd => (int) pd["test"] < (int) d)
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
                    .Where(pd => (double) pd["test"] < value)
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
                    .Where(pd=>pd.PageName=="test" && pd.PageTypeID == 1);
                
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
        }

        public class Not_Supported : QueryTranslationSpec
        {
            [Test]
            public void contained_on_constant_expression_should_throw()
            {
                system_under_test
                    .Where(pd => "test".Contains((string) pd["MainBody"]))
                    .should_not_be_supported();
            }

            [Test]
            public void int_less_than_or_equal_with_reversed_argument_order()
            {
                system_under_test
                    .Where(pd => 10 >= (int) pd["int"])
                    .should_not_be_supported();
            }

            [Test]
            public void mixed_and_with_or_in_not()
            {
                // Translated to pd["1"]!="1" || (pd["2"]!="2" && pd["3"]!="3"))), which is not supported
                system_under_test
                    .Where(pd => !(pd["1"] == "1" && (pd["2"] == "2" || pd["3"] == "3")))
                    .should_not_be_supported();
            }

            [Test]
            public void string_equals_with_string_comparison()
            {
                system_under_test
                    .Where(pd => pd.PageName.Equals("a", StringComparison.OrdinalIgnoreCase))
                    .should_not_be_supported();
            }

            [Test]
            public void list_contains()
            {
                system_under_test
                    .Where(pd => ((List<int>) pd["test"]).Contains(1))
                    .should_not_be_supported();
            }

           

            [Test]
            public void skip()
            {
                system_under_test
                    .Skip(0)
                    .should_not_be_supported();
            }

            [Test]
            public void and_nested_in_or()
            {
                system_under_test
                    .Where(pd =>
                           pd["test"] == "test" ||
                           (pd["test2"] == "test2" && pd["test3"] == "test3"))
                    .should_not_be_supported();
            }

            [Test]
            public void multiple_and_with_nested_or()
            {
                system_under_test
                    .Where(pd =>
                           (pd["test2"] == "test2" || pd["test3"] == "test3") &&
                           (pd["test2"] == "test2" || pd["test3"] == "test3")
                    ).should_not_be_supported();
            }

            [Test]
            public void multiple_nested_or()
            {
                system_under_test
                    .Where(pd =>
                           (pd["test2"] == "test2" || pd["test3"] == "test3") ||
                           (pd["test2"] == "test2" || pd["test3"] == "test3")
                    ).should_not_be_supported();
            }

            [Test]
            public void order_by()
            {
                system_under_test
                    .OrderBy(pd => pd.PageName)
                    .should_not_be_supported();
            }

            [Test]
            public void select()
            {
                system_under_test
                    .Select(pd => pd.PageName)
                    .should_not_be_supported();
            }

        }
    }
}