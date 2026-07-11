using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
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
            var configuration = BuildConfiguration();

            var dbType = args.Length > 0 ? args[0] : configuration["Database:DbType"] ?? "SqlServer";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (dbType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                var pgConn = configuration["Database:ConnectionStrings:PostgreSQL"]
                    ?? throw new InvalidOperationException(
                        "Database:ConnectionStrings:PostgreSQL not found in appsettings.Development.json");

                optionsBuilder.UseNpgsql(pgConn,
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "cre"));
            }
            else
            {
                var sqlConn = configuration["Database:ConnectionStrings:SqlServer"]
                    ?? throw new InvalidOperationException(
                        "Database:ConnectionStrings:SqlServer not found in appsettings.Development.json");

                optionsBuilder.UseSqlServer(sqlConn,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "cre"));
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var basePath = ResolveStartupProjectPath();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
        }

        private static string ResolveStartupProjectPath()
        {
            var candidate = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(candidate, "appsettings.Development.json")))
                return candidate;

            var dir = new DirectoryInfo(candidate);
            while (dir != null && dir.GetFiles("*.sln").Length == 0)
                dir = dir.Parent;

            if (dir != null)
            {
                var startupProjectPath = Path.Combine(dir.FullName, "FinRiskLensAI");
                if (File.Exists(Path.Combine(startupProjectPath, "appsettings.Development.json")))
                    return startupProjectPath;
            }

            return candidate;
        }
    }
}
