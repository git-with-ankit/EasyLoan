using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyLoan.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovedThreeFieldsFromPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0730a01e-5fa3-41c9-86ab-848f297e86cd"));

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "LoanPayments");

            migrationBuilder.DropColumn(
                name: "InterestPaid",
                table: "LoanPayments");

            migrationBuilder.DropColumn(
                name: "PrincipalPaid",
                table: "LoanPayments");

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Email", "Name", "Password", "PhoneNumber", "Role" },
                values: new object[] { new Guid("7dada553-0025-47cb-94f9-8658065c6d21"), "ankitkumarsingh018@gmail.com", "Ankit", "$2a$11$PEzbmgwDWsZu3Zabt9GKP.4ZV1Ci6zk5k7zFqXs2kvHQMCrZ9Macq", "1234567890", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("7dada553-0025-47cb-94f9-8658065c6d21"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "LoanPayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "InterestPaid",
                table: "LoanPayments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrincipalPaid",
                table: "LoanPayments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Email", "Name", "Password", "PhoneNumber", "Role" },
                values: new object[] { new Guid("0730a01e-5fa3-41c9-86ab-848f297e86cd"), "ankitkumarsingh018@gmail.com", "Ankit", "$2a$11$XJJcBdKAVXeHAy.weZQJN.1k6onGuOal8s03LGm8vSmPU4IuAnO1C", "1234567890", "Admin" });
        }
    }
}
