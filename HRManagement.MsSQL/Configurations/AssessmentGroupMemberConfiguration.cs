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

        builder.HasOne(x => x.Group)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Employee)
           .WithOne(x => x.AssessmentGroupMember)
           .HasForeignKey<AssessmentGroupMember>(x => x.EmployeeId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);


    }
}