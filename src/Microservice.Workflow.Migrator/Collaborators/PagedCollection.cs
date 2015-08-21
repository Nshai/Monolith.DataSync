using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microservice.Workflow.Migrator.Collaborators
{
    public class PagedCollection<T> where T : IRepresentation
    {
        public PagedCollection()
        {
            Items = new List<T>();
        }

        public int Count { get; set; }

        [JsonProperty("items")]
        public IEnumerable<T> Items { get; set; }
    }
}
