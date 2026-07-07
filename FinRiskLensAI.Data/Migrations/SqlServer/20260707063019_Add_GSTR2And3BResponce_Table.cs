using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinRiskLensAI.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Add_GSTR2And3BResponce_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_GSTR2And3BResponce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UdyamNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    GSTINNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    FilingPeriod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    _2BRequestData = table.Column<string>(name: "2BRequestData", type: "nvarchar(max)", nullable: false),
                    _3BRequestData = table.Column<string>(name: "3BRequestData", type: "nvarchar(max)", nullable: false),
                    _2BResponseData = table.Column<string>(name: "2BResponseData", type: "nvarchar(max)", nullable: false),
                    _3BResponseData = table.Column<string>(name: "3BResponseData", type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_GSTR2And3BResponce", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_GSTR2And3BResponce_UdyamNumber",
                table: "t_GSTR2And3BResponce",
                column: "UdyamNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_GSTR2And3BResponce");
        }
    }
}
