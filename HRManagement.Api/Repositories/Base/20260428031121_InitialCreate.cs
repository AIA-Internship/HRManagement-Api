using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Api.Repositories.Base
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmergencyContacts",
                columns: table => new
                {
                    emergency_contact_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    emergency_contact_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    emergency_contact_phone = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    emergency_contact_relationship = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContacts", x => x.emergency_contact_id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    emp_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    emp_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    emp_gender = table.Column<int>(type: "int", nullable: false),
                    emp_personal_email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    emp_work_email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    emp_nik = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    emp_POB = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    emp_DOB = table.Column<DateTime>(type: "date", nullable: false),
                    emp_marital_status = table.Column<int>(type: "int", nullable: false),
                    emp_current_st_address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    emp_current_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    emp_current_province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    emp_current_postal_code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    emp_residential_st_address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    emp_residential_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    emp_residential_province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    emp_residential_postal_code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    emp_phone = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    emp_role = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.emp_id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeUpdateRequests",
                columns: table => new
                {
                    request_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    emp_id = table.Column<int>(type: "int", nullable: false),
                    new_full_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    new_gender = table.Column<int>(type: "int", nullable: true),
                    new_personal_email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    new_place_of_birth = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    new_date_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    new_marital_status = table.Column<int>(type: "int", nullable: true),
                    new_current_street_address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    new_current_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    new_current_province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    new_current_postal_code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    new_residential_street_address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    new_residential_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    new_residential_province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    new_residential_postal_code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    new_phone_number = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    NewEmergencyContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewEmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewEmergencyContactRelationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    request_status = table.Column<int>(type: "int", nullable: false),
                    hr_reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeUpdateRequests", x => x.request_id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentInformation",
                columns: table => new
                {
                    employment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    emp_id = table.Column<int>(type: "int", nullable: false),
                    employment_status = table.Column<int>(type: "int", nullable: false),
                    employment_start_date = table.Column<DateTime>(type: "date", nullable: false),
                    employment_type = table.Column<int>(type: "int", nullable: false),
                    employment_department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    employment_position = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    employment_supervisor_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    employee_display_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentInformation", x => x.employment_id);
                });

            migrationBuilder.CreateTable(
                name: "SystemLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimesheetDayComments",
                columns: table => new
                {
                    ts_comment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ts_comment_submission_id = table.Column<int>(type: "int", nullable: false),
                    ts_comment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    ts_comment_text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ts_comment_is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    ts_comment_created_by = table.Column<long>(type: "bigint", nullable: false),
                    ts_comment_created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ts_comment_modified_by = table.Column<long>(type: "bigint", nullable: false),
                    ts_comment_modified_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetDayComments", x => x.ts_comment_id);
                });

            migrationBuilder.CreateTable(
                name: "TimesheetEntries",
                columns: table => new
                {
                    ts_entry_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ts_entry_employee_id = table.Column<int>(type: "int", nullable: false),
                    ts_entry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    ts_entry_duration_minutes = table.Column<int>(type: "int", nullable: false),
                    ts_entry_project_id = table.Column<int>(type: "int", nullable: false),
                    ts_entry_app_used = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ts_entry_task_desc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ts_entry_project_lead_id = table.Column<int>(type: "int", nullable: false),
                    ts_entry_location = table.Column<int>(type: "int", nullable: false),
                    ts_entry_is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    ts_entry_created_by = table.Column<long>(type: "bigint", nullable: false),
                    ts_entry_created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ts_entry_modified_by = table.Column<long>(type: "bigint", nullable: false),
                    ts_entry_modified_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetEntries", x => x.ts_entry_id);
                });

            migrationBuilder.CreateTable(
                name: "TimesheetProjects",
                columns: table => new
                {
                    ts_project_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ts_project_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ts_project_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ts_project_leader = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    ts_project_status = table.Column<int>(type: "int", nullable: false),
                    ts_project_is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    ts_project_created_by = table.Column<long>(type: "bigint", nullable: false),
                    ts_project_created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ts_project_modified_by = table.Column<long>(type: "bigint", nullable: false),
                    ts_project_modified_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetProjects", x => x.ts_project_id);
                });

            migrationBuilder.CreateTable(
                name: "TimesheetSubmissions",
                columns: table => new
                {
                    ts_sub_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ts_sub_employee_id = table.Column<int>(type: "int", nullable: false),
                    ts_sub_year = table.Column<int>(type: "int", nullable: false),
                    ts_sub_month = table.Column<int>(type: "int", nullable: false),
                    ts_sub_submitted_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ts_sub_status = table.Column<int>(type: "int", nullable: false),
                    ts_sub_revision_note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ts_sub_reviewed_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ts_sub_reviewed_by = table.Column<int>(type: "int", nullable: true),
                    ts_sub_is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    ts_sub_created_by = table.Column<long>(type: "bigint", nullable: false),
                    ts_sub_created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ts_sub_modified_by = table.Column<long>(type: "bigint", nullable: false),
                    ts_sub_modified_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetSubmissions", x => x.ts_sub_id);
                });

            migrationBuilder.CreateTable(
                name: "TodoTasks",
                columns: table => new
                {
                    todo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    todo_employee_id = table.Column<int>(type: "int", nullable: false),
                    todo_task_name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    todo_due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    todo_priority = table.Column<int>(type: "int", nullable: false),
                    todo_is_completed = table.Column<bool>(type: "bit", nullable: false),
                    todo_is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    todo_created_by = table.Column<long>(type: "bigint", nullable: false),
                    todo_created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    todo_modified_by = table.Column<long>(type: "bigint", nullable: false),
                    todo_modified_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoTasks", x => x.todo_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employee_email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    user_role = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedUtcDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_employee_id",
                table: "EmergencyContacts",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_emp_name",
                table: "Employees",
                column: "emp_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_emp_nik",
                table: "Employees",
                column: "emp_nik",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_emp_personal_email",
                table: "Employees",
                column: "emp_personal_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_emp_phone",
                table: "Employees",
                column: "emp_phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_emp_work_email",
                table: "Employees",
                column: "emp_work_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeUpdateRequests_emp_id",
                table: "EmployeeUpdateRequests",
                column: "emp_id");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentInformation_emp_id",
                table: "EmploymentInformation",
                column: "emp_id");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentInformation_employee_display_id",
                table: "EmploymentInformation",
                column: "employee_display_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetDayComments_ts_comment_submission_id_ts_comment_date",
                table: "TimesheetDayComments",
                columns: new[] { "ts_comment_submission_id", "ts_comment_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetEntries_ts_entry_employee_id_ts_entry_date",
                table: "TimesheetEntries",
                columns: new[] { "ts_entry_employee_id", "ts_entry_date" });

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetEntries_ts_entry_project_id",
                table: "TimesheetEntries",
                column: "ts_entry_project_id");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetEntries_ts_entry_project_lead_id",
                table: "TimesheetEntries",
                column: "ts_entry_project_lead_id");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetProjects_ts_project_name",
                table: "TimesheetProjects",
                column: "ts_project_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetSubmissions_ts_sub_employee_id_ts_sub_year_ts_sub_month",
                table: "TimesheetSubmissions",
                columns: new[] { "ts_sub_employee_id", "ts_sub_year", "ts_sub_month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoTasks_todo_employee_id_todo_is_deleted",
                table: "TodoTasks",
                columns: new[] { "todo_employee_id", "todo_is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_employee_email",
                table: "Users",
                column: "employee_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "EmployeeAttachments");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeeUpdateRequests");

            migrationBuilder.DropTable(
                name: "EmploymentInformation");

            migrationBuilder.DropTable(
                name: "SystemLookups");

            migrationBuilder.DropTable(
                name: "TimesheetDayComments");

            migrationBuilder.DropTable(
                name: "TimesheetEntries");

            migrationBuilder.DropTable(
                name: "TimesheetProjects");

            migrationBuilder.DropTable(
                name: "TimesheetSubmissions");

            migrationBuilder.DropTable(
                name: "TodoTasks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
