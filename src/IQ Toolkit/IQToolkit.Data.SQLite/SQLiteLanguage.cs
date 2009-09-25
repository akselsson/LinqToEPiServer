using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace IQToolkit.Data.SQLite
{
    using IQToolkit.Data.Common;

    public sealed class SQLiteLanguage : QueryLanguage
    {
        private SQLiteTypeSystem _typeSystem = new SQLiteTypeSystem();

        public override QueryTypeSystem TypeSystem
        {
            get { return _typeSystem; }
        }

        public override string Quote(string name)
        {
            if (name.StartsWith("[") && name.EndsWith("]"))
            {
                return name;
            }
            else if (name.IndexOf('.') > 0)
            {
                return "[" + string.Join("].[", name.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)) + "]";
            }
            else
            {
                return "[" + name + "]";
            }
        }

        private static readonly char[] splitChars = new char[] { '.' };

        public override Expression GetGeneratedIdExpression(MemberInfo member)
        {
            return new FunctionExpression(TypeHelper.GetMemberType(member), "last_insert_rowid()", null);
        }

        public override Expression GetRowsAffectedExpression(Expression command)
        {
            return new FunctionExpression(typeof(int), "changes()", null);
        }

        public override Expression Translate(Expression expression)
        {
            // fix up any order-by's
            expression = OrderByRewriter.Rewrite(expression);

            expression = base.Translate(expression);

            //expression = SkipToNestedOrderByRewriter.Rewrite(expression);
            expression = UnusedColumnRemover.Remove(expression);

            return expression;
        }

        public override string Format(Expression expression)
        {
            return SQLiteFormatter.Format(expression);
        }

        public static readonly QueryLanguage Default = new SQLiteLanguage();
    }
}
