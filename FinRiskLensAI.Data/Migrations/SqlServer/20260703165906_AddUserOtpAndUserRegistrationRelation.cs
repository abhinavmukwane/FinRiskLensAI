using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddUserOtpAndUserRegistrationRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MsmeEnquiryID",
                table: "t_UserRegistration",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "t_UserOtp",
                columns: table => new
                {
                    UserOtpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserRegistrationID = table.Column<int>(type: "int", nullable: false),
                    MsmeEnquiryID = table.Column<int>(type: "int", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OTP = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_UserOtp", x => x.UserOtpID);
                    table.ForeignKey(
                        name: "FK_t_UserOtp_t_MsmeEnquiry_MsmeEnquiryID",
                        column: x => x.MsmeEnquiryID,
                        principalTable: "t_MsmeEnquiry",
                        principalColumn: "MsmeEnquiryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_UserOtp_t_UserRegistration_UserRegistrationID",
                        column: x => x.UserRegistrationID,
                        principalTable: "t_UserRegistration",
                        principalColumn: "UserRegistrationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_UserRegistration_MsmeEnquiryID",
                table: "t_UserRegistration",
                column: "MsmeEnquiryID");

            migrationBuilder.CreateIndex(
                name: "IX_t_UserOtp_MsmeEnquiryID",
                table: "t_UserOtp",
                column: "MsmeEnquiryID");

            migrationBuilder.CreateIndex(
                name: "IX_t_UserOtp_UserRegistrationID",
                table: "t_UserOtp",
                column: "UserRegistrationID");

            migrationBuilder.AddForeignKey(
                name: "FK_t_UserRegistration_t_MsmeEnquiry_MsmeEnquiryID",
                table: "t_UserRegistration",
                column: "MsmeEnquiryID",
                principalTable: "t_MsmeEnquiry",
                principalColumn: "MsmeEnquiryID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_UserRegistration_t_MsmeEnquiry_MsmeEnquiryID",
                table: "t_UserRegistration");

            migrationBuilder.DropTable(
                name: "t_UserOtp");

            migrationBuilder.DropIndex(
                name: "IX_t_UserRegistration_MsmeEnquiryID",
                table: "t_UserRegistration");

            migrationBuilder.DropColumn(
                name: "MsmeEnquiryID",
                table: "t_UserRegistration");
        }
    }
}
