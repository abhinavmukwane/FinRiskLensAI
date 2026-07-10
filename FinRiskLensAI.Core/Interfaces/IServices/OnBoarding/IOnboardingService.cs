using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
using FinRiskLensAI.Core.Models.User_Activity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.IServices.OnBoarding
{
    public interface IOnboardingService
    {
        Task<ResultModel<StaticResponseModel>> FetchUdyam(string uan);
        Task<SaveMsmeResultModel> SaveMsmeData(string json);
        Task<ResultModel<UdyamDetailsModel>> GetUdyamDetails(string uan);
        Task<ResultModel<string>> GetUdyamPayload(string uan);
        Task<ResultModel<UserRegistrationModel>> AddUpdateUserRegst(int? msmeEnquiryId, string email, string clientIp);
        //Task<ResultModel<UserRegistrationModel>> AddUpdateUserRegst(UserRegistrationModel entity);
        Task<ResultModel<UserOtpModel>> AddUpdateUserOtp(UserOtpModel entity);
        Task<ResultModel<UserSessionModel>> FetchUserOTPDet(UserOtpModel model);
        Task<bool> IsUdyamRegistered(string uan);
        Task<ResultModel<UserOtpModel>> ValidateCustomerEmail(string email);
        //Task AddUpdateUserRegst(int? msmeEnquiryID, string? email, string clientIp);
    }
}
