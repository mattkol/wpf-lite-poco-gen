// -----------------------------------------------------------------------
// <copyright file="MySqlSchemaReader.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// This is code is based on the T4 template from the PetaPoco project which in turn is based on the subsonic project.
// This is adapted from OrmLite T4 and Dapper.SimpleCRUD Projects.
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using MySql.Data.MySqlClient;
    using System.Linq;
    using Models;

    /// <summary>
    /// This class represents a MySql database schema reader.
    /// Class inherits abstract class SchemaReader.
    /// </summary>
    public class MySqlSchemaReader : SchemaReader
    {
        /// <summary>
        /// MySql connection object.
        /// </summary>
         private MySqlConnection _connection;

        /// <summary>
        /// Reads the Schema returning all tables in the databse.
        /// </summary>
        public override Tables ReadSchema(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
            var result = new Tables();

            _connection.Open();

            using (var sqlCommand = new MySqlCommand(TableSql, _connection))
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

            //this will return everything for the DB
            var schema = _connection.GetSchema("COLUMNS");

            //loop again - but this time pull by table name
            foreach (var item in result)
            {
                item.Columns = new List<Column>();

                //pull the columns from the schema
                var columns = schema.Select("TABLE_NAME='" + item.Name + "'");
                foreach (var row in columns)
                {
                    Column col = new Column();
                    col.Name = row["COLUMN_NAME"].ToString();
                    col.PropertyName = Utils.CleanUp(col.Name);
                    col.PropertyType = GetPropertyType(row);
                    col.IsNullable = row["IS_NULLABLE"].ToString() == "YES";
                    col.IsPk = row["COLUMN_KEY"].ToString() == "PRI";
                    col.IsAutoIncrement = row["extra"].ToString().ToLower().IndexOf("auto_increment", System.StringComparison.CurrentCultureIgnoreCase) >= 0;
                    item.Columns.Add(col);
                }

                // Only table with single primary key is allowed for this implementation
                // number of columns that are valid primary keys
                int pkeyCount = item.Columns.Count(x => x.IsPk);
                if (pkeyCount > 1)
                {
                    foreach (var column in item.Columns)
                    {
                        column.IsPk = false;
                    }
                }
            }

            var referencesInfoDataTable = _connection.GetSchema("Foreign Key Columns");
            LoadReferencesKeysInfo(result, referencesInfoDataTable);

            return result;
        }

        /// <summary>
        /// Loads the reference keys info for the entire database.
        /// </summary>
        private void LoadReferencesKeysInfo(Tables tables, DataTable dataTable)
        {
            var innerKeysDic = new Dictionary<string, List<Key>>();
            foreach (var item in tables)
            {
                item.OuterKeys = new List<Key>();
                item.InnerKeys = new List<Key>();

                //pull the foreign key details from the schema
                var columns = dataTable.Select("TABLE_NAME='" + item.Name + "'");
                foreach (DataRow row in columns)
                {
                    // Outer keys
                    var outerKey = new Key();
                    outerKey.Name = row["CONSTRAINT_NAME"].ToString();
                    var referencedTable = row["REFERENCED_TABLE_NAME"].ToString();
                    outerKey.ReferencedTableName = referencedTable;
                    outerKey.ReferencedTableColumnName = row["REFERENCED_COLUMN_NAME"].ToString();
                    outerKey.ReferencingTableColumnName = row["COLUMN_NAME"].ToString();
                    item.OuterKeys.Add(outerKey);

                    var innerKey = new Key();
                    innerKey.Name = row["CONSTRAINT_NAME"].ToString();
                    innerKey.ReferencingTableName = row["TABLE_NAME"].ToString();
                    innerKey.ReferencedTableColumnName = row["REFERENCED_COLUMN_NAME"].ToString();
                    innerKey.ReferencingTableColumnName = row["COLUMN_NAME"].ToString();

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
        /// Gets the property type from the column.
        /// </summary>
        private static string GetPropertyType(DataRow row)
        {
            bool bUnsigned = row["COLUMN_TYPE"].ToString().IndexOf("unsigned", System.StringComparison.CurrentCultureIgnoreCase) >= 0;
            string propType = "string";
            switch (row["DATA_TYPE"].ToString())
            {
                case "bigint":
                    propType = bUnsigned ? "ulong" : "long";
                    break;
                case "int":
                    propType = bUnsigned ? "uint" : "int";
                    break;
                case "smallint":
                    propType = bUnsigned ? "ushort" : "short";
                    break;
                case "guid":
                    propType = "Guid";
                    break;
                case "smalldatetime":
                case "date":
                case "datetime":
                case "timestamp":
                    propType = "DateTime";
                    break;
                case "float":
                    propType = "float";
                    break;
                case "double":
                    propType = "double";
                    break;
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    propType = "decimal";
                    break;
                case "bit":
                case "bool":
                case "boolean":
                    propType = "bool";
                    break;
                case "tinyint":
                    propType = bUnsigned ? "byte" : "sbyte";
                    break;
                case "image":
                case "binary":
                case "blob":
                case "mediumblob":
                case "longblob":
                case "varbinary":
                    propType = "byte[]";
                    break;

            }
            return propType;
        }

        /// <summary>
        /// Sql query to get table schema info.
        /// </summary>
        private const string TableSql = @"
			SELECT * 
			FROM information_schema.tables 
			WHERE (table_type='BASE TABLE' OR table_type='VIEW') AND TABLE_SCHEMA=DATABASE()
			";

    }
   
}
