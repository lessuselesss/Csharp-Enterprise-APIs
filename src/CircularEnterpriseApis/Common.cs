using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Package-level constants matching Go exactly
    /// Maps to Go package-level: const LibVersion = "1.0.13"
    /// Enables direct access: LibVersion (instead of Common.LibVersion)
    /// </summary>
    public static class Constants
    {
        // Package-level constants - exact values from Go implementation
        public const string LibVersion = "1.0.13";
        public const string DefaultChain = "0x8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2";
        public const string DefaultNAG = "https://nag.circularlabs.io/NAG.php?cep=";

        // Mutable like Go (var NetworkURL)
        public static string NetworkURL = "https://circularlabs.io/network/getNAG?network=";
    }

    /// <summary>
    /// Common functions and configuration matching the Go implementation
    /// Maps to Go: pkg/common.go
    /// </summary>
    public static class Common
    {
        // Re-expose constants for backward compatibility - will be removed in future version
        [Obsolete("Use Constants.LibVersion instead")]
        public const string LibVersion = Constants.LibVersion;
        [Obsolete("Use Constants.DefaultChain instead")]
        public const string DefaultChain = Constants.DefaultChain;
        [Obsolete("Use Constants.DefaultNAG instead")]
        public const string DefaultNAG = Constants.DefaultNAG;
        [Obsolete("Use Constants.NetworkURL instead")]
        public static string NetworkURL = Constants.NetworkURL;

        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };


        /// <summary>
        /// Async NAG discovery function - matches Rust async fn get_nag
        /// Maps to Rust: pub async fn get_nag(network: &str) -> Result<String, String>
        /// Returns (url, error) tuple instead of throwing exceptions
        /// </summary>
        public static async Task<(string url, string? error)> GetNAGAsync(string network)
        {
            if (string.IsNullOrEmpty(network))
                return ("", "network identifier cannot be empty");

            try
            {
                string url = Constants.NetworkURL + network;
                // Async HTTP call - matches Rust async implementation
                HttpResponseMessage httpResponse = await httpClient.GetAsync(url);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return ("", $"network discovery failed with status: {httpResponse.StatusCode}");
                }

                string response = await httpResponse.Content.ReadAsStringAsync();

                // Parse JSON response to extract NAG URL - matches Go implementation exactly
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    // Handle Go's expected response format: {"status":"success", "url":"...", "message":"OK"}
                    if (doc.RootElement.TryGetProperty("status", out JsonElement statusElement))
                    {
                        string status = statusElement.GetString() ?? "";

                        if (status == "error")
                        {
                            string message = "";
                            if (doc.RootElement.TryGetProperty("message", out JsonElement messageElement))
                            {
                                message = messageElement.GetString() ?? "";
                            }
                            return ("", $"failed to get valid NAG URL from response: {message}");
                        }

                        if (status == "success" && doc.RootElement.TryGetProperty("url", out JsonElement urlElement))
                        {
                            string? nagUrl = urlElement.GetString();
                            if (!string.IsNullOrEmpty(nagUrl))
                                return (nagUrl, null);
                        }
                    }

                    // Try alternative format for backward compatibility
                    if (doc.RootElement.TryGetProperty("Result", out JsonElement resultElement) &&
                        resultElement.GetInt32() == 200 &&
                        doc.RootElement.TryGetProperty("Response", out JsonElement responseElement))
                    {
                        // Handle both "nagurl" and "url" field names
                        JsonElement urlElementAlt;
                        bool foundUrl = responseElement.TryGetProperty("nagurl", out urlElementAlt) ||
                                       responseElement.TryGetProperty("url", out urlElementAlt);

                        if (foundUrl)
                        {
                            string? nagUrl = urlElementAlt.GetString();
                            if (!string.IsNullOrEmpty(nagUrl))
                                return (nagUrl, null);
                        }
                    }
                }

                return ("", "failed to get valid NAG URL from response");
            }
            catch (JsonException ex)
            {
                return ("", $"failed to unmarshal NAG response: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                return ("", $"failed to fetch NAG URL: {ex.Message}");
            }
        }

    }
}