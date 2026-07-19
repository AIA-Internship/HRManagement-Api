using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.Models.Tables.ELearningModels;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Base;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<Users> Users { get; set; }
    public virtual DbSet<Roles> Roles { get; set; }
    public virtual DbSet<Permission> Permission { get; set; }
    public virtual DbSet<RolePermission> RolePermission { get; set; }
    public virtual DbSet<Lookup> Lookup { get; set; }

    public virtual DbSet<Employee> Employee { get; set; }
    public virtual DbSet<EmergencyContact> EmergencyContact { get; set; }
    public virtual DbSet<EmployeeUpdateRequest> EmployeeUpdateRequest { get; set; }
    public virtual DbSet<EmploymentInformation> EmploymentInformation { get; set; }
    public virtual DbSet<EmployeeAttachment> EmployeeAttachment { get; set; }

    public virtual DbSet<ModuleModel> ELearningModules { get; set; } = null!;
    public virtual DbSet<ModuleContentModel> ELearningModuleContents { get; set; } = null!;
    public virtual DbSet<ProgressModel> ELearningModuleProgress { get; set; } = null!;
    public virtual DbSet<GroupModel> ELearningGroups { get; set; } = null!;
    public virtual DbSet<GroupMemberModel> ELearningGroupMembers { get; set; } = null!;
    public virtual DbSet<ProgramModel> ELearningPrograms { get; set; } = null!;
    public virtual DbSet<BatchModel> ELearningBatches { get; set; } = null!;
    public virtual DbSet<QuizModel> ELearningQuizzes { get; set; } = null!;
    public virtual DbSet<QuizQuestionModel> ELearningQuizQuestions { get; set; } = null!;
    public virtual DbSet<QuizQuestionOptionModel> ELearningQuizQuestionOptions { get; set; } = null!;
    public virtual DbSet<QuizSubmissionModel> ELearningQuizSubmissions { get; set; } = null!;
    public virtual DbSet<StudentAnswerModel> ELearningStudentAnswers { get; set; } = null!;
    public virtual DbSet<InternProfileModel> InternProfiles { get; set; } = null!;
    public virtual DbSet<ContentProgressModel> ELearningContentProgress { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<GroupMemberModel>().HasKey(gm => new { gm.GroupId, gm.EmployeeId });
    }
}