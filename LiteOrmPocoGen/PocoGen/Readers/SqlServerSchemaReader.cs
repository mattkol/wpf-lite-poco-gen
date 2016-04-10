// -----------------------------------------------------------------------
// <copyright file="SqlServerSchemaReader.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// This is code is based on the T4 template from the PetaPoco project which in turn is based on the subsonic project.
// This is adapted from OrmLite T4 and Dapper.SimpleCRUD Projects.
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using Models;

    /// <summary>
    /// This class represents a MsSql database schema reader.
    /// Class inherits abstract class SchemaReader.
    /// </summary>
    public class SqlServerSchemaReader : SchemaReader
    {
        /// <summary>
        /// MsSql connection object.
        /// </summary>
        private SqlConnection _connection;

        /// <summary>
        /// Reads the Schema returning all tables in the databse.
        /// </summary>
        public override Tables ReadSchema(string connectionString)
        {
            _connection = new SqlConnection(connectionString); 
            var result = new Tables();
            _connection.Open();

            using (var sqlCommand = new SqlCommand(TableSql, _connection))
            {
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Table tbl = new Table();
                        tbl.Name = reader["TABLE_NAME"].ToString();
                        tbl.Schema = reader["TABLE_SCHEMA"].ToString();
                        tbl.IsView = String.Compare(reader["TABLE_TYPE"].ToString(), "View", System.StringComparison.OrdinalIgnoreCase) == 0;
                        tbl.CleanName = Utils.CleanName(tbl.Name);
                        tbl.ClassName = Utils.CleanNameToClassName(tbl.CleanName);
                        result.Add(tbl);
                    }
                }
            }

            foreach (var tbl in result)
            {
                tbl.Columns = LoadColumns(tbl);

                // Mark the primary key
                string primaryKey = GetPk(tbl.Name);
                var pkColumn = tbl.Columns.SingleOrDefault(x => x.Name.ToLower().Trim() == primaryKey.ToLower().Trim());
                if (pkColumn != null)
                {
                    pkColumn.IsPk = true;
                }

                try
                {
                    tbl.OuterKeys = LoadOuterKeys(tbl);
                    tbl.InnerKeys = LoadInnerKeys(tbl);
                }
                catch (Exception x)
                {
                    var error = x.Message.Replace("\r\n", "\n").Replace("\n", " ");
                }
            }

            return result;
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Loads the columns for the specidied table.
        /// </summary>
        private List<Column> LoadColumns(Table tbl)
        {
            using (var sqlCommand = new SqlCommand(ColumnSql, _connection))
            {
                var parameter = sqlCommand.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = tbl.Name;
                sqlCommand.Parameters.Add(parameter);

                parameter = sqlCommand.CreateParameter();
                parameter.ParameterName = "@schemaName";
                parameter.Value = tbl.Schema;
                sqlCommand.Parameters.Add(parameter);

                var result = new List<Column>();

                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Column col = new Column();
                        col.Name = reader["ColumnName"].ToString();
                        col.PropertyName = Utils.CleanUp(col.Name);
                        col.PropertyType = GetPropertyType(reader["DataType"].ToString());
                        col.IsNullable = reader["IsNullable"].ToString() == "YES";
                        col.IsAutoIncrement = ((int)reader["IsIdentity"]) == 1;
                        result.Add(col);
                    }

                    return result;
                }
            }

        }

        /// <summary>
        /// Loads the outer reference keys for the specified table.
        /// </summary>
        private List<Key> LoadOuterKeys(Table tbl)
        {
            using (var sqlCommand = new SqlCommand(OuterKeysSql, _connection))
            {
                var parameter = sqlCommand.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = tbl.Name;
                sqlCommand.Parameters.Add(parameter);

                var result = new List<Key>();

                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var key = new Key();
                        key.Name = reader["FK"].ToString();
                        key.ReferencedTableName = reader["Referenced_tbl"].ToString();
                        key.ReferencedTableColumnName = reader["Referenced_col"].ToString();
                        key.ReferencingTableColumnName = reader["Referencing_col"].ToString();
                        result.Add(key);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Loads the inner reference keys for the specified table.
        /// </summary>
        private List<Key> LoadInnerKeys(Table tbl)
        {
            using (var sqlCommand = new SqlCommand(InnerKeysSql, _connection))
            {
                var parameter = sqlCommand.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = tbl.Name;
                sqlCommand.Parameters.Add(parameter);

                var result = new List<Key>();
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var key = new Key();
                        key.Name = reader["FK"].ToString();
                        key.ReferencingTableName = reader["Referencing_tbl"].ToString();
                        key.ReferencedTableColumnName = reader["Referenced_col"].ToString();
                        key.ReferencingTableColumnName = reader["Referencing_col"].ToString();
                        result.Add(key);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Gets the column primary key name from table specified.
        /// </summary>
        private string GetPk(string table)
        {

            string sql = @"SELECT c.name AS ColumnName
                FROM sys.indexes AS i 
                INNER JOIN sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id 
                INNER JOIN sys.objects AS o ON i.object_id = o.object_id 
                LEFT OUTER JOIN sys.columns AS c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
                WHERE (i.type = 1) AND (o.name = @tableName)";


            using (var sqlCommand = new SqlCommand(sql, _connection))
            {
                var parameter = sqlCommand.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = table;
                sqlCommand.Parameters.Add(parameter);

                var result = sqlCommand.ExecuteScalar();

                if (result != null)
                    return result.ToString();
            }

            return "";
        }

        /// <summary>
        /// Gets the property table based on the sql column type.
        /// </summary>
        private string GetPropertyType(string sqlType)
        {
            string sysType = "string";
            switch (sqlType)
            {
                case "bigint":
                    sysType = "long";
                    break;
                case "smallint":
                    sysType = "short";
                    break;
                case "int":
                    sysType = "int";
                    break;
                case "uniqueidentifier":
                    sysType = "Guid";
                    break;
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "date":
                case "time":
                    sysType = "DateTime";
                    break;
                case "float":
                    sysType = "double";
                    break;
                case "real":
                    sysType = "float";
                    break;
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType = "decimal";
                    break;
                case "tinyint":
                    sysType = "byte";
                    break;
                case "bit":
                    sysType = "bool";
                    break;
                case "image":
                case "binary":
                case "varbinary":
                case "timestamp":
                    sysType = "byte[]";
                    break;
                case "geography":
                    sysType = "Microsoft.SqlServer.Types.SqlGeography";
                    break;
                case "geometry":
                    sysType = "Microsoft.SqlServer.Types.SqlGeometry";
                    break;
            }
            return sysType;
        }

        /// <summary>
        /// Sql query to get table schema info.
        /// </summary>
        private const string TableSql = @"SELECT *
		    FROM  INFORMATION_SCHEMA.TABLES
		    WHERE TABLE_TYPE='BASE TABLE' OR TABLE_TYPE='VIEW'";

        /// <summary>
        /// Sql query to get table columns info.
        /// </summary>
        private const string ColumnSql = @"SELECT 
			TABLE_CATALOG AS [Database],
			TABLE_SCHEMA AS Owner, 
			TABLE_NAME AS TableName, 
			COLUMN_NAME AS ColumnName, 
			ORDINAL_POSITION AS OrdinalPosition, 
			COLUMN_DEFAULT AS DefaultSetting, 
			IS_NULLABLE AS IsNullable, DATA_TYPE AS DataType, 
			CHARACTER_MAXIMUM_LENGTH AS MaxLength, 
			DATETIME_PRECISION AS DatePrecision,
			COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsIdentity') AS IsIdentity,
			COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsComputed') as IsComputed
		    FROM  INFORMATION_SCHEMA.COLUMNS
		    WHERE TABLE_NAME=@tableName AND TABLE_SCHEMA=@schemaName
		    ORDER BY OrdinalPosition ASC";

        /// <summary>
        /// Sql query to get outer reference keys  info.
        /// </summary>
        private const string OuterKeysSql = @"SELECT 
			FK = OBJECT_NAME(pt.constraint_object_id),
			Referenced_tbl = OBJECT_NAME(pt.referenced_object_id),
			Referencing_col = pc.name, 
			Referenced_col = rc.name
		    FROM sys.foreign_key_columns AS pt
		    INNER JOIN sys.columns AS pc
		    ON pt.parent_object_id = pc.[object_id]
		    AND pt.parent_column_id = pc.column_id
		    INNER JOIN sys.columns AS rc
		    ON pt.referenced_column_id = rc.column_id
		    AND pt.referenced_object_id = rc.[object_id]
		    WHERE pt.parent_object_id = OBJECT_ID(@tableName);";

        /// <summary>
        /// Sql query to get inner reference keys  info.
        /// </summary>
        private const string InnerKeysSql = @"SELECT 
			[Schema] = OBJECT_SCHEMA_NAME(pt.parent_object_id),
			Referencing_tbl = OBJECT_NAME(pt.parent_object_id),
			FK = OBJECT_NAME(pt.constraint_object_id),
			Referencing_col = pc.name, 
			Referenced_col = rc.name
		    FROM sys.foreign_key_columns AS pt
		    INNER JOIN sys.columns AS pc
		    ON pt.parent_object_id = pc.[object_id]
		    AND pt.parent_column_id = pc.column_id
		    INNER JOIN sys.columns AS rc
		    ON pt.referenced_column_id = rc.column_id
		    AND pt.referenced_object_id = rc.[object_id]
		    WHERE pt.referenced_object_id = OBJECT_ID(@tableName);";
    }

}
