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
    public class UpgradePersistenceDataStoreSchemaTask : TaskBase<UpgradePersistenceDataStoreSchemaTask>
    {
        private readonly ConnectionStringSettings connStr;

        public UpgradePersistenceDataStoreSchemaTask()
        {
            connStr = ConfigurationManager.ConnectionStrings["afper"];
        }

        public override void Dispose() { }

        public override object Execute(IDatabaseSettings settings)
        {
            if (false == string.Equals("true", ConfigurationManager.AppSettings["UpgradeAfperDatabase"], StringComparison.OrdinalIgnoreCase))
            {
                Logger.WarnFormat("Skipping execution of 'SqlWorkflowInstanceStoreSchemaUpgrade.sql' since 'UpgradeAfperDatabase' is not set to true.");
                return true;
            }

            if (connStr != null)
            {
                var sql = (NameValueCollection)ConfigurationManager.GetSection("SqlAfper");

                if (sql != null)
                {
                    Logger.InfoFormat("running script to update 'afper' schema...");
                    RunSqlScript(sql.Get("schemaUpgrade"));
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

    }
}
