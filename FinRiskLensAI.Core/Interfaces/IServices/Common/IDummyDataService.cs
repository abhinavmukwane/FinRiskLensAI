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

        /// <summary>
        /// Dummy DIN (Director Identification Number) verification response (JSON) for a
        /// single director, mirroring the real MCA DIN API shape. Name is taken from the
        /// MCA director entry so the DIN profile stays in sync with the company's board.
        /// </summary>
        string GetDummyDin(string din, string directorName, string? pan = null);
    }
}
