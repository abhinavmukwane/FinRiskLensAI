using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
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
        //Task<ResultModel<RegisterModel>> AddUpdateConsentRegister(RegisterModel entity);
        //Task<ResultModel<RegisterModel>> AddUpdateOtp(RegisterModel entity);
    }
}
