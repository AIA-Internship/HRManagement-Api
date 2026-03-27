using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTimesheetForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetEntries_Employees_ts_entry_employee_id",
                table: "TimesheetEntries",
                column: "ts_entry_employee_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetEntries_Employees_ts_entry_project_lead_id",
                table: "TimesheetEntries",
                column: "ts_entry_project_lead_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetEntries_TimesheetProjects_ts_entry_project_id",
                table: "TimesheetEntries",
                column: "ts_entry_project_id",
                principalTable: "TimesheetProjects",
                principalColumn: "ts_project_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetSubmissions_Employees_ts_sub_employee_id",
                table: "TimesheetSubmissions",
                column: "ts_sub_employee_id",
                principalTable: "Employees",
                principalColumn: "emp_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
