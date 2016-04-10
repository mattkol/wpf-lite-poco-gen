// -----------------------------------------------------------------------
// <copyright file="ModelTemplate.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// This is code is based on the T4 template from the PetaPoco project which in turn is based on the subsonic project.
// This is adapted from OrmLite T4 and Dapper.SimpleCRUD Projects.
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Templates
{
    using Models;

    /// <summary>
    /// This class represents ModelTemplate class.
    /// This a partial class of the template class generated from ModelTemplate.tt.
    /// Note that the namespace - PocoGen.Templates matches the template file namespace.
    /// </summary>
    partial class ModelTemplate
    {
        public string Namespace { get; set; }
        public bool IncludeRelationships { get; set; }
        public Table Table { get; set; }
        public Tables Tables { get; set; }
    }
}