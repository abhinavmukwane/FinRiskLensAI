using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Data.DbContextEDMX
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var dbType = args.Length > 0 ? args[0] : "SqlServer";
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (dbType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                optionsBuilder
                    .UseNpgsql(
                        "Host=localhost;Port=5432;Database=CreditRuleEngine;Username=postgres;Password=postgres",
                        npgsql =>
                        {
                            npgsql.MigrationsAssembly("FinRiskLensAI.Data.Migrations.PostgreSQL");
                            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "cre");
                        });
            }
            else
            {
                optionsBuilder.UseSqlServer(
                    "Server=(localdb)\\MSSQLLocalDB;Database=CreditRuleEngine;Trusted_Connection=True;TrustServerCertificate=true",
                    sql =>
                    {
                        sql.MigrationsAssembly("FinRiskLensAI.Data.Migrations.SqlServer");
                        sql.MigrationsHistoryTable("__EFMigrationsHistory", "cre");
                    });
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
