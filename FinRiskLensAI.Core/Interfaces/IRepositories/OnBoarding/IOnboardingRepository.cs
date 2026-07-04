using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
using FinRiskLensAI.Core.Models.User_Activity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.IRepositories.OnBoarding
{
    public interface IOnboardingRepository
    {
        Task<ResultModel<StaticResponseModel>> FetchUdyam(string uan);
        Task SaveMsmeData(string json);
        Task<UdyamDetailsModel> GetUdyamDetails(string uan);
        Task<ResultModel<UserRegistrationModel>> AddUpdateUserRegst(UserRegistrationModel entity);
        Task<ResultModel<UserOtpModel>> AddUpdateUserOtp(UserOtpModel entity);
        Task<ResultModel<UserOtpModel>> FetchUserOTPDet(UserOtpModel model);
    }
}
