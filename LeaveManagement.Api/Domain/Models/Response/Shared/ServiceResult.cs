using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagement.Api.Domain.Models.Response.Shared
{
    public class ServiceResult
    {
        public string? Message { get; set; }

        public bool IsError { get; set; }

        public dynamic? Content { get; set; }
    }
}
