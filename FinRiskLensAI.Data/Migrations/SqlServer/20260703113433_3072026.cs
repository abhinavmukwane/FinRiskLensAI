using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class _3072026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_UserRegistration",
                columns: table => new
                {
                    UserRegistrationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MobileNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UdyamNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GstinNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PanNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_UserRegistration", x => x.UserRegistrationID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_UserRegistration_GstinNumber",
                table: "t_UserRegistration",
                column: "GstinNumber");

            migrationBuilder.CreateIndex(
                name: "IX_t_UserRegistration_MobileNumber",
                table: "t_UserRegistration",
                column: "MobileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_t_UserRegistration_PanNumber",
                table: "t_UserRegistration",
                column: "PanNumber");

            migrationBuilder.CreateIndex(
                name: "IX_t_UserRegistration_UdyamNumber",
                table: "t_UserRegistration",
                column: "UdyamNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_UserRegistration");
        }
    }
}
