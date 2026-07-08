using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.IRepositories.Common;
using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Models;
using FinRiskLensAI.Core.Models.Onboarding;
using FinRiskLensAI.Core.Models.Universal;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace FinRiskLensAI.Services.Implementation.Common
{
    public class StaticResponseService : IStaticResponseService
    {
        private readonly IStaticResponseRepository _repository;

        public StaticResponseService(IStaticResponseRepository repository)
        {
            _repository = repository;
        }

        public Task<ResultModel<StaticResponseModel>> FetchResponses(string udyamNumber)
            => _repository.FetchByUdyamNumber(udyamNumber);

        public async Task<string?> GetStaticCommonResponce(string udyamNumber, StaticResponseType type)
        {
            var result = await _repository.FetchByUdyamNumber(udyamNumber);

            if (result.Result != tflResultType.tflSuccess || result.Data == null)
                return null;

            return type switch
            {
                StaticResponseType.Udyam => result.Data.UdyamNumberResponce,
                StaticResponseType.Pan => result.Data.PanNumberResponce,
                StaticResponseType.Gst => result.Data.GSTNumberResponce,
                StaticResponseType.Gst2B => result.Data.GST2BResponce,
                StaticResponseType.Gst3B => result.Data.GST3BResponce,
                StaticResponseType.Itr => result.Data.ITRNumberResponce,
                StaticResponseType.Ip => result.Data.IPResponce,
                StaticResponseType.Mca => result.Data.MCAResponce,
                StaticResponseType.Din => result.Data.DINResponce,
                _ => null
            };
        }

        public async Task<T?> GetStaticCommonResponce<T>(string udyamNumber, StaticResponseType type) where T : class
        {
            var json = await GetStaticCommonResponce(udyamNumber, type);

            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonConvert.DeserializeObject<T>(json);
        }
    }
}
