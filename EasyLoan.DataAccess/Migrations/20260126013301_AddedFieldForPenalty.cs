using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyLoan.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedFieldForPenalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_LoanApplications_LoanApplicationNumber",
                table: "Loans");

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("7dada553-0025-47cb-94f9-8658065c6d21"));

            migrationBuilder.RenameColumn(
                name: "LoanApplicationNumber",
                table: "Loans",
                newName: "LoanApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_LoanApplicationNumber",
                table: "Loans",
                newName: "IX_Loans_LoanApplicationId");

            migrationBuilder.AddColumn<decimal>(
                name: "InterestComponent",
                table: "LoanEmis",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PenaltyAmount",
                table: "LoanEmis",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrincipalComponent",
                table: "LoanEmis",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Email", "Name", "Password", "PhoneNumber", "Role" },
                values: new object[] { new Guid("cac68a94-2859-42c6-bd2e-86c8eef2fd64"), "ankitkumarsingh018@gmail.com", "Ankit", "$2a$11$uCcQaBXQNoeqhfs13PDZDuOmr7B.8Aj8ctUpMPTnC3tAEAGv0n.Ne", "1234567890", "Admin" });

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_LoanApplications_LoanApplicationId",
                table: "Loans",
                column: "LoanApplicationId",
                principalTable: "LoanApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_LoanApplications_LoanApplicationId",
                table: "Loans");

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("cac68a94-2859-42c6-bd2e-86c8eef2fd64"));

            migrationBuilder.DropColumn(
                name: "InterestComponent",
                table: "LoanEmis");

            migrationBuilder.DropColumn(
                name: "PenaltyAmount",
                table: "LoanEmis");

            migrationBuilder.DropColumn(
                name: "PrincipalComponent",
                table: "LoanEmis");

            migrationBuilder.RenameColumn(
                name: "LoanApplicationId",
                table: "Loans",
                newName: "LoanApplicationNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_LoanApplicationId",
                table: "Loans",
                newName: "IX_Loans_LoanApplicationNumber");

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Email", "Name", "Password", "PhoneNumber", "Role" },
                values: new object[] { new Guid("7dada553-0025-47cb-94f9-8658065c6d21"), "ankitkumarsingh018@gmail.com", "Ankit", "$2a$11$PEzbmgwDWsZu3Zabt9GKP.4ZV1Ci6zk5k7zFqXs2kvHQMCrZ9Macq", "1234567890", "Admin" });

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_LoanApplications_LoanApplicationNumber",
                table: "Loans",
                column: "LoanApplicationNumber",
                principalTable: "LoanApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
