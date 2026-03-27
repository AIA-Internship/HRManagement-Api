using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagement.Api.Domain.Models.Response.Shared
{
    public class QueryResultResponse
    {
        public int MessageCode { get; set; }

        public string MessageName { get; set; } = string.Empty;

        public Guid MessageId { get; set; }

        public int ID { get; set; }
    }
}
