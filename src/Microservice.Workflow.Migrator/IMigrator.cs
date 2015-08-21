using System;
using System.Threading.Tasks;

namespace Microservice.Workflow.Migrator
{
    public interface IMigrator 
    {
        Task Execute(Action<int> intialise, Action<string> tick);
    }
}