using System;
using System.Text;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Internal utility functions for string encoding, hex conversion, and timestamp formatting.
    /// These utilities are exposed through the <see cref="CircularEnterpriseApis"/> class for public access.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Converts a UTF-8 string to its hexadecimal representation (uppercase).
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns>Uppercase hexadecimal string without separators, or empty string if input is null/empty</returns>
        public static string StringToHex(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>
        /// Converts a hexadecimal string back to its original UTF-8 string representation.
        /// Handles hex strings with or without "0x" prefix.
        /// Returns empty string if the hex string has odd length or contains invalid characters.
        /// </summary>
        /// <param name="hexStr">The hexadecimal string to convert</param>
        /// <returns>Decoded UTF-8 string, or empty string on error</returns>
        public static string HexToString(string hexStr)
        {
            if (string.IsNullOrEmpty(hexStr))
                return "";

            // Remove any prefixes first
            string originalHex = hexStr;
            if (hexStr.StartsWith("0x") || hexStr.StartsWith("0X"))
                hexStr = hexStr.Substring(2);

            // Check if original (after prefix removal) is odd length - if so return empty
            if (hexStr.Length % 2 != 0)
                return "";

            // Now apply HexFix for normalization (but we already know it's even length)
            hexStr = HexFix(originalHex);

            try
            {
                byte[] bytes = new byte[hexStr.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hexStr.Substring(i * 2, 2), 16);
                }
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Normalizes a hexadecimal string for blockchain operations.
        /// Removes "0x" prefix, converts to lowercase, and pads with a leading zero if length is odd.
        /// </summary>
        /// <param name="hexStr">The hexadecimal string to normalize</param>
        /// <returns>Normalized hex string (lowercase, no prefix, even length)</returns>
        public static string HexFix(string hexStr)
        {
            if (string.IsNullOrEmpty(hexStr))
                return "";

            // Remove common hex prefixes
            if (hexStr.StartsWith("0x") || hexStr.StartsWith("0X"))
                hexStr = hexStr.Substring(2);

            // Convert to lowercase for consistency
            hexStr = hexStr.ToLowerInvariant();

            // Pad with '0' if length is odd
            if (hexStr.Length % 2 != 0)
                hexStr = "0" + hexStr;

            return hexStr;
        }

        /// <summary>
        /// Pads a number with a leading zero if it's a single digit (0-9).
        /// Numbers outside this range are returned as-is.
        /// </summary>
        /// <param name="num">The number to pad</param>
        /// <returns>String with leading zero for single positive digits, unchanged otherwise</returns>
        public static string PadNumber(int num)
        {
            if (num >= 0 && num < 10)
            {
                return "0" + num.ToString();
            }
            return num.ToString();
        }

        /// <summary>
        /// Gets the current UTC timestamp in Circular Protocol format.
        /// Format: YYYY:MM:DD-HH:MM:SS (uses colons for date separators, not dashes)
        /// </summary>
        /// <returns>Formatted timestamp string for blockchain transactions</returns>
        public static string GetFormattedTimestamp()
        {
            var now = DateTime.UtcNow;
            return $"{now.Year}:{PadNumber(now.Month)}:{PadNumber(now.Day)}-{PadNumber(now.Hour)}:{PadNumber(now.Minute)}:{PadNumber(now.Second)}";
        }
    }
}
