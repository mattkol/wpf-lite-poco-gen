// -----------------------------------------------------------------------
// <copyright file="SchemaReaderProvider.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// This is code is based on the T4 template from the PetaPoco project which in turn is based on the subsonic project.
// This is adapted from OrmLite T4 and Dapper.SimpleCRUD Projects.
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Readers
{
    public class SchemaReaderProvider
    {
        /// <summary>
        /// This class represents a provier class to get right schema reader based on database type.
        /// </summary>
        public static SchemaReader GetReader(DbServerType dbServerType)
        {
            SchemaReader schemaReader = null;
            switch (dbServerType)
            {
                case DbServerType.MsSql:
                    schemaReader = new SqlServerSchemaReader();
                    break;
                case DbServerType.Sqlite:
                    schemaReader = new SqliteSchemaReader();
                    break;
                case DbServerType.MySql:
                    schemaReader = new MySqlSchemaReader();
                    break;
                case DbServerType.Postgres:
                    schemaReader = new PostgreSqlSchemaReader();
                    break;
            }

            return schemaReader;
        }
    }
}
