using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Payload
{
    public record CopyPerformanceReviewPlanPayload
    (
        string Name,
        string PeriodType,
        DateTime StartDate,
        DateTime EndDate,
        int DurationInMonth
    );

}
