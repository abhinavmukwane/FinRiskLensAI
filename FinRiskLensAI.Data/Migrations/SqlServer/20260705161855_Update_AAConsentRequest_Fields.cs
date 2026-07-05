using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Update_AAConsentRequest_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "aaId_bank",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "aaId_custId",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "consentDescription",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "consentStatus",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "dateTimeRangeFrom",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "dateTimeRangeTo",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "fetchType",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "requestConsentId",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "requestSessionId",
                table: "t_AAConsentRequest");

            migrationBuilder.RenameColumn(
                name: "consentPurpose",
                table: "t_AAConsentRequest",
                newName: "encryptedFiuId");

            migrationBuilder.AddColumn<string>(
                name: "encryptedRequest",
                table: "t_AAConsentRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "t_AAConsentRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "encryptedRequest",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "url",
                table: "t_AAConsentRequest");

            migrationBuilder.RenameColumn(
                name: "encryptedFiuId",
                table: "t_AAConsentRequest",
                newName: "consentPurpose");

            migrationBuilder.AddColumn<string>(
                name: "aaId_bank",
                table: "t_AAConsentRequest",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "aaId_custId",
                table: "t_AAConsentRequest",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "consentDescription",
                table: "t_AAConsentRequest",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "consentStatus",
                table: "t_AAConsentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dateTimeRangeFrom",
                table: "t_AAConsentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dateTimeRangeTo",
                table: "t_AAConsentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "fetchType",
                table: "t_AAConsentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "requestConsentId",
                table: "t_AAConsentRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "requestSessionId",
                table: "t_AAConsentRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
