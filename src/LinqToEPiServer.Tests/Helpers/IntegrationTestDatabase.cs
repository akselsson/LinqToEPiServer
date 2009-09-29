using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace LinqToEPiServer.Tests.Helpers
{
    public class IntegrationTestDatabase
    {
        private const string _connectionString =
            @"Data Source=.;integrated security=true;MultipleActiveResultSets=True;Initial Catalog=LinqToEPiServer_IntegrationTests";

        public const string BackupScriptFormat =
            @"
RESTORE DATABASE [LinqToEPiServer_IntegrationTests] 
FROM  DISK = N'{0}\LinqToEPiServer_IntegrationTests.bak' 
WITH  FILE = 1,  
MOVE N'Finnish_Swedish_CI_AS_dat' TO N'{0}\LinqToEPiServer_IntegrationTests.mdf',  
MOVE N'Finnish_Swedish_CI_AS_log' TO N'{0}\LinqToEPiServer_IntegrationTests_1.ldf',  
NOUNLOAD,  
STATS = 10
";

        private const string MasterConnectionString = @"Data Source=.;integrated security=true;initial catalog=master";
        
        public static readonly DirectoryInfo DbFolder = new DirectoryInfo("DB");

        public static string ConnectionString
        {
            get { return _connectionString; }
        }

        public static void Restore()
        {
            string script = String.Format(BackupScriptFormat, DbFolder.FullName);
            using (var connection = new SqlConnection(MasterConnectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = script;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }
    }
}