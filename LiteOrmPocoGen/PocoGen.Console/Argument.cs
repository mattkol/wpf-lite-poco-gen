// -----------------------------------------------------------------------
// <copyright file="Argument.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Console
{
    using System;

    /// <summary>
    /// This class represents Argumentc class.
    /// </summary>
    public class Argument
    {
        public const string DbTypeArgTag = "dbtype";
        public const string ConnStringArgTag = "conn_string";
        public const string ConnStringNameArgTag = "conn_string_name";
        public const string IncludeRelationshipsArgTag = "include_relationships";
        public const string NamespaceArgTag = "namespace";
        public const string ModelsLocationArgTag = "models_location";

        public string Name { get; set; }
        public string Value { get; set; }

        public bool Required
        {
            get
            {
                switch (Name)
                {
                    case DbTypeArgTag:
                    case ConnStringArgTag:
                    case ConnStringNameArgTag:
                        return true;
                }

                return false;
            }
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Value))
            {
                return false;
            }

            switch (Name)
            {
                case DbTypeArgTag:
                case ConnStringArgTag:
                case ConnStringNameArgTag:
                case NamespaceArgTag:
                case ModelsLocationArgTag:
                    return true;
                case IncludeRelationshipsArgTag:
                    if (string.Compare("true", Value, StringComparison.CurrentCultureIgnoreCase) == 0 ||
                        string.Compare("false", Value, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
