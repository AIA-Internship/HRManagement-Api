using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class AssessmentGroupMemberConfiguration : IEntityTypeConfiguration<AssessmentGroupMember>
{
    public void Configure(EntityTypeBuilder<AssessmentGroupMember> builder)
    {
        builder.ToTable("AssessmentGroupMembers");

        builder.HasKey(x => x.Id);

        builder.HasOne<AssessmentGroup>()
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}