using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSystemApi.Migrations
{
    /// <inheritdoc />
    public partial class DropUniqueEmailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_Email",
                table: "BankAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Email",
                table: "BankAccounts",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_Email",
                table: "BankAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Email",
                table: "BankAccounts",
                column: "Email",
                unique: true);
        }
    }
}
