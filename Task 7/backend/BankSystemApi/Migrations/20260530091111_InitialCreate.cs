using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSystemApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountNumber = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    HolderName = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    Email = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false
                    ),
                    Type = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    Balance = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false
                    ),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_AccountNumber",
                table: "BankAccounts",
                column: "AccountNumber",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Email",
                table: "BankAccounts",
                column: "Email",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BankAccounts");
        }
    }
}
