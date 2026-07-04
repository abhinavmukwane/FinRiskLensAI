using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.IRepositories.OnBoarding;
using FinRiskLensAI.Core.Interfaces.IServices.OnBoarding;
using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
using FinRiskLensAI.Core.Models.User_Activity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Services.Implementation.OnBoarding
{
    public class OnboardingService: IOnboardingService
    {
        private readonly IOnboardingRepository _repository;

        public OnboardingService(IOnboardingRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResultModel<StaticResponseModel>> FetchUdyam(string uan)
        {
            return await _repository.FetchUdyam(uan);
        }
        public async Task SaveMsmeData(string json)
        {
            await _repository.SaveMsmeData(json);
        }

        public async Task<ResultModel<UdyamDetailsModel>> GetUdyamDetails(string uan)
        {
            var result = new ResultModel<UdyamDetailsModel>();

            var data = await _repository.GetUdyamDetails(uan);

            if (data != null)
            {
                result.Result = tflResultType.tflSuccess;
                result.Data = data;
            }
            else
            {
                result.Result = tflResultType.tflError;
                result.Message = "No data found.";
            }

            return result;
        }
        public async Task<ResultModel<UserRegistrationModel>> AddUpdateUserRegst(UserRegistrationModel entity)
        {
            var result = _repository.AddUpdateUserRegst(entity);

            return await result;
        }
        public async Task<ResultModel<UserOtpModel>> AddUpdateUserOtp(UserOtpModel entity)
        {
            var result = _repository.AddUpdateUserOtp(entity);

            return await result;
        }
        public async Task<ResultModel<UserOtpModel>> FetchUserOTPDet(UserOtpModel model)
        {
            return await _repository.FetchUserOTPDet(model);
        }
    }
}
