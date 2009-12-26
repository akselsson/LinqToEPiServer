using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace LinqToEPiServer.Tests.Helpers
{
    public class IntegrationTestDatabase
    {
        public const string ConnectionString =
     @"Data Source=.\sqlexpress;integrated security=true;MultipleActiveResultSets=True;Initial Catalog=LinqToEPiServer_IntegrationTests";
    }
}