using FinRiskLensAI.Core.Models.AccountAggregator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinRiskLensAI.Data.Configurations
{
    public class AAConsentReqConfiguration : IEntityTypeConfiguration<AAConsentReqModel>
    {
        public void Configure(EntityTypeBuilder<AAConsentReqModel> builder)
        {
            builder.ToTable("t_AAConsentRequest");

            builder.HasKey(x => x.ConsentReqId);
            builder.Property(x => x.ConsentReqId).ValueGeneratedOnAdd();

            builder.Property(x => x.uan).HasMaxLength(50);
            builder.Property(x => x.transactionId).HasMaxLength(100);
            builder.Property(x => x.rid).HasMaxLength(100);
            builder.Property(x => x.ts).HasMaxLength(50);
            builder.Property(x => x.channelId).HasMaxLength(100);
            builder.Property(x => x.encryptedRequest);   // long ciphertext — nvarchar(max)
            builder.Property(x => x.requestDate).HasMaxLength(50);
            builder.Property(x => x.encryptedFiuId).HasMaxLength(200);
            builder.Property(x => x.consentHandle).HasMaxLength(100);
            builder.Property(x => x.url);                 // long redirect URL — nvarchar(max)

            builder.HasIndex(x => x.uan);
            builder.HasIndex(x => x.transactionId);
            builder.HasIndex(x => x.consentHandle);
        }
    }
}
