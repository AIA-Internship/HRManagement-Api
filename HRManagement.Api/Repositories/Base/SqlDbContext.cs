using HRManagement.Api.Domain.Models.Table;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace HRManagement.Api.Repositories.Base
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<UserModel>().ToTable("User");
            modelBuilder.Entity<UserModel>().HasKey(p => p.UserId);

            modelBuilder.Entity<RoleModel>().ToTable("Role");
            modelBuilder.Entity<RoleModel>().HasKey(p => p.RoleId);

            modelBuilder.Entity<EmployeeModel>().ToTable("Employee");
            modelBuilder.Entity<EmployeeModel>().HasKey(p => p.Id);

            modelBuilder.Entity<LoginActivityModel>().ToTable("LoginActivity");
            modelBuilder.Entity<LoginActivityModel>().HasKey(p => p.Id);


            modelBuilder.Entity<ModuleModel>().ToTable("ELearningModules");
            modelBuilder.Entity<ModuleModel>().HasKey(p => p.ModuleId);

            modelBuilder.Entity<ModuleContentModel>().ToTable("ELearningModuleContents");
            modelBuilder.Entity<ModuleContentModel>().HasKey(p => p.ContentId);

            modelBuilder.Entity<ProgressModel>().ToTable("ELearningProgress");
            modelBuilder.Entity<ProgressModel>().HasKey(p => p.ProgressId);

            modelBuilder.Entity<QuizSubmissionModel>().ToTable("ELearningQuizSubmissions");
            modelBuilder.Entity<QuizSubmissionModel>().HasKey(p => p.SubmissionId);

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<UserModel> User { get; set; }
        public virtual DbSet<RoleModel> Role { get; set; }
        public virtual DbSet<EmployeeModel> Employee { get; set; }
        public virtual DbSet<LoginActivityModel> LoginActivity { get; set; }
        public DbSet<EmploymentInformationModel> EmploymentInformation { get; set; } = null!;

        public virtual DbSet<ModuleModel> ELearningModules { get; set; }
        public virtual DbSet<ModuleContentModel> ELearningModuleContents { get; set; }
        public virtual DbSet<ProgressModel> ELearningProgress { get; set; }
        public virtual DbSet<QuizSubmissionModel> ELearningQuizSubmissions { get; set; }
        public DbSet<InternProfileModel> InternProfiles { get; set; }

        public DbSet<GroupModel> ELearningGroups { get; set; } = null!;
        public DbSet<GroupMemberModel> ELearningGroupMembers { get; set; } = null!;
        public DbSet<ProgramModel> ELearningPrograms { get; set; } = null!;
        public DbSet<BatchModel> ELearningBatches { get; set; } = null!;
        public DbSet<ProgressModel> ELearningModuleProgress { get; set; } = null!;

        public DbSet<QuizModel> ELearningQuizzes { get; set; } = null!;

        public DbSet<QuizQuestionModel> ELearningQuizQuestions { get; set; } = null!;

        public DbSet<QuizQuestionOptionModel> ELearningQuizQuestionOptions { get; set; } = null!;

        public DbSet<StudentAnswerModel> ELearningStudentAnswers { get; set; } = null!;


    }
}
