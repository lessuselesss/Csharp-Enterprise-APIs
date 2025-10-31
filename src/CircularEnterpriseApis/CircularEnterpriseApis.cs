using System;
using System.Threading.Tasks;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Package-level utility functions and constants
    /// Provides access to common utilities for blockchain operations
    /// </summary>
    public static class CircularEnterpriseApis
    {
        // Package-level constants
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
        [Obsolete("Use GetNAGAsync() instead. Synchronous methods will be removed in v2.0.0.", false)]
        public static (string url, string? error) GetNAG(string network)
        {
            return Common.GetNAGInternal(network);
        }

        /// <summary>
        /// Async package-level NAG discovery function matching Rust
        /// Maps to Rust: pub async fn get_nag(network: &str) -> Result<String, String>
        /// Returns tuple: (url, errorMessage) instead of throwing exceptions
        /// </summary>
        public static async Task<(string url, string? error)> GetNAGAsync(string network)
        {
            return await Common.GetNAGAsync(network);
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