using System;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Package-level factory functions that exactly match Go implementation
    /// Enables identical IntelliSense discovery patterns:
    /// Go: circular_enterprise_apis.NewCEPAccount()
    /// C#: CircularEnterpriseApis.NewCEPAccount()
    /// </summary>
    public static class CircularEnterpriseApis
    {
        /// <summary>
        /// Creates a new CEPAccount instance - matches Go package-level function exactly
        /// Maps to Go: func NewCEPAccount() *CEPAccount
        /// Usage: var account = CircularEnterpriseApis.NewCEPAccount();
        /// </summary>
        public static CEPAccount NewCEPAccount()
        {
            return CEPAccount.NewCEPAccount();
        }

        /// <summary>
        /// Creates a new CCertificate instance - matches Go package-level function exactly
        /// Maps to Go: func NewCCertificate() *CCertificate
        /// Usage: var cert = CircularEnterpriseApis.NewCCertificate();
        /// </summary>
        public static CCertificate NewCCertificate()
        {
            return CCertificate.NewCCertificate();
        }

        // Re-expose constants at package level to match Go exactly
        // Go: circular_enterprise_apis.LibVersion
        // C#: CircularEnterpriseApis.LibVersion
        public const string LibVersion = Constants.LibVersion;
        public const string DefaultChain = Constants.DefaultChain;
        public const string DefaultNAG = Constants.DefaultNAG;
        public static string NetworkURL = Constants.NetworkURL;

        /// <summary>
        /// Package-level NAG discovery function matching Go exactly
        /// Maps to Go: func GetNAG(network string) (string, error)
        /// Returns Go-style tuple: (url, errorMessage) instead of throwing exceptions
        /// </summary>
        public static (string url, string? error) GetNAG(string network)
        {
            return Common.GetNAGInternal(network);
        }

        // Package-level utility functions matching Go utils package exactly
        // Go: utils.StringToHex("test")
        // C#: CircularEnterpriseApis.StringToHex("test")

        /// <summary>
        /// Converts a string to hexadecimal representation
        /// Maps to Go: func StringToHex(s string) string
        /// </summary>
        public static string StringToHex(string s) => Utils.StringToHex(s);

        /// <summary>
        /// Converts hexadecimal string back to original string
        /// Maps to Go: func HexToString(hexStr string) string
        /// </summary>
        public static string HexToString(string hexStr) => Utils.HexToString(hexStr);

        /// <summary>
        /// Fixes and normalizes a hex string
        /// Maps to Go: func HexFix(hexStr string) string
        /// </summary>
        public static string HexFix(string hexStr) => Utils.HexFix(hexStr);

        /// <summary>
        /// Pads a number to ensure it's at least 2 digits
        /// Maps to Go: func PadNumber(num int) string
        /// </summary>
        public static string PadNumber(int num) => Utils.PadNumber(num);

        /// <summary>
        /// Gets formatted timestamp for Circular Protocol
        /// Maps to Go: func GetFormattedTimestamp() string
        /// </summary>
        public static string GetFormattedTimestamp() => Utils.GetFormattedTimestamp();
    }
}