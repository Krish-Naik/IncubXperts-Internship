using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOS.Migrations
{
    /// <inheritdoc />
    public partial class AddInfoRequestAndCoApplicantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtraDetailsJson",
                table: "LoanApplications",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "InfoRequestDetails",
                table: "LoanApplications",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "InfoResponseText",
                table: "LoanApplications",
                type: "text",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ExtraDetailsJson", table: "LoanApplications");

            migrationBuilder.DropColumn(name: "InfoRequestDetails", table: "LoanApplications");

            migrationBuilder.DropColumn(name: "InfoResponseText", table: "LoanApplications");
        }
    }
}
