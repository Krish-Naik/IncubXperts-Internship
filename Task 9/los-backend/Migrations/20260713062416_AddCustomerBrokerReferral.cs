using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOS.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerBrokerReferral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReferredByBrokerId",
                table: "InternalUsers",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_InternalUsers_ReferredByBrokerId",
                table: "InternalUsers",
                column: "ReferredByBrokerId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InternalUsers_InternalUsers_ReferredByBrokerId",
                table: "InternalUsers",
                column: "ReferredByBrokerId",
                principalTable: "InternalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternalUsers_InternalUsers_ReferredByBrokerId",
                table: "InternalUsers"
            );

            migrationBuilder.DropIndex(
                name: "IX_InternalUsers_ReferredByBrokerId",
                table: "InternalUsers"
            );

            migrationBuilder.DropColumn(name: "ReferredByBrokerId", table: "InternalUsers");
        }
    }
}
