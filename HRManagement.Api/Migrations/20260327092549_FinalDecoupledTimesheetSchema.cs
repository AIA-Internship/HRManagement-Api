using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class FinalDecoupledTimesheetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyContacts_Employees_employee_id",
                table: "EmergencyContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeUpdateRequests_Employees_emp_id",
                table: "EmployeeUpdateRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_EmploymentInformation_Employees_emp_id",
                table: "EmploymentInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_TimesheetDayComments_TimesheetSubmissions_ts_comment_submission_id",
                table: "TimesheetDayComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TimesheetEntries_Employees_ts_entry_employee_id",
                table: "TimesheetEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TimesheetEntries_Employees_ts_entry_project_lead_id",
                table: "TimesheetEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TimesheetEntries_TimesheetProjects_ts_entry_project_id",
                table: "TimesheetEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TimesheetSubmissions_Employees_ts_sub_employee_id",
                table: "TimesheetSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoTasks_Employees_todo_employee_id",
                table: "TodoTasks");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentInformation_emp_id",
                table: "EmploymentInformation");

            migrationBuilder.AlterColumn<int>(
                name: "request_status",
                table: "EmployeeUpdateRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentInformation_emp_id",
                table: "EmploymentInformation",
                column: "emp_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmploymentInformation_emp_id",
                table: "EmploymentInformation");

            migrationBuilder.AlterColumn<string>(
                name: "request_status",
                table: "EmployeeUpdateRequests",
                type: "nvarchar(64)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentInformation_emp_id",
                table: "EmploymentInformation",
                column: "emp_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyContacts_Employees_employee_id",
                table: "EmergencyContacts",
                column: "employee_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeUpdateRequests_Employees_emp_id",
                table: "EmployeeUpdateRequests",
                column: "emp_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmploymentInformation_Employees_emp_id",
                table: "EmploymentInformation",
                column: "emp_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetDayComments_TimesheetSubmissions_ts_comment_submission_id",
                table: "TimesheetDayComments",
                column: "ts_comment_submission_id",
                principalTable: "TimesheetSubmissions",
                principalColumn: "ts_sub_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetEntries_Employees_ts_entry_employee_id",
                table: "TimesheetEntries",
                column: "ts_entry_employee_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetEntries_Employees_ts_entry_project_lead_id",
                table: "TimesheetEntries",
                column: "ts_entry_project_lead_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetEntries_TimesheetProjects_ts_entry_project_id",
                table: "TimesheetEntries",
                column: "ts_entry_project_id",
                principalTable: "TimesheetProjects",
                principalColumn: "ts_project_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetSubmissions_Employees_ts_sub_employee_id",
                table: "TimesheetSubmissions",
                column: "ts_sub_employee_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoTasks_Employees_todo_employee_id",
                table: "TodoTasks",
                column: "todo_employee_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
