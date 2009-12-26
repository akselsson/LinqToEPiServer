using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace LinqToEPiServer.Tests.Helpers
{
    public class IntegrationTestDatabase
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["IntegrationTests"].ConnectionString; }
        }
    }
}