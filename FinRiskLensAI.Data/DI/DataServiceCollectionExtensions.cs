using FinRiskLensAI.Data.DbContextEDMX;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinRiskLensAI.Data.DI
{
    public static class DataServiceCollectionExtensions
    {
        public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var dbType = configuration["Database:DbType"] ?? "SqlServer";

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (dbType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                {
                    var connectionString = configuration["Database:ConnectionStrings:PostgreSQL"]
                        ?? throw new InvalidOperationException("Missing Database:ConnectionStrings:PostgreSQL in configuration.");

                    options.UseNpgsql(connectionString,
                        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "cre"));
                }
                else
                {
                    var connectionString = configuration["Database:ConnectionStrings:SqlServer"]
                        ?? throw new InvalidOperationException("Missing Database:ConnectionStrings:SqlServer in configuration.");

                    options.UseSqlServer(connectionString,
                        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "cre"));
                }
            });

            return services;
        }
    }
}
