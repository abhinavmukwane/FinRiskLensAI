using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinRiskLensAI.Data.Configurations
{
    public class UserRegistrationConfiguration : IEntityTypeConfiguration<UserRegistrationModel>
    {
        public void Configure(EntityTypeBuilder<UserRegistrationModel> builder)
        {
            builder.ToTable("t_UserRegistration");

            builder.HasKey(x => x.UserRegistrationID);

            builder.Property(x => x.UserRegistrationID)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.MobileNumber)
                   .HasMaxLength(40);

            builder.Property(x => x.Email)
                   .HasMaxLength(100);

            builder.Property(x => x.UdyamNumber)
                   .HasMaxLength(100);

            builder.Property(x => x.GstinNumber)
                   .HasMaxLength(100);

            builder.Property(x => x.PanNumber)
                   .HasMaxLength(100);

            builder.Property(x => x.MsmeEnquiryID);

            builder.HasOne(x => x.MsmeEnquiry)
                   .WithMany()
                   .HasForeignKey(x => x.MsmeEnquiryID)
                   .OnDelete(DeleteBehavior.Restrict);

            // Optional indexes for faster lookups
            builder.HasIndex(x => x.UdyamNumber);

            builder.HasIndex(x => x.GstinNumber);

            builder.HasIndex(x => x.PanNumber);

            builder.HasIndex(x => x.MobileNumber);
        }
    }
}