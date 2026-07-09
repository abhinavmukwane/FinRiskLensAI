namespace FinRiskLensAI.Core.Models.Universal
{
    /// <summary>
    /// Identifies one cached JSON response column of m_StaticResponces,
    /// so callers can ask the common static-response service for exactly
    /// the payload they need against a Udyam number.
    /// </summary>
    public enum StaticResponseType
    {
        Udyam,
        Pan,
        Gst,
        Gst2B,
        Gst3B,
        Itr,
        Ip,
        Mca,
        Din
    }
}
