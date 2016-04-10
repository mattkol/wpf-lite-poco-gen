// -----------------------------------------------------------------------
// <copyright file="ShellViewModel.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.UI
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Data.SQLite;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using MySql.Data.MySqlClient;
    using Npgsql;
    using PocoGen.UI.Models;

    /// <summary>
    /// This class represents ShellViewModel classs.
    /// </summary>
    public class ShellViewModel : ViewModelBase
    {
        private bool _useWindowsAuthentication;
        private List<ConnectionStringInfo> _connectionStringInfos;
        private DbInfoOption _dbInfoOption;

        public ShellViewModel()
        {
            DatabaseTypeSelectionChangedCommand = new RelayCommand(DatabaseTypeSelectionChanged);
            GenerateCommand = new RelayCommand(Generate, CanGenerate);
            SelectSqliteFileCommand = new RelayCommand(SelectSqliteFile);
            SelectModelFolderLocationCommand = new RelayCommand(SelectModelFolderLocation);
            FileSelectionChangedCommand = new RelayCommand(FileSelectionChanged);

            ReadAllConnectionStrings();

            var dbTypes = new List<string>();
            dbTypes.Add(DbServerType.MsSql.ToString());
            dbTypes.Add(DbServerType.Sqlite.ToString());
            dbTypes.Add(DbServerType.MySql.ToString());
            dbTypes.Add(DbServerType.Postgres.ToString());
            DatabaseTypeItems = new ObservableCollection<string>(dbTypes);
            SelectedDatabaseTypeItem = DbServerType.Sqlite.ToString();
            DatabaseTypeSelectionChanged();

            EnabledForAuthentication = true;
            UseWindowsAuthentication = false;
            PortRequired = false;
            DbInfoOption = DbInfoOption.ConnectionString;
            IncludeRelationship = true;
            ShowBuildrogress = false;
        }

        public RelayCommand GenerateCommand { get; private set; }
        public RelayCommand SelectSqliteFileCommand { get; private set; }
        public RelayCommand SelectModelFolderLocationCommand { get; private set; }
        public RelayCommand FileSelectionChangedCommand { get; private set; }
        public RelayCommand DatabaseTypeSelectionChangedCommand { get; private set; } 

        public ObservableCollection<string> DatabaseTypeItems { get; set; }
        public ObservableCollection<string> ConnectionStringItems { get; set; }
        public ObservableCollection<string> FileItems { get; set; }
        
        public object DocumentSource { get; set; }
        public string SelectedDatabaseTypeItem { get; set; }

        public string SelectedConnectionStringItem { get; set; }
        public string SelectedFile { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SqliteFile { get; set; }
        public string Namespace { get; set; }
        public string ModelsFolderLocation { get; set; }

        public int SelectedIndexDatabaseType { get; set; }
        public int SelectedIndexConnectionString  { get; set; }
        public DbInfoOption DbInfoOption
        {
            get { return _dbInfoOption; }
            set
            {
                DbInfoEntryOption = value;
                if (SelectedDatabaseTypeItem == DbServerType.Sqlite.ToString())
                {
                    if(value == DbInfoOption.Manually)
                    {
                        DbInfoEntryOption = DbInfoOption.File;
                    }
                }
                _dbInfoOption = value;
            }
        }

        public DbInfoOption DbInfoEntryOption { get; set; }
        

        public bool ShowBuildrogress { get; set; }
        public bool EnabledForAuthentication { get; set; }
        public bool PortRequired { get; set; }
        public bool IncludeRelationship { get; set; }
        
        public bool UseWindowsAuthentication
        {
            get { return _useWindowsAuthentication; }
            set
            {
                 _useWindowsAuthentication = value;
                EnabledForAuthentication = !_useWindowsAuthentication;
            }
        }

                   
        private async void Generate()
        {
            try
            {
                // Reset content
                FileItems = new ObservableCollection<string>();
                SelectedFile = string.Empty;

                ShowBuildrogress = true;
                var request = new GeneratePocoRequest();
                DbServerType serverType = (DbServerType)Enum.Parse(typeof(DbServerType), SelectedDatabaseTypeItem);
                request.ServerType = serverType;
                request.Namespace = Namespace;
                request.FolderLocation = ModelsFolderLocation;
                request.IncludeRelationship = IncludeRelationship;
                request.ConnectionString = GetConnectionString(serverType);

                if (string.IsNullOrEmpty(request.ConnectionString))
                {
                    throw new Exception("Invalid database info provided!");
                }

                var pocoResponse = await GeneratePocoModels.Generate(request);

                if (pocoResponse != null && pocoResponse.Filenames != null)
                {
                    var fileNames = pocoResponse.Filenames;
                    FileItems = new ObservableCollection<string>(fileNames);
                    SelectedFile = string.Empty;
                    if (fileNames.Count > 0)
                    {
                        SelectedFile = fileNames[0];
                        FileSelectionChanged();
                    }
                }
            }
            finally
            {
                ShowBuildrogress = false;
            }
        }

        private bool CanGenerate()
        {
            if (string.IsNullOrEmpty(ModelsFolderLocation) ||
                string.IsNullOrEmpty(Namespace))
            {
                return false;
            }

            if (DbInfoOption == DbInfoOption.ConnectionString)
            {
                if (string.IsNullOrEmpty(SelectedDatabaseTypeItem))
                {
                    return false;
                }
            }
            else
            {
                var serverType = (DbServerType)Enum.Parse(typeof(DbServerType), SelectedDatabaseTypeItem);
                if (serverType == DbServerType.Sqlite)
                {
                    if (string.IsNullOrEmpty(SqliteFile))
                    {
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Server) ||
                        string.IsNullOrEmpty(Database))
                    {
                        return false;
                    }

                    if (!UseWindowsAuthentication)
                    {
                        if (string.IsNullOrEmpty(Username) ||
                            string.IsNullOrEmpty(Password))
                        {
                            return false;
                        }
                    }

                    if (PortRequired)
                    {
                        if (string.IsNullOrEmpty(Port))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void DatabaseTypeSelectionChanged()
        {
            List<ConnectionStringInfo> connectionStringInfos = _connectionStringInfos.Where(x => x.DbServerType.ToString() == SelectedDatabaseTypeItem).ToList();
            var stringInfos = connectionStringInfos.Select(x => x.Name).ToList();
            ConnectionStringItems = new ObservableCollection<string>(stringInfos);
            SelectedConnectionStringItem = (stringInfos.Count > 0) ? stringInfos[0] : string.Empty;
            Namespace = SelectedDatabaseTypeItem + ".Models";
            string dir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string currentWorkingDir = System.IO.Path.GetDirectoryName(dir);
            if (currentWorkingDir != null)
            {
                ModelsFolderLocation = Path.Combine(currentWorkingDir, Namespace);
            }
            DbInfoEntryOption = DbInfoOption;
            if (SelectedDatabaseTypeItem == DbServerType.Sqlite.ToString())
            {
                if (DbInfoOption == DbInfoOption.Manually)
                {
                    DbInfoEntryOption = DbInfoOption.File;
                }
            }
            PortRequired = (DbServerType.MySql.ToString() == SelectedDatabaseTypeItem) || (DbServerType.Postgres.ToString() == SelectedDatabaseTypeItem);
        }

        private void SelectSqliteFile()
        {
            // Select sqlite file
            var dlg = new OpenFileDialog();
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Set file 
                SqliteFile = dlg.FileName;
            }
        }

        private void FileSelectionChanged()
        {
             var paragraph = new Paragraph();
            paragraph.FontSize = 12;
            paragraph.FontStyle = FontStyles.Italic;

            if(!string.IsNullOrEmpty(SelectedFile))
            {
                string fileFullPath = Path.Combine(ModelsFolderLocation, SelectedFile);
                paragraph.Inlines.Add(System.IO.File.ReadAllText(fileFullPath));
            }
            else
            {
                paragraph.Inlines.Add(string.Empty);
            }

            var document = new FlowDocument(paragraph);
            DocumentSource = document;
        }
        
        private void SelectModelFolderLocation()
        {
            var folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            var result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                var newFolder = folderDlg.SelectedPath;

                if (!string.IsNullOrEmpty(newFolder))
                {
                    ModelsFolderLocation = newFolder;
                }
            }
        }

        // Read all connection strings found in the App.config
        private void ReadAllConnectionStrings()
        {
            _connectionStringInfos = new List<ConnectionStringInfo>();
            var connectionStringSettings = ConfigurationManager.ConnectionStrings;
            if (connectionStringSettings != null)
            {
                foreach (ConnectionStringSettings setting in connectionStringSettings)
                {
                    string providerName = setting.ProviderName;
                    var serverType = DbServerType.MsSql;
                    bool validProviderFound = false;
                    if (!string.IsNullOrEmpty(providerName))
                    {
                        if (string.Compare(providerName, DbServerProviderName.MsSql, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            serverType = DbServerType.MsSql;
                            validProviderFound = true;
                        }
                        else if (string.Compare(providerName, DbServerProviderName.Sqlite, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            serverType = DbServerType.Sqlite;
                            validProviderFound = true;
                        }
                        else if (string.Compare(providerName, DbServerProviderName.MySql, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            serverType = DbServerType.MySql;
                            validProviderFound = true;
                        }
                        else if (string.Compare(providerName, DbServerProviderName.Postgres, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            serverType = DbServerType.Postgres;
                            validProviderFound = true;
                        }
                    }

                    if (validProviderFound)
                    {
                        var connectionStringInfo = new ConnectionStringInfo();
                        connectionStringInfo.DbServerType = serverType;
                        connectionStringInfo.Name = setting.Name;
                        connectionStringInfo.ProviderName = providerName;
                        connectionStringInfo.ConnectionString = setting.ConnectionString;
                        _connectionStringInfos.Add(connectionStringInfo);
                    }
                }
            }
        }


        private string GetConnectionString(DbServerType serverType)
        {
            string connectionString = string.Empty;
            SQLiteConnectionStringBuilder sqliteStringBuilder;
            switch (DbInfoOption)
            {
                case DbInfoOption.ConnectionString:
                    connectionString = ConfigurationManager.ConnectionStrings[SelectedConnectionStringItem].ConnectionString;
                    break;
                case DbInfoOption.Manually:
                    if (serverType == DbServerType.Sqlite)
                    {
                        sqliteStringBuilder = new SQLiteConnectionStringBuilder();
                        sqliteStringBuilder.DataSource = SqliteFile;
                        connectionString = sqliteStringBuilder.ConnectionString;
                    }
                    else
                    {
                        connectionString = BuildConnectionString(serverType);  
                    }

                    break;
                case DbInfoOption.File:
                    sqliteStringBuilder = new SQLiteConnectionStringBuilder();
                    sqliteStringBuilder.DataSource = SqliteFile;
                    connectionString = sqliteStringBuilder.ConnectionString;
                    break;
            }

            return connectionString;
        }

        private string BuildConnectionString(DbServerType serverType)
        {
            string connectionString = string.Empty;
            switch (serverType)
            {
                case DbServerType.MsSql:
                    var msSqlStringBuilder = new SqlConnectionStringBuilder();
                    if (UseWindowsAuthentication)
                    {
                        msSqlStringBuilder.DataSource = Server;
                        msSqlStringBuilder.InitialCatalog = Database;
                        msSqlStringBuilder.IntegratedSecurity = true;
                    }
                    else
                    {
                        msSqlStringBuilder.DataSource = Server;
                        msSqlStringBuilder.InitialCatalog = Database;
                        msSqlStringBuilder.UserID = Username;
                        msSqlStringBuilder.Password = Password;
                        msSqlStringBuilder.IntegratedSecurity = false;
                    }
                    connectionString = msSqlStringBuilder.ConnectionString;
                    break;
                case DbServerType.MySql:
                    var mySqlStringBuilder = new MySqlConnectionStringBuilder();
                    mySqlStringBuilder.Server = Server;
                    mySqlStringBuilder.Database = Database;
                    mySqlStringBuilder.Port = (uint)Convert.ToInt32(Port);
                    mySqlStringBuilder.UserID = Username;
                    mySqlStringBuilder.Password = Password;
                    connectionString = mySqlStringBuilder.ConnectionString;
                    break;
                case DbServerType.Postgres:
                    var postgresStringBuilder = new NpgsqlConnectionStringBuilder();
                    postgresStringBuilder.Host = Server;
                    postgresStringBuilder.Database = Database;
                    postgresStringBuilder.Port = Convert.ToInt32(Port);
                    postgresStringBuilder.UserName = Username;
                    postgresStringBuilder.Password = Password;
                    connectionString = postgresStringBuilder.ConnectionString;
                    break;
            }

            return connectionString;
        }
    }
}