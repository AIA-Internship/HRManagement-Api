using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public class FillAssignmentResponseDto
    {
        public int AssignmentId { get; set; }
        public int IntervalId { get; set; }
        public int SubjectId { get; set; }
        public int AssessmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public PerformanceReviewPlanIntervalResponseDto? Interval { get; set; }
        public AssessmentBriefResponseDto? Assessment { get; set; }
    }
}
