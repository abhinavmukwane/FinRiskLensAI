using FinRiskLensAI.Core.Models.AccountAggregator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinRiskLensAI.Data.Configurations
{
    public class AccAggreTokenConfiguration : IEntityTypeConfiguration<AccAggreTokenModel>
    {
        public void Configure(EntityTypeBuilder<AccAggreTokenModel> builder)
        {
            builder.ToTable("t_AccAggreToken");

            builder.HasKey(x => x.TokenID);
            builder.Property(x => x.TokenID).ValueGeneratedOnAdd();

            builder.Property(x => x.rid).HasMaxLength(100);
            builder.Property(x => x.ts).HasMaxLength(50);
            builder.Property(x => x.token);   // JWTs are long — leave as nvarchar(max)
        }
    }
}
