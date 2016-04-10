// -----------------------------------------------------------------------
// <copyright file="ModelsGenerator.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Console
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using PocoGen.Readers;
    using PocoGen.Templates;

    /// <summary>
    /// This class represents ModelsGenerator class.
    /// </summary>
    public class ModelsGenerator
    {
        public static void Generate(ReaderParameters readerParameters)
        {
            using (var schemaReader = SchemaReaderProvider.GetReader(readerParameters.ServerType))
            {
                var connectionString = readerParameters.ConnectionString;
                if (readerParameters.UseConnectionStringName)
                {
                    connectionString = ConfigurationManager.ConnectionStrings[readerParameters.ConnectionStringName].ConnectionString;
                }

                if (schemaReader == null || string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("Schema reader object or connection ring not valid!");
                }

                if (string.IsNullOrEmpty(readerParameters.Namespace))
                {
                    readerParameters.Namespace = readerParameters.ServerType.ToString() + ".Models";
                }
                if (string.IsNullOrEmpty(readerParameters.ModelsLocation))
                {
                    readerParameters.ModelsLocation = readerParameters.ServerType.ToString() + ".Models";
                }

                var tables = schemaReader.ReadSchema(connectionString);

                foreach (var table in tables)
                {
                    var model = new ModelTemplate();
                    model.Namespace = readerParameters.Namespace;
                    model.IncludeRelationships = readerParameters.IncludeRelationships;
                    model.Table = table;
                    model.Tables = tables;

                    // get page content
                    string pageContent = model.TransformText();


                    if (!Directory.Exists(readerParameters.ModelsLocation))
                        Directory.CreateDirectory(readerParameters.ModelsLocation);

                    // Write model to file
                    string fileName = table.ClassName + ".cs";
                    Console.WriteLine(string.Format("Creating file {0} ...", fileName));
                    File.WriteAllText(Path.Combine(readerParameters.ModelsLocation, fileName), pageContent);
                }
            }
        }
    }
}
