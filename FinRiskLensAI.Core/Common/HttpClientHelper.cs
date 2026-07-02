using FinRiskLensAI.Core.Interfaces.ICommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Common
{
    public class HttpClientHelper : IHttpClientHelper
    {
        //private readonly IAppLogger<HttpClientHelper> _logger;
        //private readonly IApiFailureAlertService _alertService;

        // Static HttpClient stays — one instance for the app lifetime
        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
        {
            CheckCertificateRevocationList = true
        });

        // Guard ensures ServicePointManager is configured only once
        private static bool _servicePointConfigured = false;
        private static readonly object _configLock = new object();


        public HttpClientHelper()
        {

            EnsureServicePointConfigured();
        }
        //public HttpClientHelper(IAppLogger<HttpClientHelper> logger, IApiFailureAlertService alertService)
        //{
        //    _logger = logger;
        //    _alertService = alertService;

        //    EnsureServicePointConfigured();
        //}

        private void EnsureServicePointConfigured()
        {
            if (_servicePointConfigured) return;

            lock (_configLock)
            {
                if (_servicePointConfigured) return;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                _servicePointConfigured = true;
            }
        }

        //Extracted to a named instance method — no more lambda capturing issues
        private bool ValidateServerCertificate(object sender, X509Certificate cert, X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            string requestUrl = sender is HttpWebRequest req ? req.RequestUri.ToString() : string.Empty;

            if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
            {
               // _logger.Error($"[HttpClientHelper] SSL Policy Error for URL: {requestUrl} — {sslPolicyErrors}");
                return false;
            }

            try
            {
                var cert2 = new X509Certificate2(cert);

                var chainPolicy = new X509ChainPolicy
                {
                    RevocationMode = X509RevocationMode.Online,
                    RevocationFlag = X509RevocationFlag.EntireChain,
                    VerificationFlags = X509VerificationFlags.NoFlag,
                    UrlRetrievalTimeout = TimeSpan.FromSeconds(15)
                };


                using (var x509Chain = new X509Chain())
                {
                    x509Chain.ChainPolicy = chainPolicy;

                    bool isValid = x509Chain.Build(cert2);

                    if (!isValid)
                    {
                        foreach (var status in x509Chain.ChainStatus)
                        {
                            //_logger.Error($"[HttpClientHelper] Chain validation error for URL: {requestUrl} — {status.StatusInformation}");
                        }
                    }

                    return isValid;
                }

            }
            catch (Exception ex)
            {
                //_logger.Error($"[HttpClientHelper] Certificate validation exception for URL: {requestUrl} — {ex}");
                return false;
            }
        }

        private static void AttachHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers == null) return;
            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);
        }

        public async Task<string> SendPostRequest(string url, string jsonString, Dictionary<string, string> headers = null)
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                AttachHeaders(request, headers);
                request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                   // _logger.Error($"[HttpClientHelper] POST failed | URL: {url} | Status: {(int)response.StatusCode} | CorrelationId: {correlationId}");

                    //_ = Task.Run(() => _alertService.TrackApiFailure(
                    //    url,
                    //    $"HTTP {(int)response.StatusCode} | {responseContent}",
                    //    correlationId));
                }

                return responseContent;
            }
            catch (TaskCanceledException ex)
            {
               // _logger.Error($"[HttpClientHelper] POST timeout | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               // _ = Task.Run(() => _alertService.TrackApiFailure(url, $"TIMEOUT: {ex.Message}", correlationId));
                return $"Error: {ex.Message}";
            }
            catch (HttpRequestException ex)
            {
              //  _logger.Error($"[HttpClientHelper] POST network error | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               // _ = Task.Run(() => _alertService.TrackApiFailure(url, $"NETWORK_ERROR: {ex.Message}", correlationId));
                return $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
               // _logger.Error($"[HttpClientHelper] POST exception | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               // _ = Task.Run(() => _alertService.TrackApiFailure(url, $"Exception | {ex.Message}", correlationId));
                return $"Error: {ex.Message}";
            }
        }

        public async Task<HttpResponseMessage> SendPostRequestFullResp(string url, string jsonString, Dictionary<string, string> headers = null)
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                AttachHeaders(request, headers);
                request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    //_logger.Error($"[HttpClientHelper] POST (FullResp) failed | URL: {url} | Status: {(int)response.StatusCode} | CorrelationId: {correlationId}");

                    //_ = Task.Run(() => _alertService.TrackApiFailure(
                    //    url,
                    //    $"HTTP {(int)response.StatusCode} | {responseContent}",
                    //    correlationId));
                }

                return response;
            }
            catch (TaskCanceledException ex)
            {
               // _logger.Error($"[HttpClientHelper] POST (FullResp) timeout | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               // _ = Task.Run(() => _alertService.TrackApiFailure(url, $"TIMEOUT: {ex.Message}", correlationId));

                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    Content = new StringContent($"Error: {ex.Message}")
                };
            }
            catch (HttpRequestException ex)
            {
               //_logger.Error($"[HttpClientHelper] POST (FullResp) network error | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               //_ = Task.Run(() => _alertService.TrackApiFailure(url, $"NETWORK_ERROR: {ex.Message}", correlationId));

                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Error: {ex.Message}")
                };
            }
            catch (Exception ex)
            {
               // _logger.Error($"[HttpClientHelper] POST (FullResp) exception | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               // _ = Task.Run(() => _alertService.TrackApiFailure(url, $"Exception | {ex.Message}", correlationId));

                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Error: {ex.Message}")
                };
            }
        }


        public async Task<string> SendGetRequest(string url, Dictionary<string, string> headers = null)
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AttachHeaders(request, headers);

                var response = await _httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    //_logger.Error($"[HttpClientHelper] GET failed | URL: {url} | Status: {(int)response.StatusCode} | CorrelationId: {correlationId}");

                    //_ = Task.Run(() => _alertService.TrackApiFailure(
                    //    url,
                    //    $"HTTP {(int)response.StatusCode} | {responseContent}",
                    //    correlationId));
                }

                return responseContent;
            }
            catch (TaskCanceledException ex)
            {
               // _logger.Error($"[HttpClientHelper] GET timeout | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               // _ = Task.Run(() => _alertService.TrackApiFailure(url, $"TIMEOUT: {ex.Message}", correlationId));
                return $"Error: {ex.Message}";
            }
            catch (HttpRequestException ex)
            {
                //_logger.Error($"[HttpClientHelper] GET network error | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
                //_ = Task.Run(() => _alertService.TrackApiFailure(url, $"NETWORK_ERROR: {ex.Message}", correlationId));
                return $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                //_logger.Error($"[HttpClientHelper] GET exception | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
                //_ = Task.Run(() => _alertService.TrackApiFailure(url, $"Exception | {ex.Message}", correlationId));
                return $"Error: {ex.Message}";
            }
        }

        public async Task<HttpResponseMessage> SendGetRequestFullResp(string url, Dictionary<string, string> headers = null)
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AttachHeaders(request, headers);

                var response = await _httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    //_logger.Error($"[HttpClientHelper] GET (FullResp) failed | URL: {url} | Status: {(int)response.StatusCode} | CorrelationId: {correlationId}");

                    //_ = Task.Run(() => _alertService.TrackApiFailure(
                    //    url,
                    //    $"HTTP {(int)response.StatusCode} | {responseContent}",
                    //    correlationId));
                }

                return response;
            }
            catch (TaskCanceledException ex)
            {
               // _logger.Error($"[HttpClientHelper] GET (FullResp) timeout | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               // _ = Task.Run(() => _alertService.TrackApiFailure(url, $"TIMEOUT: {ex.Message}", correlationId));

                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    Content = new StringContent($"Error: {ex.Message}")
                };
            }
            catch (HttpRequestException ex)
            {
               // _logger.Error($"[HttpClientHelper] GET (FullResp) network error | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
               // _ = Task.Run(() => _alertService.TrackApiFailure(url, $"NETWORK_ERROR: {ex.Message}", correlationId));

                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Error: {ex.Message}")
                };
            }
            catch (Exception ex)
            {
                //_logger.Error($"[HttpClientHelper] GET (FullResp) exception | URL: {url} | CorrelationId: {correlationId} | {ex.Message}");
                //_ = Task.Run(() => _alertService.TrackApiFailure(url, $"Exception | {ex.Message}", correlationId));

                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Error: {ex.Message}")
                };
            }
        }
    }
}
