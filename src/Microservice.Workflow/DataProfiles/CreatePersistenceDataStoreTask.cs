using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using IntelliFlo.Platform.Database;

namespace Microservice.Workflow.DataProfiles
{
    public class CreatePersistenceDataStoreTask : TaskBase<CreatePersistenceDataStoreTask>
    {
        private readonly ConnectionStringSettings connStr;
        private readonly bool DropDatabase;
        public override void Dispose() {}

        public CreatePersistenceDataStoreTask(bool dropDatabase = true)
        {
            connStr = ConfigurationManager.ConnectionStrings["afper"];
            DropDatabase = dropDatabase;
        }

        public override object Execute(IDatabaseSettings settings)
        {
            if (connStr != null)
            {
                // delete and create 'afper' database
                RecreateDatabase(settings);

                var sql = (NameValueCollection)ConfigurationManager.GetSection("SqlAfper");
                if(sql != null)
                {
                    for (var i = 0; i < sql.Count; i++)
                    {
                        RunSqlScript(sql[i]);
                    }
                }
                else
                {
                    Logger.WarnFormat("SqlAfper config section was not found");
                }
            }

            return true;
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


            var dropSql = @"
            IF db_id('afper') IS NOT NULL 
            BEGIN 
                ALTER DATABASE afper SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [afper]
            END";


            var createSql = "IF (SELECT DB_ID('afper')) IS NULL CREATE DATABASE afper";

            Logger.InfoFormat("DatabaseStartUp, Msg=Recreating database 'afper'");

            var scripts = new List<string>();

            if (DropDatabase) scripts.Add(dropSql);
            
            scripts.Add(createSql);

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
