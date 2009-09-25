using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;

namespace IQToolkit.Data.OleDb
{
    using IQToolkit.Data.Common;

    public class OleDbQueryProvider : DbEntityProvider
    {
        public OleDbQueryProvider(OleDbConnection connection, QueryMapping mapping, QueryPolicy policy)
            : base(connection, mapping, policy)
        {
        }

        protected override void AddParameter(DbCommand command, QueryParameter parameter, object value)
        {
            QueryType qt = parameter.QueryType;
            if (qt == null)
                qt = this.Language.TypeSystem.GetColumnType(parameter.Type);
            var p = ((OleDbCommand)command).Parameters.Add(parameter.Name, GetOleDbType(qt), qt.Length);
            if (qt.Precision != 0)
                p.Precision = (byte)qt.Precision;
            if (qt.Scale != 0)
                p.Scale = (byte)qt.Scale;
            p.Value = value ?? DBNull.Value;
        }

        protected virtual OleDbType GetOleDbType(QueryType type)
        {
            return ToOleDbType(type.DbType);
        }

        public static OleDbType ToOleDbType(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                    return OleDbType.VarChar;
                case DbType.AnsiStringFixedLength:
                    return OleDbType.Char;
                case DbType.Binary:
                    return OleDbType.Binary;
                case DbType.Boolean:
                    return OleDbType.Boolean;
                case DbType.Byte:
                    return OleDbType.UnsignedTinyInt;
                case DbType.Currency:
                    return OleDbType.Currency;
                case DbType.Date:
                    return OleDbType.Date;
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return OleDbType.DBTimeStamp;
                case DbType.Decimal:
                    return OleDbType.Decimal;
                case DbType.Double:
                    return OleDbType.Double;
                case DbType.Guid:
                    return OleDbType.Guid;
                case DbType.Int16:
                    return OleDbType.SmallInt;
                case DbType.Int32:
                    return OleDbType.Integer;
                case DbType.Int64:
                    return OleDbType.BigInt;
                case DbType.Object:
                    return OleDbType.Variant;
                case DbType.SByte:
                    return OleDbType.TinyInt;
                case DbType.Single:
                    return OleDbType.Single;
                case DbType.String:
                    return OleDbType.VarWChar;
                case DbType.StringFixedLength:
                    return OleDbType.WChar;
                case DbType.Time:
                    return OleDbType.DBTime;
                case DbType.UInt16:
                    return OleDbType.UnsignedSmallInt;
                case DbType.UInt32:
                    return OleDbType.UnsignedInt;
                case DbType.UInt64:
                    return OleDbType.UnsignedBigInt;
                case DbType.VarNumeric:
                    return OleDbType.Numeric;
                case DbType.Xml:
                    return OleDbType.VarWChar;
                default:
                    throw new InvalidOperationException(string.Format("Unhandled db type '{0}'.", type));
            }
        }
    }
}