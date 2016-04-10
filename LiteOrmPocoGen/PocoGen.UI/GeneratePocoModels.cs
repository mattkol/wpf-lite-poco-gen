// -----------------------------------------------------------------------
// <copyright file="GeneratePocoModels.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.UI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Models;
    using Readers;
    using Templates;

    /// <summary>
    /// This class represents GeneratePocoModels class.
    /// </summary>
    public static class GeneratePocoModels
    {
        public static Task<GeneratePocoResponse> Generate(GeneratePocoRequest request)
        {
            return Task.Run(() =>
            {
                using (var schemaReader = SchemaReaderProvider.GetReader(request.ServerType))
                {
                    var connectionString = request.ConnectionString;
                    if (schemaReader == null || string.IsNullOrEmpty(connectionString))
                    {
                        return new GeneratePocoResponse();
                    }

                    var tables = schemaReader.ReadSchema(connectionString);

                    if (tables == null || tables.Count <= 0)
                    {
                        throw new Exception(string.Format("Empty database or invalid connection string: {0}", connectionString));
                    }

                    var fileNames = new List<string>();
                    foreach (var table in tables)
                    {
                        var model = new ModelTemplate();
                        model.Namespace = request.Namespace;
                        model.IncludeRelationships = request.IncludeRelationship;
                        model.Table = table;
                        model.Tables = tables;

                        // get page content
                        string pageContent = model.TransformText();


                        if (!Directory.Exists(request.FolderLocation))
                            Directory.CreateDirectory(request.FolderLocation);

                        // Write model to file
                        string fileName = table.ClassName + ".cs";
                        File.WriteAllText(Path.Combine(request.FolderLocation, fileName), pageContent);
                        fileNames.Add(fileName);
                    }

                    var response = new GeneratePocoResponse();
                    response.Filenames = fileNames;
                    return response;
                }
            });
        }
    }
}
