// -----------------------------------------------------------------------
// <copyright file="GeneratePocoRequest.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.UI.Models
{
    /// <summary>
    /// This class represents GeneratePocoRequest class.
    /// </summary>
    public class GeneratePocoRequest
    {
        public DbServerType ServerType { get; set; }
        public string ConnectionString { get; set; }
        public string Namespace { get; set; }
        public string FolderLocation { get; set; }
        public bool IncludeRelationship { get; set; }
    }
}

