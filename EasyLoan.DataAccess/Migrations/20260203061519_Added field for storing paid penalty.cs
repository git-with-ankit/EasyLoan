using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyLoan.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Addedfieldforstoringpaidpenalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<decimal>(
                name: "PaidPenaltyAmount",
                table: "LoanEmis",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            //migrationBuilder.InsertData(
            //    table: "Employees",
            //    columns: new[] { "Id", "Email", "Name", "Password", "PhoneNumber", "Role" },
            //    values: new object[] { new Guid("a1adbd15-4709-4017-97af-77cc56bdd439"), "ankitkumarsingh018@gmail.com", "Ankit", "$2a$11$JqtCImh.HFTH8RKcjjVEW.owsp.qgwhJsK.M0b5FozVxzZxSgdBSi", "1234567890", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "PaidPenaltyAmount",
                table: "LoanEmis");

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Email", "Name", "Password", "PhoneNumber", "Role" },
                values: new object[] { new Guid("cac68a94-2859-42c6-bd2e-86c8eef2fd64"), "ankitkumarsingh018@gmail.com", "Ankit", "$2a$11$uCcQaBXQNoeqhfs13PDZDuOmr7B.8Aj8ctUpMPTnC3tAEAGv0n.Ne", "1234567890", "Admin" });
        }
    }
}
