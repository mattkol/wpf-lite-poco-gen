// -----------------------------------------------------------------------
// <copyright file="SqlServerSchemaReaderTest.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using Models;
    using Readers;
    using Templates;
    using Xunit;

    /// <summary>
    /// This class represents SqlServerSchemaReaderTest class.
    /// </summary>
    public class SqlServerSchemaReaderTest
    {
        private readonly string _connectionString;
        private const string Namespace = "MsSql.Models";

        public SqlServerSchemaReaderTest()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MsSqlConnectionString"].ConnectionString;
        }

        [Fact]
        public void TableCounTest()
        {
            using (var schemaReader = SchemaReaderProvider.GetReader(DbServerType.MsSql))
            {
                Tables tables = schemaReader.ReadSchema(_connectionString);

                Assert.NotNull(schemaReader);
                Assert.NotNull(tables);
                Assert.Equal(11, tables.Count);
            }
        }

        [Fact]
        public void CreateModelsTest()
        {
            using (var schemaReader = SchemaReaderProvider.GetReader(DbServerType.MsSql))
            {
                Tables tables = schemaReader.ReadSchema(_connectionString);

                Assert.NotNull(schemaReader);
                Assert.NotNull(tables);

                var fileFullPathList = new List<string>();
                foreach (var table in tables)
                {
                    var model = new ModelTemplate();
                    model.Namespace = Namespace;
                    model.IncludeRelationships = true;
                    model.Table = table;
                    model.Tables = tables;

                    // get page content
                    string pageContent = model.TransformText();

                    // your generated model files go here
                    string modelsPath = Namespace;

                    if (!Directory.Exists(modelsPath))
                        Directory.CreateDirectory(modelsPath);

                    // Write model to file
                    string fileFullPath = string.Format("{0}\\{1}", modelsPath, table.ClassName) + ".cs";
                    File.WriteAllText(fileFullPath, pageContent);

                    fileFullPathList.Add(fileFullPath);
                }

                // Check if files exits
                Assert.Equal(tables.Count, fileFullPathList.Count);

                foreach (var fileFullpath in fileFullPathList)
                {
                    Assert.True(File.Exists(fileFullpath));
                }
            }
        }
    }
}
