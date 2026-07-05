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

            builder.Property(x => x.transactionId).HasMaxLength(100);
            builder.Property(x => x.rid).HasMaxLength(100);
            builder.Property(x => x.ts).HasMaxLength(50);
            builder.Property(x => x.channelId).HasMaxLength(100);
            builder.Property(x => x.aaId_custId).HasMaxLength(150);
            builder.Property(x => x.consentHandle).HasMaxLength(100);
            builder.Property(x => x.consentPurpose).HasMaxLength(200);
            builder.Property(x => x.consentDescription).HasMaxLength(300);
            builder.Property(x => x.requestDate).HasMaxLength(50);
            builder.Property(x => x.consentStatus).HasMaxLength(50);
            builder.Property(x => x.requestSessionId).HasMaxLength(100);
            builder.Property(x => x.requestConsentId).HasMaxLength(100);
            builder.Property(x => x.dateTimeRangeFrom).HasMaxLength(50);
            builder.Property(x => x.dateTimeRangeTo).HasMaxLength(50);
            builder.Property(x => x.aaId_bank).HasMaxLength(150);
            builder.Property(x => x.fetchType).HasMaxLength(50);

            builder.HasIndex(x => x.transactionId);
            builder.HasIndex(x => x.consentHandle);
        }
    }
}
