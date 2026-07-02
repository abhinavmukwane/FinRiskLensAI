using FinRiskLensAI.Core.Models.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinRiskLensAI.Data.Configurations
{
    public class AdmLoginConfiguration : IEntityTypeConfiguration<AdmLogin>
    {
        public void Configure(EntityTypeBuilder<AdmLogin> builder)
        {
            builder.ToTable("ADM_Login");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Username).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
            builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Role).IsRequired().HasMaxLength(50);
            builder.Property(x => x.CreatedBy).HasMaxLength(100);
            builder.Property(x => x.UpdatedBy).HasMaxLength(100);

            builder.HasIndex(x => x.Username).IsUnique();
            builder.HasIndex(x => x.Email).IsUnique();

            // Seed a default admin (password: Admin@123, ASP.NET Identity V3 PBKDF2 hash).
            // Fixed values keep the generated migration deterministic.
            builder.HasData(new
            {
                Id = new Guid("a1b2c3d4-0000-4000-8000-000000000001"),
                Username = "admin",
                Email = "admin@finrisklens.ai",
                PasswordHash = "AQAAAAIAAYagAAAAECqQCLDS1XMpIauPSCY8GXjyewL5WfquYrxarGU82tSJ8a+XzMzhzkdWS+Dfu1hVcA==",
                Role = "SuperAdmin",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed"
            });
        }
    }
}
