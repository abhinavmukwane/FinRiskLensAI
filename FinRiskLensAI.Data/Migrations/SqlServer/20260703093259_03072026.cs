using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class _03072026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "m_StaticResponces",
                columns: table => new
                {
                    StaticResponcesID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UdyamNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UdyamNumberResponce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PanNumberResponce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GSTNumberResponce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GST2BResponce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GST3BResponce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ITRNumberResponce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IPResponce = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_m_StaticResponces", x => x.StaticResponcesID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_m_StaticResponces_UdyamNumber",
                table: "m_StaticResponces",
                column: "UdyamNumber",
                unique: true,
                filter: "[UdyamNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "m_StaticResponces");
        }
    }
}
