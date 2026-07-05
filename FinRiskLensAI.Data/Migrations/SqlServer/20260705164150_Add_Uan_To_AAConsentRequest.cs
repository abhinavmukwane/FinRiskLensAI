using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Add_Uan_To_AAConsentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "uan",
                table: "t_AAConsentRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_t_AAConsentRequest_uan",
                table: "t_AAConsentRequest",
                column: "uan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_t_AAConsentRequest_uan",
                table: "t_AAConsentRequest");

            migrationBuilder.DropColumn(
                name: "uan",
                table: "t_AAConsentRequest");
        }
    }
}
