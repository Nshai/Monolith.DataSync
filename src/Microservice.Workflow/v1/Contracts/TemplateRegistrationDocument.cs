using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateRegistrationDocument
    {
        public string Identifier { get; set; }
        public Guid TemplateId { get; set; }
    }
}
