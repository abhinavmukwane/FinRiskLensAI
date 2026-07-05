namespace FinRiskLensAI.Core.Interfaces.IServices.Common
{
    public interface IDummyDataService
    {
        /// <summary>Random-but-valid dummy Udyam API response (JSON) for a UAN.</summary>
        string GetDummyUdyam(string uan);
    }
}
