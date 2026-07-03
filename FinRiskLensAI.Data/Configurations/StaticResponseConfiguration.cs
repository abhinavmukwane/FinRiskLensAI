using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Universal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinRiskLensAI.Data.Configurations
{
    public class StaticResponseConfiguration : IEntityTypeConfiguration<StaticResponseModel>
    {
        public void Configure(EntityTypeBuilder<StaticResponseModel> builder)
        {
            builder.ToTable("m_StaticResponces");

            builder.HasKey(x => x.StaticResponcesID);

            builder.Property(x => x.StaticResponcesID)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.UdyamNumber)
                   .HasMaxLength(100);

            // NVARCHAR(MAX)
            builder.Property(x => x.UdyamNumberResponce);

            builder.Property(x => x.PanNumberResponce);

            builder.Property(x => x.GSTNumberResponce);

            builder.Property(x => x.GST2BResponce);

            builder.Property(x => x.GST3BResponce);

            builder.Property(x => x.ITRNumberResponce);

            builder.Property(x => x.IPResponce);

            // Optional: Faster lookup by Udyam Number
            builder.HasIndex(x => x.UdyamNumber)
                   .IsUnique();
        }
    }
}