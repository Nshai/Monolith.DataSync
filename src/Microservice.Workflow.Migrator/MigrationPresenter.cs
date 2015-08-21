using System;
using System.Threading.Tasks;
using ShellProgressBar;

namespace Microservice.Workflow.Migrator
{
    public class MigrationPresenter
    {
        public async Task Execute(IMigrator migrator)
        {
            ProgressBar progress = null;
            try
            {
                await migrator.Execute(i => progress = new ProgressBar(i, "Starting...", ConsoleColor.White, '\u2593'), s => progress.Tick(s));
            }
            finally
            {
                if(progress != null)
                    progress.Dispose();
            }
        }
    }
}
