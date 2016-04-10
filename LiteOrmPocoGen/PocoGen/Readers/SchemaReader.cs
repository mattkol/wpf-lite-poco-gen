// -----------------------------------------------------------------------
// <copyright file="SchemaReader.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// This is code is based on the T4 template from the PetaPoco project which in turn is based on the subsonic project.
// This is adapted from OrmLite T4 and Dapper.SimpleCRUD Projects.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace PocoGen.Readers
{
    using Models;

    /// <summary>
    /// This abstract class for schema reading.
    /// </summary>
    public abstract class SchemaReader : IDisposable
    {
        /// <summary>
        /// Reads the Schema returning all tables in the databse.
        /// </summary>
        public abstract Tables ReadSchema(string connectionString);

        /// <summary>
        /// Disposes of objects
        /// </summary>
        public abstract void Dispose();
    }
}
