// -----------------------------------------------------------------------
// <copyright file="Tables.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// This is code is based on the T4 template from the PetaPoco project which in turn is based on the subsonic project.
// This is adapted from OrmLite T4 and Dapper.SimpleCRUD Projects.
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class represents all tables in the database.
    /// </summary>
    public class Tables : List<Table>
    {
        /// <summary>
        /// Gets a table based on table name.
        /// </summary>
        public Table GetTable(string tableName)
        {
            return this.Single(x => string.Compare(x.Name, tableName, true) == 0);
        }

        /// <summary>
        /// Gets a table based on table indexed name.
        /// </summary>
        public Table this[string tableName]
        {
            get
            {
                return GetTable(tableName);
            }
        }
    }
}
