using System;
using System.Text.RegularExpressions;

namespace FinRiskLensAI.Core.Common
{
    /// <summary>
    /// Universal formatting/validation helpers used throughout the application —
    /// masking of sensitive identifiers (PAN, GSTIN, Aadhaar, mobile, email) and
    /// format validation for the Indian identifiers the platform handles.
    /// Keep every cross-cutting value check here; do not re-implement per screen.
    /// </summary>
    public static class Universal
    {
        private const char MaskChar = 'X';

        // ── Masking ──────────────────────────────────────────────────────

        /// <summary>
        /// Generic masker: keeps <paramref name="visibleStart"/> leading and
        /// <paramref name="visibleEnd"/> trailing characters, masks the rest.
        /// Returns the input unchanged when it is too short to mask meaningfully.
        /// </summary>
        public static string MaskValue(string? value, int visibleStart, int visibleEnd)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            value = value.Trim();
            if (value.Length <= visibleStart + visibleEnd) return value;

            return value[..visibleStart]
                 + new string(MaskChar, value.Length - visibleStart - visibleEnd)
                 + value[^visibleEnd..];
        }

        /// <summary>PAN — keep the last 4 characters: AABCT1859L → XXXXXX859L.</summary>
        public static string MaskPan(string? pan) => MaskValue(pan, 0, 4);

        /// <summary>GSTIN — keep the 2-digit state code and last 3: 27AABCT1859L1ZY → 27XXXXXXXXXX1ZY.</summary>
        public static string MaskGstin(string? gstin) => MaskValue(gstin, 2, 3);

        /// <summary>Aadhaar — keep only the last 4 digits: XXXXXXXX1234.</summary>
        public static string MaskAadhaar(string? aadhaar) => MaskValue(aadhaar, 0, 4);

        /// <summary>Mobile — keep the last 4 digits: XXXXXX5360.</summary>
        public static string MaskMobile(string? mobile) => MaskValue(mobile, 0, 4);

        /// <summary>Email — keep the first 2 characters of the local part and the full domain.</summary>
        public static string MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;

            var at = email.IndexOf('@');
            if (at <= 2) return email;

            return email[..2] + new string(MaskChar, at - 2) + email[at..];
        }

        // ── Validation ───────────────────────────────────────────────────

        private static readonly Regex PanRegex = new(@"^[A-Z]{5}[0-9]{4}[A-Z]$", RegexOptions.Compiled);
        private static readonly Regex GstinRegex = new(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$", RegexOptions.Compiled);
        private static readonly Regex UdyamRegex = new(@"^UDYAM-[A-Z]{2}-\d{2}-\d{7}$", RegexOptions.Compiled);
        private static readonly Regex MobileRegex = new(@"^[6-9]\d{9}$", RegexOptions.Compiled);
        private static readonly Regex AadhaarRegex = new(@"^\d{12}$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public static bool IsValidPan(string? pan)
            => !string.IsNullOrWhiteSpace(pan) && PanRegex.IsMatch(pan.Trim().ToUpperInvariant());

        public static bool IsValidGstin(string? gstin)
            => !string.IsNullOrWhiteSpace(gstin) && GstinRegex.IsMatch(gstin.Trim().ToUpperInvariant());

        public static bool IsValidUdyamNumber(string? uan)
            => !string.IsNullOrWhiteSpace(uan) && UdyamRegex.IsMatch(uan.Trim().ToUpperInvariant());

        public static bool IsValidMobile(string? mobile)
            => !string.IsNullOrWhiteSpace(mobile) && MobileRegex.IsMatch(mobile.Trim());

        public static bool IsValidAadhaar(string? aadhaar)
            => !string.IsNullOrWhiteSpace(aadhaar) && AadhaarRegex.IsMatch(aadhaar.Trim());

        public static bool IsValidEmail(string? email)
            => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());
    }
}
