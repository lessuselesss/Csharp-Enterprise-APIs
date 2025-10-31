using System;
using System.Threading.Tasks;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Core package containing utility functions and constants for Circular Protocol blockchain operations.
    /// Provides convenient access to common utilities needed for certificate submission and blockchain interaction.
    /// </summary>
    public static class CircularEnterpriseApis
    {
        /// <summary>
        /// The current version of the Circular Enterprise APIs library.
        /// This version is included in all blockchain requests.
        /// </summary>
        public const string LibVersion = Constants.LibVersion;

        /// <summary>
        /// The default blockchain identifier for Circular Protocol.
        /// Use this unless you need to target a specific blockchain network.
        /// </summary>
        public const string DefaultChain = Constants.DefaultChain;

        /// <summary>
        /// The default Network Access Gateway (NAG) URL for blockchain communication.
        /// </summary>
        public const string DefaultNAG = Constants.DefaultNAG;

        /// <summary>
        /// The base URL used for network discovery and NAG resolution.
        /// </summary>
        public static string NetworkURL = Constants.NetworkURL;

        /// <summary>
        /// Discovers the Network Access Gateway (NAG) URL for a specified blockchain network.
        /// This is used internally by <see cref="CEPAccount.SetNetworkAsync(string)"/>.
        /// </summary>
        /// <param name="network">The network identifier ("testnet", "mainnet", or "devnet")</param>
        /// <returns>
        /// A tuple containing the NAG URL and an error message.
        /// If successful: (url: "https://...", error: null)
        /// If failed: (url: "", error: "error message")
        /// </returns>
        /// <example>
        /// <code>
        /// var (nagUrl, error) = await CircularEnterpriseApis.GetNAGAsync("testnet");
        /// if (error != null)
        /// {
        ///     Console.WriteLine($"Network discovery failed: {error}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"NAG URL: {nagUrl}");
        /// }
        /// </code>
        /// </example>
        public static async Task<(string url, string? error)> GetNAGAsync(string network)
        {
            return await Common.GetNAGAsync(network);
        }

        /// <summary>
        /// Converts a string to its hexadecimal representation (uppercase).
        /// Used internally for encoding certificate data and blockchain payloads.
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns>Uppercase hexadecimal string, or empty string if input is null/empty</returns>
        /// <example>
        /// <code>
        /// string hex = CircularEnterpriseApis.StringToHex("Hello");
        /// Console.WriteLine(hex); // Output: "48656C6C6F"
        /// </code>
        /// </example>
        public static string StringToHex(string s) => Utils.StringToHex(s);

        /// <summary>
        /// Converts a hexadecimal string back to its original string representation.
        /// Accepts hex strings with or without "0x" prefix.
        /// </summary>
        /// <param name="hexStr">The hexadecimal string to convert (with or without "0x" prefix)</param>
        /// <returns>The decoded string, or empty string on error or invalid input</returns>
        /// <example>
        /// <code>
        /// string original = CircularEnterpriseApis.HexToString("48656C6C6F");
        /// Console.WriteLine(original); // Output: "Hello"
        ///
        /// // Also works with 0x prefix
        /// string withPrefix = CircularEnterpriseApis.HexToString("0x48656C6C6F");
        /// Console.WriteLine(withPrefix); // Output: "Hello"
        /// </code>
        /// </example>
        public static string HexToString(string hexStr) => Utils.HexToString(hexStr);

        /// <summary>
        /// Normalizes a hexadecimal string by removing the "0x" prefix and ensuring proper formatting.
        /// Converts to lowercase and pads with a leading zero if the length is odd.
        /// </summary>
        /// <param name="hexStr">The hexadecimal string to normalize</param>
        /// <returns>Normalized hexadecimal string (lowercase, no prefix, even length)</returns>
        /// <example>
        /// <code>
        /// string fixed1 = CircularEnterpriseApis.HexFix("0x123");
        /// Console.WriteLine(fixed1); // Output: "0123"
        ///
        /// string fixed2 = CircularEnterpriseApis.HexFix("0xABCD");
        /// Console.WriteLine(fixed2); // Output: "abcd"
        /// </code>
        /// </example>
        public static string HexFix(string hexStr) => Utils.HexFix(hexStr);

        /// <summary>
        /// Pads a single-digit number with a leading zero.
        /// Used for formatting timestamps and other protocol-specific data.
        /// </summary>
        /// <param name="num">The number to pad (if between 0-9)</param>
        /// <returns>String representation with leading zero for single digits, unchanged otherwise</returns>
        /// <example>
        /// <code>
        /// string padded = CircularEnterpriseApis.PadNumber(5);
        /// Console.WriteLine(padded); // Output: "05"
        ///
        /// string notPadded = CircularEnterpriseApis.PadNumber(15);
        /// Console.WriteLine(notPadded); // Output: "15"
        /// </code>
        /// </example>
        public static string PadNumber(int num) => Utils.PadNumber(num);

        /// <summary>
        /// Gets the current UTC timestamp formatted for Circular Protocol blockchain operations.
        /// Format: YYYY:MM:DD-HH:MM:SS (note: colons in date, not dashes)
        /// </summary>
        /// <returns>Formatted timestamp string</returns>
        /// <example>
        /// <code>
        /// string timestamp = CircularEnterpriseApis.GetFormattedTimestamp();
        /// Console.WriteLine(timestamp); // Output: "2024:10:31-14:30:45"
        /// </code>
        /// </example>
        public static string GetFormattedTimestamp() => Utils.GetFormattedTimestamp();
    }
}
