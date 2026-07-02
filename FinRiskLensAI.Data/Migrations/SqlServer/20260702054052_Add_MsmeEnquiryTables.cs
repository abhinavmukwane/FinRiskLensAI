using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Add_MsmeEnquiryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_MsmeEnquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_MsmeEnquiry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "t_MsmeLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MsmeEnquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_MsmeLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_t_MsmeLocations_t_MsmeEnquiry_MsmeEnquiryId",
                        column: x => x.MsmeEnquiryId,
                        principalTable: "t_MsmeEnquiry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_MsmeNicCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MsmeEnquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nic2Digit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Nic4Digit = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Nic5Digit = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AddedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_MsmeNicCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_t_MsmeNicCodes_t_MsmeEnquiry_MsmeEnquiryId",
                        column: x => x.MsmeEnquiryId,
                        principalTable: "t_MsmeEnquiry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_MsmeEnquiry_Uan",
                table: "t_MsmeEnquiry",
                column: "Uan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_MsmeLocations_MsmeEnquiryId",
                table: "t_MsmeLocations",
                column: "MsmeEnquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_t_MsmeNicCodes_MsmeEnquiryId",
                table: "t_MsmeNicCodes",
                column: "MsmeEnquiryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_MsmeLocations");

            migrationBuilder.DropTable(
                name: "t_MsmeNicCodes");

            migrationBuilder.DropTable(
                name: "t_MsmeEnquiry");
        }
    }
}
