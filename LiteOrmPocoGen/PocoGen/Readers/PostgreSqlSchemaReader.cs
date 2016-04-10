// -----------------------------------------------------------------------
// <copyright file="PostgreSqlSchemaReader.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// This is code is based on the T4 template from the PetaPoco project which in turn is based on the subsonic project.
// This is adapted from OrmLite T4 and Dapper.SimpleCRUD Projects.
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Npgsql;
    using Models;

    /// <summary>
    /// This class represents a PostgreSql database schema reader.
    /// Class inherits abstract class SchemaReader.
    /// </summary>
    public class PostgreSqlSchemaReader : SchemaReader
    {
        /// <summary>
        /// PostgreSql connection object.
        /// </summary>
        private NpgsqlConnection _connection;

        /// <summary>
        /// Reads the Schema returning all tables in the databse.
        /// </summary>
        public override Tables ReadSchema(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            var result = new Tables();
            _connection.Open();

            using (var sqlCommand = new NpgsqlCommand(TableSql, _connection))
            {
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Table tbl = new Table();
                        tbl.Name = reader["table_name"].ToString();
                        tbl.Schema = reader["table_schema"].ToString();
                        tbl.IsView = String.Compare(reader["table_type"].ToString(), "View", System.StringComparison.OrdinalIgnoreCase) == 0;
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
                string PrimaryKey = GetPk(tbl.Name);
                var pkColumn = tbl.Columns.SingleOrDefault(x => x.Name.ToLower().Trim() == PrimaryKey.ToLower().Trim());
                if (pkColumn != null)
                    pkColumn.IsPk = true;
            }

            LoadReferencesKeysInfo(result);
            return result;
        }

        /// <summary>
        /// Loads the columns for the specidied table.
        /// </summary>
        private List<Column> LoadColumns(Table tbl)
        {

            using (var sqlCommand = new NpgsqlCommand(ColumnSql, _connection))
            {
                var parameter = sqlCommand.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = tbl.Name;
                sqlCommand.Parameters.Add(parameter);

                var result = new List<Column>();

                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Column col = new Column();
                        col.Name = reader["column_name"].ToString();
                        col.PropertyName = Utils.CleanUp(col.Name);
                        col.PropertyType = GetPropertyType(reader["udt_name"].ToString());
                        col.IsNullable = reader["is_nullable"].ToString() == "YES";
                        col.IsAutoIncrement = reader["column_default"].ToString().StartsWith("nextval(");
                        result.Add(col);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Loads the reference keys info for the entire database.
        /// </summary>
        private void LoadReferencesKeysInfo(Tables tables)
        {
            var innerKeysDic = new Dictionary<string, List<Key>>();
            foreach (var item in tables)
            {
                using (var sqlCommand = new NpgsqlCommand(ForeignKeysSql, _connection))
                {
                    var parameter = sqlCommand.CreateParameter();
                    parameter.ParameterName = "@tableName";
                    parameter.Value = item.Name;
                    sqlCommand.Parameters.Add(parameter);

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        item.OuterKeys = new List<Key>();
                        item.InnerKeys = new List<Key>();

                        while (reader.Read())
                        {
                            // Outer keys
                            var outerKey = new Key();
                            outerKey.Name = reader["constraint_name"].ToString();
                            var referencedTable = reader["fkey_to_table"].ToString();
                            outerKey.ReferencedTableName = referencedTable;
                            outerKey.ReferencedTableColumnName = reader["fkey_to_column"].ToString();
                            outerKey.ReferencingTableColumnName = reader["fkey_from_column"].ToString();
                            item.OuterKeys.Add(outerKey);

                            var innerKey = new Key();
                            innerKey.Name = reader["constraint_name"].ToString();
                            innerKey.ReferencingTableName = reader["table_name"].ToString();
                            innerKey.ReferencedTableColumnName = reader["fkey_to_column"].ToString();
                            innerKey.ReferencingTableColumnName = reader["fkey_from_column"].ToString();

                            // add to inner keys references
                            if (innerKeysDic.ContainsKey(referencedTable))
                            {
                                var innerKeys = innerKeysDic[referencedTable];
                                innerKeys.Add(innerKey);
                                innerKeysDic[referencedTable] = innerKeys;
                            }
                            else
                            {
                                var innerKeys = new List<Key>();
                                innerKeys.Add(innerKey);
                                innerKeysDic[referencedTable] = innerKeys;
                            }
                        }

                    }
                }
            }

            // add inner references to tables
            foreach (var item in tables)
            {
                if (innerKeysDic.ContainsKey(item.Name))
                {
                    var innerKeys = innerKeysDic[item.Name];
                    item.InnerKeys = innerKeys;
                }
            }
        }

        /// <summary>
        /// Gets the column primary key name from table specified.
        /// </summary>
        private string GetPk(string table)
        {
            var sql = @"SELECT kcu.column_name 
			FROM information_schema.key_column_usage kcu
			JOIN information_schema.table_constraints tc
			ON kcu.constraint_name=tc.constraint_name
			WHERE lower(tc.constraint_type)='primary key'
			AND kcu.table_name=@tablename";

            using (var sqlCommand = new NpgsqlCommand(sql, _connection))
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
        /// Gets the property table based on the sql column type.
        /// </summary>
        private string GetPropertyType(string sqlType)
        {
            switch (sqlType)
            {
                case "int8":
                case "serial8":
                    return "long";

                case "bool":
                    return "bool";

                case "bytea	":
                    return "byte[]";

                case "float8":
                    return "double";

                case "int4":
                case "serial4":
                    return "int";

                case "money	":
                    return "decimal";

                case "numeric":
                    return "decimal";

                case "float4":
                    return "float";

                case "int2":
                    return "short";

                case "time":
                case "timetz":
                case "timestamp":
                case "timestamptz":
                case "date":
                    return "DateTime";

                default:
                    return "string";
            }
        }

        /// <summary>
        /// Sql query to get table schema info.
        /// </summary>
        private const string TableSql = @"
			SELECT table_name, table_schema, table_type
			FROM information_schema.tables 
			WHERE (table_type='BASE TABLE' OR table_type='VIEW')
				AND table_schema NOT IN ('pg_catalog', 'information_schema');
			";

        /// <summary>
        /// Sql query to get columns info.
        /// </summary>
        private const string ColumnSql = @"
			SELECT column_name, is_nullable, udt_name, column_default
			FROM information_schema.columns 
			WHERE table_name=@tableName;
			";


        /// <summary>
        /// Sql query to get foreign keys info.
        /// </summary>
        private const string ForeignKeysSql = @"
             SELECT
                tc.constraint_name AS CONSTRAINT_NAME, tc.table_name AS TABLE_NAME, kcu.column_name AS FKEY_FROM_COLUMN, 
                ccu.table_name AS FKEY_TO_TABLE,
                ccu.column_name AS FKEY_TO_COLUMN 
            FROM 
                information_schema.table_constraints AS tc 
                JOIN information_schema.key_column_usage AS kcu
                  ON tc.constraint_name = kcu.constraint_name
                JOIN information_schema.constraint_column_usage AS ccu
                  ON ccu.constraint_name = tc.constraint_name
            WHERE constraint_type = 'FOREIGN KEY' AND tc.table_name=@tableName
			    ";
    }
}
