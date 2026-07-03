using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADM_Login",
                columns: table => new
                {
                    AdmLoginID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_Login", x => x.AdmLoginID);
                });

            migrationBuilder.CreateTable(
                name: "t_MsmeEnquiry",
                columns: table => new
                {
                    MsmeEnquiryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Uan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CertificateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NameOfEnterprise = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MajorActivity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SocialCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfCommencement = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Flat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NameOfBuilding = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Road = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Village = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Block = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Pin = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OrganizationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfIncorporation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MsmeDfo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_MsmeEnquiry", x => x.MsmeEnquiryID);
                });

            migrationBuilder.CreateTable(
                name: "t_MsmeLocations",
                columns: table => new
                {
                    MsmeLocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MsmeEnquiryID = table.Column<int>(type: "int", nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Line1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Building = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Village = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Road = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Pin = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_MsmeLocations", x => x.MsmeLocationID);
                    table.ForeignKey(
                        name: "FK_t_MsmeLocations_t_MsmeEnquiry_MsmeEnquiryID",
                        column: x => x.MsmeEnquiryID,
                        principalTable: "t_MsmeEnquiry",
                        principalColumn: "MsmeEnquiryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_MsmeNicCodes",
                columns: table => new
                {
                    MsmeNicCodeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MsmeEnquiryID = table.Column<int>(type: "int", nullable: false),
                    Nic2Digit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Nic4Digit = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Nic5Digit = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AddedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_MsmeNicCodes", x => x.MsmeNicCodeID);
                    table.ForeignKey(
                        name: "FK_t_MsmeNicCodes_t_MsmeEnquiry_MsmeEnquiryID",
                        column: x => x.MsmeEnquiryID,
                        principalTable: "t_MsmeEnquiry",
                        principalColumn: "MsmeEnquiryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ADM_Login",
                columns: new[] { "AdmLoginID", "CreatedAt", "CreatedBy", "Email", "IsActive", "LastLoginAt", "PasswordHash", "Role", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { 1, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "admin@finrisklens.ai", true, null, "AQAAAAIAAYagAAAAECqQCLDS1XMpIauPSCY8GXjyewL5WfquYrxarGU82tSJ8a+XzMzhzkdWS+Dfu1hVcA==", "SuperAdmin", new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_ADM_Login_Email",
                table: "ADM_Login",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ADM_Login_Username",
                table: "ADM_Login",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_MsmeEnquiry_Uan",
                table: "t_MsmeEnquiry",
                column: "Uan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_MsmeLocations_MsmeEnquiryID",
                table: "t_MsmeLocations",
                column: "MsmeEnquiryID");

            migrationBuilder.CreateIndex(
                name: "IX_t_MsmeNicCodes_MsmeEnquiryID",
                table: "t_MsmeNicCodes",
                column: "MsmeEnquiryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADM_Login");

            migrationBuilder.DropTable(
                name: "t_MsmeLocations");

            migrationBuilder.DropTable(
                name: "t_MsmeNicCodes");

            migrationBuilder.DropTable(
                name: "t_MsmeEnquiry");
        }
    }
}
