using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagement.Api.Domain.Models.Response.Shared
{
    public class ApiException : Exception
    {
        public ApiException()
        {

        }
        public string Title { get; set; }

        public int StatusCode { get; set; }

        public ServiceResult Result { get; set; }
    }
}
