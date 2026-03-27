using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingEmployeeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "emp_st_address",
                table: "Employees",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "emp_province",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "emp_postal_code",
                table: "Employees",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "emp_city",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "emp_st_address", table: "Employees");
            migrationBuilder.DropColumn(name: "emp_province", table: "Employees");
            migrationBuilder.DropColumn(name: "emp_postal_code", table: "Employees");
            migrationBuilder.DropColumn(name: "emp_city", table: "Employees");
        }
    }
}
