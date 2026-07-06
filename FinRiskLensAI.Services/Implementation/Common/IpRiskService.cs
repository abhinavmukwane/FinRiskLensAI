using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinRiskLensAI.Core.Interfaces.ICommon;
using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Models.Common;
using FinRiskLensAI.Core.Models.Universal;
using Newtonsoft.Json;

namespace FinRiskLensAI.Services.Implementation.Common
{
    /// <summary>
    /// The one IP risk-score service. It reuses the shared HttpClientHelper for
    /// the Signzy call and the existing static-response service for the DB
    /// fallback. Switching to the live API later is a single toggle
    /// (Signzy:UseApi) — no controller or UI change required.
    /// </summary>
    public class IpRiskService : IIpRiskService
    {
        private const string RiskScorePath = "ipQuality/riskScores";

        private readonly SignzySettings _settings;
        private readonly IHttpClientHelper _http;
        private readonly IStaticResponseService _staticResponses;

        public IpRiskService(SignzySettings settings, IHttpClientHelper http, IStaticResponseService staticResponses)
        {
            _settings = settings;
            _http = http;
            _staticResponses = staticResponses;
        }

        public async Task<string?> GetIpRiskScoreAsync(string ip, string udyamNumber, CancellationToken ct = default)
        {
            // ── TEMPORARY (development) ──────────────────────────────────────
            // The production Authorization key isn't available yet, so instead of
            // calling the live API we return the stored response from
            // m_StaticResponces.IPResponce. When the real key arrives, set
            // Signzy:UseApi = true and the live call below takes over unchanged.
            if (!_settings.UseApi)
            {
                return string.IsNullOrWhiteSpace(udyamNumber)
                    ? null
                    : await _staticResponses.GetStaticCommonResponce(udyamNumber, StaticResponseType.Ip);
            }

            return await FetchFromApiAsync(ip);
        }

        /// <summary>
        /// The production Signzy call — POST {baseUrl}/ipQuality/riskScores with
        /// body { "ip": "<ip>" } and the configured Authorization header.
        /// </summary>
        private async Task<string?> FetchFromApiAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return null;

            var url = $"{_settings.BaseUrl.TrimEnd('/')}/{RiskScorePath}";
            var body = JsonConvert.SerializeObject(new { ip });

            // Content-Type is set by the helper's StringContent; only the extra
            // request headers go here.
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = _settings.Authorization,
                ["Accept"] = "application/json"
            };

            var response = await _http.SendPostRequest(url, body, headers);

            // HttpClientHelper reports transport failures as "Error: ..." strings.
            if (string.IsNullOrWhiteSpace(response) || response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
                return null;

            return response;
        }
    }
}
