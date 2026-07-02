using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Interfaces.ICommon
{
    public interface IHttpClientHelper
    {
        Task<string> SendPostRequest(string url, string jsonString, Dictionary<string, string> headers = null);
        Task<string> SendGetRequest(string url, Dictionary<string, string> headers = null);
        Task<HttpResponseMessage> SendPostRequestFullResp(string url, string jsonString, Dictionary<string, string> headers = null);
        Task<HttpResponseMessage> SendGetRequestFullResp(string url, Dictionary<string, string> headers = null);
    }
}
