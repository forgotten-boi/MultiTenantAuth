using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DataAccess.Abstract;

namespace DataAccess.Databases
{
    /// <summary>
    /// Used to connect SQL Server Specific database connection and it's operations.
    /// </summary>
    public class SqlDatabase : Database
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDatabase"/> class.
        /// </summary>
        /// <param name="conString">The name or connection string.</param>
        public SqlDatabase(string conString)
            : base(conString)
        {
        }
        protected override DbConnection CreateConnection(string conString)
        {
            return new SqlConnection(conString);
        }

        /// <summary>
        /// Creates the data adapter. Must be overriden by the derived class
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns></returns>
        protected override DbDataAdapter CreateDataAdapter(DbCommand cmd)
        {
            return new SqlDataAdapter(cmd as SqlCommand);
        }

        public override DbParameter CreateTVPParameter(string name, DataTable value)
        {
            var typeName = value.TableName;
            if (string.IsNullOrWhiteSpace(typeName))
                throw new System.Exception("Please specify the name of table same as your TVP type name.");

            var parameter = new SqlParameter(name, value)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = typeName
            };
            return parameter;
        }
        
        public override DbParameter CreateParameter(string name, object value)
        {
            if (!(value is DbParameter param))
            {
                var parameter = new SqlParameter
                {
                    ParameterName = CreateParameterName(name),
                    Value = value ?? DBNull.Value
                };
                return parameter;
            }
            return param;
        }

        public override DbParameter CreateOutputParameter(string name, DbType dbType)
        {
            var parameter = new SqlParameter
            {
                ParameterName = CreateParameterName(name),
                DbType = dbType,
                Direction = ParameterDirection.Output
            };
            return parameter;
        }
    }
}
