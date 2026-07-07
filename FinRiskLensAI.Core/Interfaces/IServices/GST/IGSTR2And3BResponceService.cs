using System.Threading.Tasks;
using FinRiskLensAI.Core.Models.GST;

namespace FinRiskLensAI.Core.Interfaces.IServices.GST
{
    /// <summary>
    /// Serves the latest stored GSTR-2B/3B response payloads for the currently
    /// logged-in MSME. The UdyamNumber is resolved from the HTTP session, never
    /// supplied by the caller/UI.
    /// </summary>
    public interface IGSTR2And3BResponceService
    {
        /// <summary>
        /// Latest t_GSTR2And3BResponce row for the session's UdyamNumber,
        /// or null when the user isn't signed in or has no stored record.
        /// </summary>
        Task<GSTR2And3BResponceResult?> GetResponces();
    }
}
