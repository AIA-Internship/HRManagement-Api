using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public record PerformanceReviewEmployeeDto
    (
        int EmployeeId,

        string EmployeeDisplayId,

        string FullName,

        string Department,

        string Position
    );
}
