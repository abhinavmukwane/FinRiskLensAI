using System.Linq;
using Microsoft.AspNetCore.Http;

namespace FinRiskLensAI.Common
{
    /// <summary>
    /// Common helper for resolving the calling client's IP address in ASP.NET
    /// Core (the classic HttpContext.Current / ServerVariables API doesn't exist
    /// here). Prefers the first X-Forwarded-For hop (behind a proxy/CDN) and
    /// falls back to the connection's RemoteIpAddress.
    /// </summary>
    public static class IP_Get_Service
    {
        public static string GetClientIPAddress(HttpContext context)
        {
            if (context == null) return string.Empty;

            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var addresses = forwarded.Split(',');
                if (addresses.Length > 0)
                    return addresses[0].Trim();
            }

            var remote = context.Connection.RemoteIpAddress;
            if (remote != null)
            {
                if (remote.IsIPv4MappedToIPv6)
                    remote = remote.MapToIPv4();
                return remote.ToString();
            }

            return string.Empty;
        }
    }
}
