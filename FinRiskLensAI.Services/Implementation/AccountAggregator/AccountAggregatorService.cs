using FinRiskLensAI.Core.Common;
using FinRiskLensAI.Core.Interfaces.ICommon;
using FinRiskLensAI.Core.Interfaces.IRepositories.AccountAggregator;
using FinRiskLensAI.Core.Interfaces.IServices.AccountAggregator;
using FinRiskLensAI.Core.Models.AccountAggregator;
using FinRiskLensAI.Core.Models.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FinRiskLensAI.Core.Models.AccountAggregator.ConsentReqRespModel;
using static FinRiskLensAI.Core.Models.AccountAggregator.ConsentStatusRespModel;
using static FinRiskLensAI.Core.Models.AccountAggregator.FinInfoReqRespModel;
using static FinRiskLensAI.Core.Models.AccountAggregator.FinInfoReqStatusModel;
using static FinRiskLensAI.Core.Models.AccountAggregator.FinvuTokenModel;

namespace FinRiskLensAI.Services.Implementation.AccountAggregator
{
    public class AccountAggregatorService : IAccountAggregatorService
    {
        private readonly IAccountAggregatorRepository _aaRepo;
        private readonly IHttpClientHelper _httpClientHelper;
        private readonly FinvuSettings _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountAggregatorService(
            IAccountAggregatorRepository aaRepo, IHttpClientHelper httpClientHelper,
            FinvuSettings config, IHttpContextAccessor httpContextAccessor)
        {
            _aaRepo = aaRepo; _httpClientHelper = httpClientHelper;
            _config = config; _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// AA consent-callback URL: the configured override if set, otherwise
        /// built from the current request so it's correct both locally and after publish.
        /// </summary>
        private string ResolveCallbackUrl()
        {
            if (!string.IsNullOrWhiteSpace(_config.CallbackUrl))
                return _config.CallbackUrl;

            var req = _httpContextAccessor.HttpContext?.Request;
            return req != null
                ? $"{req.Scheme}://{req.Host}/Home/AAConsentCallback"
                : "/Home/AAConsentCallback";
        }

        private async Task EnsureTokenValidAsync()
        {
            bool isTokenValid = _aaRepo.CheckAATokenValidity();
            if (!isTokenValid)
                await GenerateFinvuToken().ConfigureAwait(false);
        }

        private object BuildFinvuHeader() => new
        {
            rid = Guid.NewGuid().ToString(),
            ts = DateTime.UtcNow.ToString(
                            "yyyy-MM-ddTHH:mm:ss.fff",
                            CultureInfo.InvariantCulture) + "+00:00",
            channelId = _config.FinvuChannelId
        };

        private Dictionary<string, string> BuildAuthHeader(string token) =>
            new Dictionary<string, string> { { "Authorization", "Bearer " + token } };

        public async Task GenerateFinvuToken()
        {
            try
            {
                var payload = new
                {
                    header = BuildFinvuHeader(),
                    body = new
                    {
                        userId = _config.AaUserId,
			            password = _config.AaPassword
                    }
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);

                string responseContent = await _httpClientHelper
                    .SendPostRequest(
                        _config.FinvuApi + "/User/Login",
                        jsonPayload, null)
                    .ConfigureAwait(false);

                var resp = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                var tokenData = _aaRepo.GetAAToken();

                if (tokenData == null)
                    return;

                tokenData.rid = resp.header.rid;
                tokenData.ts = resp.header.ts;
                tokenData.token = resp.body.token;
                tokenData.UpdatedOn = DateTime.Now;

                var updatedToken = await _aaRepo.UpdateAAToken(tokenData).ConfigureAwait(false);

                if (updatedToken == null)
                {
                    // Update failed
                    return;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> CreateConsentRequest(string transactionId, string AAID)
        {
            try
            {
                await EnsureTokenValidAsync().ConfigureAwait(false);
                AccAggreTokenModel aaToken = _aaRepo.GetAAToken();

                var payload = new
                {
                    header = BuildFinvuHeader(),
                    body = new
                    {
                        custId = AAID,
                        consentDescription = "Wealth Management Service",
                        templateName = "FINVUDEMO_TESTING",
                        aaId = "cookiejar-aa@finvu.in",
                        redirectUrl = ResolveCallbackUrl()
                    }
                };

                HttpResponseMessage response = await _httpClientHelper
                                                        .SendPostRequestFullResp(
                                                            _config.FinvuApi + "/ConsentRequestPlus",
                                                            JsonConvert.SerializeObject(payload),
                                                            BuildAuthHeader(aaToken.token))
                                                        .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    //_logger.Error("CreateConsentRequest HTTP error: " +await response.Content.ReadAsStringAsync());
                    return null;
                }

                string responseContent = await response.Content
                                                       .ReadAsStringAsync()
                                                       .ConfigureAwait(false);
                var resp = JsonConvert.DeserializeObject<ConsentRespModel>(responseContent);
                //_logger.Info("CreateConsentRequest: " + responseContent);

                AAConsentReqModel aaConReqMdl = new AAConsentReqModel
                {
                    transactionId = transactionId,
                    rid = resp.header.rid,
                    ts = resp.header.ts,
                    channelId = resp.header.channelId,
                    encryptedRequest = resp.body.encryptedRequest,
                    requestDate = resp.body.requestDate,
                    encryptedFiuId = resp.body.encryptedFiuId,
                    consentHandle = resp.body.ConsentHandle,
                    url = resp.body.url,
                    CreatedOn = DateTime.Now
                };

                var saveResult = await _aaRepo
                                          .AddAAConsentRequest(aaConReqMdl)
                                          .ConfigureAwait(false);

                if (saveResult.Result != tflResultType.tflSuccess)
                    //_logger.Error("AddAAConsentRequest failed: " + saveResult.Message);
                    return null;

                return resp.body.ConsentHandle;
            }
            catch (Exception ex)
            {
                //_logger.Error("Error from CreateConsentRequest: " + ex.Message);
                return null;
            }
        }

        public async Task<AAConsentReqModel> GetAAConsentData(string transactionId)
        {
            return await _aaRepo
                             .GetAAConsentRequest(transactionId)
                             .ConfigureAwait(false);
        }

        public async Task<ConsentDetailsById> CheckConsentStatus(string consentHandle, string AAID)
        {
            try
            {
                await EnsureTokenValidAsync().ConfigureAwait(false);

                var aaToken = _aaRepo.GetAAToken();
                if (aaToken == null)
                    return null;

                HttpResponseMessage response = await _httpClientHelper
                    .SendGetRequestFullResp(
                        _config.FinvuApi + $"/ConsentStatus/{consentHandle}/{AAID}",
                        BuildAuthHeader(aaToken.token))
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string responseContent = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                var ConsentStatusResp = JsonConvert.DeserializeObject<ConsentStatusModel>(responseContent);

                HttpResponseMessage response2 = await _httpClientHelper
                    .SendGetRequestFullResp(
                        _config.FinvuApi + $"/Consent/{ConsentStatusResp.body.consentId}",
                        BuildAuthHeader(aaToken.token))
                    .ConfigureAwait(false);

                string responseContent2 = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                var ConsentDetResp = JsonConvert.DeserializeObject<ConsentDetailsById>(responseContent2);

                return ConsentDetResp;
            }
            catch
            {
                return null;
            }
        }

        public async Task<FinInfoRespModel> FinancialInfoRequest(
            string consentHandle, string AAID, string consentId)
        {
            try
            {
                await EnsureTokenValidAsync().ConfigureAwait(false);
                AccAggreTokenModel aaToken = _aaRepo.GetAAToken();

                var payload = new
                {
                    header = BuildFinvuHeader(),
                    body = new
                    {
                        custId = AAID,
                        consentHandleId = consentHandle,
                        consentId = consentId,
                        dateTimeRangeFrom = "2024-11-01T12:14:43.727+0000",
                        dateTimeRangeTo = "2025-01-17T12:14:43.727+0000"
                    }
                };

                HttpResponseMessage response = await _httpClientHelper
                                                        .SendPostRequestFullResp(
                                                           _config.FinvuApi + "/FIRequest",
                                                            JsonConvert.SerializeObject(payload),
                                                            BuildAuthHeader(aaToken.token))
                                                        .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                   // _logger.Error("FinancialInfoRequest HTTP error: " + await response.Content.ReadAsStringAsync());
                    return null;
                }

                string responseContent = await response.Content
                                                       .ReadAsStringAsync()
                                                       .ConfigureAwait(false);
                return JsonConvert.DeserializeObject<FinInfoRespModel>(responseContent);
            }
            catch (Exception ex)
            {
                //_logger.Error("Error from FinancialInfoRequest: " + ex.Message);
                return null;
            }
            return null;
        }

        public async Task<FinInfoRespStatusModel> FinancialInfoReqStatus(
            string consentHandle, string AAID, string consentId, string sessionId)
        {
           try
           {
               await EnsureTokenValidAsync().ConfigureAwait(false);
               AccAggreTokenModel aaToken = _aaRepo.GetAAToken();

               HttpResponseMessage response = await _httpClientHelper
                                                       .SendGetRequestFullResp(
                                                           _config.FinvuApi + $"/FIStatus/{consentId}/{sessionId}/{consentHandle}/{AAID}",
                                                           BuildAuthHeader(aaToken.token))
                                                       .ConfigureAwait(false);

               if (!response.IsSuccessStatusCode)
               {
                 //  _logger.Error("FinancialInfoReqStatus HTTP error: " +await response.Content.ReadAsStringAsync());
                   return null;
               }

               string responseContent = await response.Content
                                                      .ReadAsStringAsync()
                                                      .ConfigureAwait(false);
               return JsonConvert.DeserializeObject<FinInfoRespStatusModel>(responseContent);
           }
           catch (Exception ex)
           {
               //_logger.Error("Error from FinancialInfoReqStatus: " + ex.Message);
               return null;
           }
            return null;
        }
        public async Task<string> FinancialInfoFetch(string AAID, string consentId, string sessionId)
        {
            try
            {
                await EnsureTokenValidAsync().ConfigureAwait(false);
                AccAggreTokenModel aaToken = _aaRepo.GetAAToken();

                HttpResponseMessage response = await _httpClientHelper
                                                        .SendGetRequestFullResp(
                                                           _config.FinvuApi + $"/FIFetch/{AAID}/{consentId}/{sessionId}",
                                                            BuildAuthHeader(aaToken.token))
                                                        .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    //_logger.Error("FinancialInfoFetch HTTP error: " +await response.Content.ReadAsStringAsync());
                    return null;
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //_logger.Error("Error from FinancialInfoFetch: " + ex.Message);
                return null;
            }
            return null;
        }
    }
}
