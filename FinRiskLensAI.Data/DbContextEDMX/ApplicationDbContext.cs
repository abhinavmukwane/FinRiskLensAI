using FinRiskLensAI.Core.Models.Admin;
using FinRiskLensAI.Core.Models.Common;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Data.DbContextEDMX
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<AdmLogin> AdmLogins => Set<AdmLogin>();
        public DbSet<MsmeEnquiry> MsmeEnquiries => Set<MsmeEnquiry>();
        public DbSet<MsmeLocation> MsmeLocations => Set<MsmeLocation>();
        public DbSet<MsmeNicCode> MsmeNicCodes => Set<MsmeNicCode>();
        public DbSet<StaticResponseModel> StaticResponseModel => Set<StaticResponseModel>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Default precision for all decimals — keeps both SQL Server and PostgreSQL happy.
            configurationBuilder.Properties<decimal>().HavePrecision(18, 4);
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SetAuditableFields();
            return base.SaveChangesAsync(ct);
        }

        private void SetAuditableFields()
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;
                if (entry.State is EntityState.Added or EntityState.Modified)
                    entry.Entity.UpdatedAt = now;
            }
        }
    }
}
