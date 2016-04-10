// -----------------------------------------------------------------------
// <copyright file="SqliteSchemaReader.cs" company="Poco Generator for Lite ORMs">
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
    using System.Data.SQLite;
    using System.Linq;
    using Models;

    /// <summary>
    /// This class represents a Sqlite database schema reader.
    /// Class inherits abstract class SchemaReader.
    /// </summary>
    public class SqliteSchemaReader : SchemaReader
    {
        /// <summary>
        /// Sqlite connection object.
        /// </summary>
        private SQLiteConnection _connection;

        /// <summary>
        /// Reads the Schema returning all tables in the databse.
        /// </summary>
        public override Tables ReadSchema(string connectionString)
        {
            _connection = new SQLiteConnection(connectionString);
            Tables result;
            _connection.Open();

            var tablesInfoDataTable = _connection.GetSchema("Tables");
            var columnsInfoDataTable = _connection.GetSchema("Columns");
            var referencesInfoDataTable = _connection.GetSchema("ForeignKeys");

            result = LoadTables(tablesInfoDataTable);
            LoadColumns(result, columnsInfoDataTable);
            LoadReferencesKeysInfo(result, referencesInfoDataTable);

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
        /// Reads the Schema returning all tables in the databse.
        /// </summary>
        private Tables LoadTables(DataTable dataTable)
        {
            var tables = new Tables();

            // get table names and schema names
            foreach (DataRow row in dataTable.Rows)
            {
                string schemaName = string.Empty;
                string tableName = string.Empty;
                string tableType = string.Empty;

                foreach (DataColumn col in dataTable.Columns)
                {
                    switch (col.ColumnName.ToUpper())
                    {
                        case "TABLE_SCHEMA":
                            schemaName = (row[col] == null || row[col] is DBNull) ? string.Empty : (string)row[col];
                            break;
                        case "TABLE_NAME":
                            tableName = (row[col] == null || row[col] is DBNull) ? string.Empty : (string)row[col];
                            break;
                        case "TABLE_TYPE":
                            tableType = (row[col] == null || row[col] is DBNull) ? string.Empty : (string)row[col];
                            break;
                    }
                }

                bool isTable = string.Compare(tableType, "table", StringComparison.CurrentCultureIgnoreCase) == 0;
                bool isView = string.Compare(tableType, "view", StringComparison.CurrentCultureIgnoreCase) == 0;
                if (isTable || isView)
                {
                    Table tbl = new Table();
                    tbl.Name = tableName;
                    tbl.Schema = schemaName;
                    tbl.IsView = isView;
                    tbl.CleanName = Utils.CleanName(tbl.Name);
                    tbl.ClassName = Utils.CleanNameToClassName(tbl.CleanName);
                    tables.Add(tbl);
                }
            }

            return tables;
        }

        /// <summary>
        /// Reads all columns from columns data table info.
        /// </summary>
        private void LoadColumns(Tables tables, DataTable dataTable)
        {
            // Read columns for each table
            //loop again - but this time pull by table name
            foreach (var item in tables)
            {
                item.Columns = new List<Column>();

                //pull the columns from the schema
                var columns = dataTable.Select("TABLE_NAME='" + item.Name + "'");
                foreach (DataRow row in columns)
                {
                    var col = new Column();
                    col.Name = row["COLUMN_NAME"].ToString();
                    col.PropertyName = Utils.CleanUp(col.Name);
                    col.PropertyType = GetPropertyType(row["DATA_TYPE"].ToString());
                    col.IsNullable = string.Compare(row["IS_NULLABLE"].ToString(), "True", StringComparison.CurrentCultureIgnoreCase) == 0;
                    col.IsAutoIncrement = string.Compare(row["AUTOINCREMENT"].ToString(), "True", StringComparison.CurrentCultureIgnoreCase) == 0;
                    col.IsPk = string.Compare(row["PRIMARY_KEY"].ToString(), "True", StringComparison.CurrentCultureIgnoreCase) == 0;
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
                    if (string.Compare(row["CONSTRAINT_TYPE"].ToString(), "FOREIGN KEY", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        // Outer keys
                        var outerKey = new Key();
                        outerKey.Name = row["CONSTRAINT_NAME"].ToString();
                        var referencedTable = row["FKEY_TO_TABLE"].ToString();
                        outerKey.ReferencedTableName = referencedTable;
                        outerKey.ReferencedTableColumnName = row["FKEY_TO_COLUMN"].ToString();
                        outerKey.ReferencingTableColumnName = row["FKEY_FROM_COLUMN"].ToString();
                        item.OuterKeys.Add(outerKey);

                        var innerKey = new Key();
                        innerKey.Name = row["CONSTRAINT_NAME"].ToString();
                        innerKey.ReferencingTableName = row["TABLE_NAME"].ToString();
                        innerKey.ReferencedTableColumnName = row["FKEY_TO_COLUMN"].ToString();
                        innerKey.ReferencingTableColumnName = row["FKEY_FROM_COLUMN"].ToString();

                        // add to inner keys references
                        if (innerKeysDic.ContainsKey(referencedTable))
                        {
                            var innerKeys = innerKeysDic[referencedTable];
                            innerKeys.Add(innerKey);
                            innerKeysDic[referencedTable] = innerKeys;
                        }
                        else
                        {
                            var innerKeys= new List<Key>();
                            innerKeys.Add(innerKey);
                            innerKeysDic[referencedTable] = innerKeys;
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
                case "integer":
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
     }

}
