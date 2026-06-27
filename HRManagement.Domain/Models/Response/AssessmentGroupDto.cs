namespace HRManagement.Domain.Models.Response
{
    public record AssessmentGroupDto
    (
        int GroupId,

        string Name,

        string? Description,

        List<AssessmentGroupMemberDto> Members
    );
}