using FinRiskLensAI.Core.Models.Universal;
using FinRiskLensAI.Core.Models.User_Activity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Data.Configurations
{
    public class UserOtpConfiguration : IEntityTypeConfiguration<UserOtpModel>
    {
        public void Configure(EntityTypeBuilder<UserOtpModel> builder)
        {
            builder.ToTable("t_UserOtp");

            builder.HasKey(x => x.UserOtpID);

            builder.Property(x => x.UserOtpID)
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.MobileNumber)
                   .HasMaxLength(40);

            builder.Property(x => x.Email)
                   .HasMaxLength(100);

            builder.Property(x => x.OTP);

            builder.HasOne(x => x.UserRegistration)
                   .WithMany()
                   .HasForeignKey(x => x.UserRegistrationID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MsmeEnquiry)
                   .WithMany()
                   .HasForeignKey(x => x.MsmeEnquiryID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
