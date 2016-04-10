// -----------------------------------------------------------------------
// <copyright file="ReaderParameters.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class represents ReaderParameters class.
    /// </summary>
    public class ReaderParameters
    {
        public DbServerType ServerType { get; set; }
        public string ConnectionStringName { get; set; }
        public string ConnectionString { get; set; }
        public string Namespace { get; set; }
        public string ModelsLocation { get; set; }
        public bool IncludeRelationships { get; set; }
        public bool UseConnectionStringName { get; set; }

        public string ErrorMessage { get; set; }

        public ReaderParameters()
        {
            IncludeRelationships = true;
        }

        public string Usage
        {
            get
            {
                var builder = new StringBuilder();

                builder.AppendLine("\nUsage:\n");
                builder.AppendLine("poco_gen_console");
                builder.AppendLine("\t -dbtype:database type [mssql, sqlite, mysql, postgres]");
                builder.AppendLine("\t -conn_string:full connection string value");
                builder.AppendLine("\t -include_relationships:true|false [Optional]");
                builder.AppendLine("\t -namespace:namespace [Optional - deafult is database type + .Model]");
                builder.AppendLine("\t -models_location:full folder path for model [Optional - default is current location with database type + .Model subdirectory name]");
                builder.AppendLine("\nor\n");
                builder.AppendLine("poco_gen_console");
                builder.AppendLine("\t -dbtype:database type [mssql, sqlite, mysql, postgres]");
                builder.AppendLine("\t -conn_string_name:connection string name in app.config");
                builder.AppendLine("\t -include_relationships:true|false [Optional]");
                builder.AppendLine("\t -namespace:namespace [Optional - deafult is database type + .Model]");
                builder.AppendLine("\t -models_location:full folder path for model [Optional - default is current location with database type + .Model subdirectory name]");
                return builder.ToString();
            }
        }

        public ParserResult ParseArguments(string[] args)
        {
            try
            {
                if (args == null)
                {
                    return ParserResult.Invalid;
                }

                var argList = new List<string>(args);


                // get arguments list
                var arguments = new List<Argument>();
                foreach (var item in argList)
                {
                    int index = item.IndexOf(':');
                    if (index > 0)
                    {
                        var argItem = item.Trim();
                        // Starting at 1 to remove (-)
                        var name = argItem.Substring(1, index - 1);
                        var value = argItem.Substring(index + 1);

                        var argument = new Argument();
                        argument.Name = name;
                        argument.Value = value;

                        if (argument.IsValid())
                        {
                            arguments.Add(argument);
                        }
                    }
                }

                // Only 2 arguments are required - dbtype, conn_string|conn_string_name - others are optional
                if (arguments.Count(x => x.Required) < 2)
                {
                    return ParserResult.Invalid;
                }
                foreach (var argument in arguments)
                {
                    switch (argument.Name)
                    {
                        case Argument.DbTypeArgTag:
                            ServerType = GetServerType(argument.Value);
                            break;
                        case Argument.ConnStringArgTag:
                            ConnectionString = argument.Value;
                            break;
                        case Argument.ConnStringNameArgTag:
                            UseConnectionStringName = true;
                            ConnectionStringName = argument.Value;
                            break;
                        case Argument.NamespaceArgTag:
                            Namespace = argument.Value;
                            break;
                        case Argument.ModelsLocationArgTag:
                            ModelsLocation = argument.Value;
                            break;
                        case Argument.IncludeRelationshipsArgTag:
                            IncludeRelationships = Convert.ToBoolean(argument.Value);
                            break;
                    }

                }
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
                return ParserResult.Failure;
            }

            return ParserResult.Success;
        }

        private DbServerType GetServerType(string dbType)
        {
            switch (dbType)
            {
                case "mssql":
                    return DbServerType.MsSql;
                case "sqlite":
                    return DbServerType.Sqlite;
                case "mysql":
                    return DbServerType.MySql;
                case "postgres":
                    return DbServerType.Postgres;
            } 

           throw new Exception("Invalid database type specified!");
        }

    }
}
