using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitialCreate_AdmLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADM_Login",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_Login", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ADM_Login",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "IsActive", "IsDeleted", "LastLoginAt", "PasswordHash", "Role", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { new Guid("a1b2c3d4-0000-4000-8000-000000000001"), new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "admin@finrisklens.ai", true, false, null, "AQAAAAIAAYagAAAAECqQCLDS1XMpIauPSCY8GXjyewL5WfquYrxarGU82tSJ8a+XzMzhzkdWS+Dfu1hVcA==", "SuperAdmin", new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADM_Login");
        }
    }
}
