# WPF Lite Poco Generator

This is a lite Poco generation tool that uses t4 template. It is "lite" because it is meant to support lite .NET ORM's like [Dapper](https://github.com/StackExchange/dapper-dot-net), [PetaPoco](http://www.toptensoftware.com/petapoco/), [OrmLite](http://ormlite.com/). It is designed to primary create models for the [Dapper.SimpleCRUD](https://github.com/ericdc1/Dapper.SimpleCRUD) project but can be easily modified for other lite ORM's. The solution extends the [template](https://github.com/ericdc1/Dapper.SimpleCRUD/wiki/T4-Template) provided by Dapper.SimpleCRUD for MsSql, and PetaPoco, OrmLite t4 templates for MySql and PostgreSql. I added implementation for Sqlite.

While Dapper.SimpleCRUD, PetaPoco, and OrmLite uses build time t4 processing, the solution uses runtime t4 processing - https://msdn.microsoft.com/en-us/library/ee844259.aspx

* The solution supports
  * MsSql
  * Sqlite
  * MySql
  * PostgresSql
  
There are 2 approaches - a WPF MVVM UI and a Console App. There are sample executable files in the Executables folder.

## WPF MVVM UI
The UI uses [Mahapps.Metro](http://mahapps.com/) WPF framework.

![](https://github.com/mattkol/wpf-lite-poco-gen/blob/master/PocoGenUI.png)

## Console App
The console app has the following usages:

```csharp
Usage:
poco_gen_console
         -dbtype:database type [mssql, sqlite, mysql, postgres]
         -conn_string:full connection string value
         -include_relationships:true|false [Optional]
         -namespace:namespace [Optional - deafult is database type + .Model]
         -models_location:full folder path for model [Optional - default is current location with database type + .Model subdirectory name]

or

poco_gen_console
         -dbtype:database type [mssql, sqlite, mysql, postgres]
         -conn_string_name:connection string name in app.config
         -include_relationships:true|false [Optional]
         -namespace:namespace [Optional - deafult is database type + .Model]
         -models_location:full folder path for model [Optional - default is current location with database type + .Model subdirectory name]
```
Sample usages:
```csharp
poco_gen_console  -dbtype:postgres -conn_string_name:PostgreSqlConnectionString
poco_gen_console  -dbtype:mysql -conn_string_name:MySqlConnectionString -include_relationships:false
poco_gen_console  -dbtype:sqlite -conn_string_name:SqliteConnectionString
poco_gen_console  -dbtype:mssql -conn_string_name:MsSqlConnectionString -namespace:Chinook.Models
poco_gen_console  -dbtype:mssql -conn_string:"Data Source=localhost; Initial Catalog=Chinook;Integrated Security=SSPI" 
```
## References
* Mahapps.Metro - http://mahapps.com/
* MVVM Light - http://www.mvvmlight.net/
* SpicyTaco.AutoGrid - https://github.com/kmcginnes/SpicyTaco.AutoGrid
* PropertyChanged.Fody - https://github.com/Fody/PropertyChanged
* Dapper.SimpleCRUD -https://github.com/ericdc1/Dapper.SimpleCRUD
* Dapper.SimpleCRUD t4 template -https://github.com/ericdc1/Dapper.SimpleCRUD/wiki/T4-Template
* Dapper - https://github.com/StackExchange/dapper-dot-net
* PetaPoco - http://www.toptensoftware.com/petapoco/
* OrmLite - http://ormlite.com/

