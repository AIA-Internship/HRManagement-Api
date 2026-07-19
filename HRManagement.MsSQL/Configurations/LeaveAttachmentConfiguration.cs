using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HRManagement.MsSQL.Configurations;

public class LeaveAttachmentConfiguration : IEntityTypeConfiguration<LeaveAttachment>
{
    public void Configure(EntityTypeBuilder<LeaveAttachment> builder)
    {
        builder.ToTable("LeaveAttachment");

        builder.HasKey(x => x.AttachmentId);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.ModifiedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}