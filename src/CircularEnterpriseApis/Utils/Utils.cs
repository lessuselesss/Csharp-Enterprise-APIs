using System;
using System.Text;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Utility functions that match Go package-level functions exactly
    /// Moved to root namespace for direct access like Go: StringToHex("test")
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Converts a string to its hexadecimal representation
        /// Matches Go: func StringToHex(s string) string
        /// Returns UPPERCASE hex to match Go exactly
        /// </summary>
        public static string StringToHex(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>
        /// Converts a hexadecimal string back to its original string representation
        /// Matches Go: func HexToString(hexStr string) string
        /// </summary>
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
        /// Fixes and normalizes a hex string by removing prefixes and ensuring lowercase
        /// Matches Go: func HexFix(hexStr string) string exactly
        /// </summary>
        public static string HexFix(string hexStr)
        {
            if (string.IsNullOrEmpty(hexStr))
                return "";

            // Remove common hex prefixes
            if (hexStr.StartsWith("0x") || hexStr.StartsWith("0X"))
                hexStr = hexStr.Substring(2);

            // Convert to lowercase for consistency (matches Go implementation)
            hexStr = hexStr.ToLowerInvariant();

            // Pad with '0' if length is odd (matches Go exactly)
            if (hexStr.Length % 2 != 0)
                hexStr = "0" + hexStr;

            return hexStr;
        }

        /// <summary>
        /// Pads a number to ensure it's at least 2 digits with leading zeros
        /// Matches Go: func PadNumber(num int) string
        /// EXACTLY matches Go behavior: only pads single positive digits
        /// </summary>
        public static string PadNumber(int num)
        {
            if (num >= 0 && num < 10)
            {
                return "0" + num.ToString();
            }
            return num.ToString();
        }

        /// <summary>
        /// Gets the current timestamp in the format used by Circular Protocol
        /// Matches Go: func GetFormattedTimestamp() string
        /// Format: YYYY:MM:DD-HH:MM:SS
        /// </summary>
        public static string GetFormattedTimestamp()
        {
            var now = DateTime.UtcNow;
            return $"{now.Year}:{PadNumber(now.Month)}:{PadNumber(now.Day)}-{PadNumber(now.Hour)}:{PadNumber(now.Minute)}:{PadNumber(now.Second)}";
        }
    }
}