using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSystemApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountOwnershipAndTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                table: "BankAccounts",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "AccountTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    Amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false
                    ),
                    BalanceAfter = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false
                    ),
                    Description = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: false
                    ),
                    PerformedByUserId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: true
                    ),
                    PerformedByRole = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountTransactions_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.Sql(
                @"
                UPDATE ba
                SET ba.AppUserId = au.Id
                FROM BankAccounts ba
                INNER JOIN AppUsers au
                    ON LOWER(LTRIM(RTRIM(ba.Email))) = LOWER(LTRIM(RTRIM(au.Email)))
            "
            );

            migrationBuilder.Sql(
                @"
                INSERT INTO AppUsers
                (
                    Id,
                    EntraObjectId,
                    TenantId,
                    Email,
                    DisplayName,
                    Role,
                    IsActive,
                    CreatedAtUtc,
                    UpdatedAtUtc,
                    LastLoginAtUtc
                )
                SELECT
                    NEWID(),
                    CONCAT('legacy-', CONVERT(varchar(36), ba.Id)),
                    'legacy-tenant',
                    LOWER(LTRIM(RTRIM(ba.Email))),
                    ba.HolderName,
                    'Customer',
                    1,
                    GETUTCDATE(),
                    GETUTCDATE(),
                    GETUTCDATE()
                FROM BankAccounts ba
                WHERE ba.AppUserId IS NULL
            "
            );

            migrationBuilder.Sql(
                @"
                UPDATE ba
                SET ba.AppUserId = au.Id
                FROM BankAccounts ba
                INNER JOIN AppUsers au
                    ON LOWER(LTRIM(RTRIM(ba.Email))) = LOWER(LTRIM(RTRIM(au.Email)))
                WHERE ba.AppUserId IS NULL
            "
            );

            migrationBuilder.Sql(
                @"
                IF EXISTS (SELECT 1 FROM BankAccounts WHERE AppUserId IS NULL)
                    THROW 50000, 'Some BankAccounts rows could not be mapped to AppUsers.', 1;
            "
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AppUserId",
                table: "BankAccounts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_AppUserId",
                table: "BankAccounts",
                column: "AppUserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransactions_BankAccountId_CreatedAtUtc",
                table: "AccountTransactions",
                columns: new[] { "BankAccountId", "CreatedAtUtc" }
            );

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_AppUsers_AppUserId",
                table: "BankAccounts",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_AppUsers_AppUserId",
                table: "BankAccounts"
            );

            migrationBuilder.DropTable(name: "AccountTransactions");

            migrationBuilder.DropIndex(name: "IX_BankAccounts_AppUserId", table: "BankAccounts");

            migrationBuilder.DropColumn(name: "AppUserId", table: "BankAccounts");
        }
    }
}
