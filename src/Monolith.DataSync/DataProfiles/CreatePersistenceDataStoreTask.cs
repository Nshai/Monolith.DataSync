using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using IntelliFlo.Platform.Database;

namespace Microservice.DataSync.DataProfiles
{
    public class CreatePersistenceDataStoreTask : TaskBase<CreatePersistenceDataStoreTask>
    {
        private readonly ConnectionStringSettings connStr;
        private readonly bool DropDatabase;
        private readonly bool UseSafetyCheck;
        public override void Dispose() { }

        private const string createDatabaseSql = "IF (SELECT DB_ID('afper')) IS NULL CREATE DATABASE afper";

        private const string dropDatabaseSql = @"
            IF db_id('afper') IS NOT NULL 
            BEGIN 
                ALTER DATABASE afper SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [afper]
            END";

        private const string safetyCheckSql = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

        public CreatePersistenceDataStoreTask(bool dropDatabase = false, bool useSafetyCheck = true)
        {
            connStr = ConfigurationManager.ConnectionStrings["afper"];
            DropDatabase = dropDatabase;
            UseSafetyCheck = useSafetyCheck;
        }

        public override object Execute(IDatabaseSettings settings)
        {
            if (false == string.Equals("true", ConfigurationManager.AppSettings["CreateAfperDatabase"], StringComparison.OrdinalIgnoreCase))
            {
                Logger.WarnFormat("Skipping execution of 'SqlWorkflowInstanceStoreSchema.sql' and 'SqlWorkflowInstanceStoreLogic.sql' since both scripts deleted all existing data.");
                return true;
            }

            if (IsSafeToDeploy() == false)
            {
                Logger.WarnFormat("Skipping execution because the creation scripts might delete existing data.");
                return true;
            }

            if (connStr != null)
            {
                // delete and create 'afper' database
                RecreateDatabase(settings);

                var sql = (NameValueCollection)ConfigurationManager.GetSection("SqlAfper");

                if (sql != null)
                {
                    Logger.InfoFormat("running script to create 'afper' schema...");
                    RunSqlScript(sql.Get("schema"));
                    Logger.InfoFormat("running script to create 'afper' logic...");
                    RunSqlScript(sql.Get("logic"));
                }
                else
                {
                    Logger.WarnFormat("SqlAfper config section was not found");
                }
            }

            return true;
        }

        private bool IsSafeToDeploy()
        {
            if (UseSafetyCheck == false) 
            {
                Logger.WarnFormat("Skipping safety check, potential DATA LOST on 'afper' can occur on schema update.");
                return true;
            }

            var con = new SqlConnection(connStr.ConnectionString);

            try
            {
                con.Open();

                using (var cmd = new SqlCommand(safetyCheckSql, con))
                {
                    var nrOfTablesInAfper = (int)cmd.ExecuteScalar();

                    // it's only safe to deplooy schema if afper does not have any tables (means empty schema)
                    return nrOfTablesInAfper == 0;
                }

            }
            finally
            {
                con.Close();
            }
        }

        private void RunSqlScript(string script)
        {
            var file = new FileInfo(script);
            if (file.Exists == false)
            {
                Logger.WarnFormat("Failed to run sql script '{0}' because the files does not exist", file.FullName);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(file.FullName);
                ExecuteNonQueries(GetStatements(lines).Where(statement => !string.IsNullOrWhiteSpace(statement)));
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed executing script: '{file}'", ex);
                throw;
            }
        }

        private static IEnumerable<string> GetStatements(IEnumerable<string> lines)
        {
            var statements = new List<StringBuilder> { new StringBuilder() };

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("GO", StringComparison.OrdinalIgnoreCase)
                    && trimmed.EndsWith("GO", StringComparison.OrdinalIgnoreCase))
                {
                    statements.Add(new StringBuilder());
                    continue;
                }
                statements.Last().AppendLine(line);
            }

            return statements.Select(b => b.ToString());
        }

        private void ExecuteNonQueries(IEnumerable<string> sql)
        {
            var con = new SqlConnection(connStr.ConnectionString);

            try
            {
                con.Open();

                foreach (var statement in sql)
                {
                    using (var cmd = new SqlCommand(statement, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                con.Close();
            }
        }

        private void RecreateDatabase(IDatabaseSettings settings)
        {
            var result = settings.Clone(s =>
            {
                var conn = new SqlConnectionStringBuilder(settings.ConnectionString)
                {
                    InitialCatalog = "master"
                };

                s.ConnectionString = conn.ConnectionString;
                return s;
            });

            var scripts = new List<string>();

            if (DropDatabase)
            {
                Logger.InfoFormat("DatabaseStartUp, Msg=Dropping database 'afper'");
                scripts.Add(dropDatabaseSql);
            }

            Logger.InfoFormat("DatabaseStartUp, Msg=Creating database 'afper'");
            scripts.Add(createDatabaseSql);

            var con = new SqlConnection(result.ConnectionString);

            try
            {
                con.Open();

                foreach (var statement in scripts)
                {
                    using (var cmd = new SqlCommand(statement, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                con.Close();
            }
        }


    }
}
