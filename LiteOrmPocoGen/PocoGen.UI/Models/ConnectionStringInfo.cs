// -----------------------------------------------------------------------
// <copyright file="ConnectionStringInfo.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.UI.Models
{
    /// <summary>
    /// This class represents ConnectionStringInfo class.
    /// </summary>
    public class ConnectionStringInfo
    {
        public DbServerType DbServerType { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
    }
}
