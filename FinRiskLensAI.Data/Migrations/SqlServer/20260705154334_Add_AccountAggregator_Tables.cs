using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Add_AccountAggregator_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_AAConsentRequest",
                columns: table => new
                {
                    ConsentReqId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    transactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    rid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ts = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    channelId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    aaId_custId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    consentHandle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    consentPurpose = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    consentDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    requestDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    consentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    requestSessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    requestConsentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dateTimeRangeFrom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    dateTimeRangeTo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    aaId_bank = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    fetchType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_AAConsentRequest", x => x.ConsentReqId);
                });

            migrationBuilder.CreateTable(
                name: "t_AccAggreToken",
                columns: table => new
                {
                    TokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ts = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_AccAggreToken", x => x.TokenID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_AAConsentRequest_consentHandle",
                table: "t_AAConsentRequest",
                column: "consentHandle");

            migrationBuilder.CreateIndex(
                name: "IX_t_AAConsentRequest_transactionId",
                table: "t_AAConsentRequest",
                column: "transactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_AAConsentRequest");

            migrationBuilder.DropTable(
                name: "t_AccAggreToken");
        }
    }
}
