namespace FinRiskLensAI.Core.Interfaces.IServices.Common
{
    public interface IDummyDataService
    {
        /// <summary>Random-but-valid dummy Udyam API response (JSON) for a UAN.</summary>
        string GetDummyUdyam(string uan);

        /// <summary>
        /// Dummy MCA (Ministry of Corporate Affairs) response (JSON) for a company —
        /// same company/owner name as the Udyam/GST data, with dynamic charges.
        /// </summary>
        string GetDummyMca(string uan, string companyName, string? pan = null);
    }
}
