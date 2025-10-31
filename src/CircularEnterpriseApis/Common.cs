using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Constants used throughout the Circular Enterprise APIs.
    /// These values are used for blockchain identification, network discovery, and versioning.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The current version of the Circular Enterprise APIs library.
        /// This version is sent with all blockchain requests for compatibility tracking.
        /// </summary>
        public const string LibVersion = "1.0.13";

        /// <summary>
        /// The default blockchain identifier for Circular Protocol operations.
        /// This is the hex-encoded blockchain ID used unless a different blockchain is specified.
        /// </summary>
        public const string DefaultChain = "0x8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2";

        /// <summary>
        /// The default Network Access Gateway (NAG) URL for blockchain communication.
        /// This URL serves as the entry point for blockchain operations.
        /// </summary>
        public const string DefaultNAG = "https://nag.circularlabs.io/NAG.php?cep=";

        /// <summary>
        /// The base URL for network discovery and NAG resolution.
        /// Used to dynamically discover the correct NAG URL for different networks (testnet, mainnet, devnet).
        /// </summary>
        public static string NetworkURL = "https://circularlabs.io/network/getNAG?network=";
    }

    /// <summary>
    /// Internal common functions for network discovery and configuration.
    /// Most functionality is exposed through the <see cref="CircularEnterpriseApis"/> class.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Deprecated: Use Constants.LibVersion instead.
        /// </summary>
        [Obsolete("Use Constants.LibVersion instead")]
        public const string LibVersion = Constants.LibVersion;

        /// <summary>
        /// Deprecated: Use Constants.DefaultChain instead.
        /// </summary>
        [Obsolete("Use Constants.DefaultChain instead")]
        public const string DefaultChain = Constants.DefaultChain;

        /// <summary>
        /// Deprecated: Use Constants.DefaultNAG instead.
        /// </summary>
        [Obsolete("Use Constants.DefaultNAG instead")]
        public const string DefaultNAG = Constants.DefaultNAG;

        /// <summary>
        /// Deprecated: Use Constants.NetworkURL instead.
        /// </summary>
        [Obsolete("Use Constants.NetworkURL instead")]
        public static string NetworkURL = Constants.NetworkURL;

        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>
        /// Discovers the Network Access Gateway (NAG) URL for a specified blockchain network.
        /// This function queries the Circular Protocol network discovery service.
        /// </summary>
        /// <param name="network">The network identifier ("testnet", "mainnet", or "devnet")</param>
        /// <returns>
        /// A tuple containing the NAG URL and error message.
        /// On success: (url: "https://...", error: null)
        /// On failure: (url: "", error: "descriptive error message")
        /// </returns>
        public static async Task<(string url, string? error)> GetNAGAsync(string network)
        {
            if (string.IsNullOrEmpty(network))
                return ("", "network identifier cannot be empty");

            try
            {
                string url = Constants.NetworkURL + network;
                HttpResponseMessage httpResponse = await httpClient.GetAsync(url);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return ("", $"network discovery failed with status: {httpResponse.StatusCode}");
                }

                string response = await httpResponse.Content.ReadAsStringAsync();

                // Parse JSON response to extract NAG URL
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    // Handle standard response format: {"status":"success", "url":"...", "message":"OK"}
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
