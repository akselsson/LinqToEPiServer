using System;
using System.Linq.Expressions;
using EPiServer.Core;
using PageTypeBuilder.Reflection;

namespace PageTypeBuilder
{
    public static class PageDataExtensionMethods
    {
        public static TProperty GetPropertyValue<TPageData, TProperty>(this TPageData pageData, Expression<Func<TPageData, TProperty>> expression) 
            where TPageData : PageData
        {
            return pageData.GetPropertyValue(expression, false);
        }

        public static TProperty GetPropertyValue<TPageData, TProperty>(this TPageData pageData, Expression<Func<TPageData, TProperty>> expression, 
            bool includeDynamicPropertyValue) where TPageData : PageData
        {
            MemberExpression memberExpression = GetMemberExpression(expression);

            object value;
            if(includeDynamicPropertyValue) 
                value = (TProperty)pageData[memberExpression.Member.Name];
            else
                value = pageData.GetValue(memberExpression.Member.Name);

            return ConvertToRequestedType<TProperty>(value);
        }

        private static MemberExpression GetMemberExpression<TPageData, TProperty>(Expression<Func<TPageData, TProperty>> expression)
        {
            MemberExpression memberExpression = null;
            if (expression.Body is MemberExpression)
            {
                memberExpression = (MemberExpression)expression.Body;
            }
            else if (expression.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression)expression.Body;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression == null)
                throw new Exception("The body of the expression must be either a MemberExpression of a UnaryExpression.");
            return memberExpression;
        }

        private static TProperty ConvertToRequestedType<TProperty>(object value)
        {
            if (value != null)
                return (TProperty)value;

            if (!typeof(TProperty).CanBeNull())
                throw new Exception(@"The property value is null and the requested type is a value type. 
                    Consider using nullable as type or make the property mandatory.");

            return (TProperty)value;
        }

        public static TProperty GetPropertyValue<TPageData, TProperty>(this TPageData pageData, Expression<Func<TPageData, object>> expression) 
            where TPageData : PageData
        {
            return (TProperty)pageData.GetPropertyValue(expression, false);
        }

        public static TProperty GetPropertyValue<TPageData, TProperty>(this TPageData pageData, Expression<Func<TPageData, object>> expression,
            bool includeDynamicPropertyValue) where TPageData : PageData
        {
            MemberExpression memberExpression = GetMemberExpression(expression);

            object value;
            if (includeDynamicPropertyValue)
                value = (TProperty)pageData[memberExpression.Member.Name];
            else
                value = pageData.GetValue(memberExpression.Member.Name);

            return ConvertToRequestedType<TProperty>(value);
        }

        public static void SetPropertyValue<TPageData>(this TPageData pageData, Expression<Func<TPageData, object>> expression, object value) where TPageData : PageData
        {
            MemberExpression memberExpression = null;
            if (expression.Body is MemberExpression)
            {
                memberExpression = (MemberExpression)expression.Body;
            }
            else if (expression.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression)expression.Body;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression == null)
                throw new Exception("The body of the expression must be either a MemberExpression or a UnaryExpression.");

            pageData[memberExpression.Member.Name] = value;
        }
    }
}
