using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using CircularEnterpriseApis.Crypto;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Main client interface for blockchain operations
    /// Maps to Go: CEPAccount struct in pkg/account.go
    /// EXACTLY matches Go API surface for compatibility
    /// </summary>
    public class CEPAccount
    {
        #region Properties - exact same names as Go struct fields

        /// <summary>Account address (hex format) - Maps to Go: Address string</summary>
        public string Address { get; set; } = "";

        /// <summary>Public key (hex format) - Maps to Go: PublicKey string</summary>
        public string PublicKey { get; set; } = "";

        /// <summary>Account information object - Maps to Go: Info interface{}</summary>
        public object? Info { get; set; }

        /// <summary>Code version identifier - Maps to Go: CodeVersion string</summary>
        public string CodeVersion { get; set; } = Constants.LibVersion;

        /// <summary>Last error message - Maps to Go: LastError string</summary>
        public string LastError { get; set; } = "";

        /// <summary>NAG URL for network communication - Maps to Go: NAGURL string</summary>
        public string NAGURL { get; set; } = Constants.DefaultNAG;

        /// <summary>Network node identifier - Maps to Go: NetworkNode string</summary>
        public string NetworkNode { get; set; } = "";

        /// <summary>Blockchain identifier (hex format) - Maps to Go: Blockchain string</summary>
        public string Blockchain { get; set; } = Constants.DefaultChain;

        /// <summary>Latest transaction ID - Maps to Go: LatestTxID string</summary>
        public string LatestTxID { get; set; } = "";

        /// <summary>Current nonce for transaction ordering - Maps to Go: Nonce int64</summary>
        public long Nonce { get; set; }

        /// <summary>Interval in seconds for polling operations - Maps to Go: IntervalSec int</summary>
        public int IntervalSec { get; set; } = 5;

        /// <summary>Network URL for NAG discovery - Maps to Go: NetworkURL string</summary>
        public string NetworkURL { get; set; } = Constants.NetworkURL;

        #endregion

        #region Private fields

        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        #endregion

        #region Factory Method - exact same as Go

        /// <summary>
        /// Factory function that creates and initializes a new CEPAccount instance
        /// Maps to Go: func NewCEPAccount() *CEPAccount
        /// </summary>
        public static CEPAccount NewCEPAccount()
        {
            return new CEPAccount();
        }

        #endregion

        #region Public Methods - exact same signatures as Go

        /// <summary>
        /// Retrieves the last error message
        /// Maps to Go: func (a *CEPAccount) GetLastError() string
        /// </summary>
        public string GetLastError()
        {
            return LastError;
        }

        /// <summary>
        /// Initializes the CEPAccount with a specified blockchain address
        /// Maps to Go: func (a *CEPAccount) Open(address string) bool
        /// </summary>
        public bool Open(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                LastError = "invalid address format";
                return false;
            }

            Address = address;
            LastError = "";
            return true;
        }

        /// <summary>
        /// Securely clears all sensitive and operational data
        /// Maps to Go: func (a *CEPAccount) Close()
        /// </summary>
        public void Close()
        {
            Address = "";
            PublicKey = "";
            Info = null;
            LastError = "";
            NAGURL = Constants.DefaultNAG;
            NetworkNode = "";
            Blockchain = Constants.DefaultChain;
            LatestTxID = "";
            Nonce = 0;
            IntervalSec = 0;
        }

        /// <summary>
        /// Configures the CEPAccount to operate on a specific blockchain network
        /// Maps to Go: func (a *CEPAccount) SetNetwork(network string) string
        /// </summary>
        public string SetNetwork(string network)
        {
            // Use package-level GetNAG with Go-style error handling
            var (url, error) = CircularEnterpriseApis.GetNAG(network);
            if (error != null)
            {
                LastError = $"network discovery failed: {error.Message}";
                return "";
            }

            // Only set properties on SUCCESS (matches Go exactly)
            NAGURL = url;
            NetworkNode = network;
            LastError = "";

            return NAGURL;
        }

        /// <summary>
        /// Explicitly sets the blockchain identifier
        /// Maps to Go: func (a *CEPAccount) SetBlockchain(chain string)
        /// </summary>
        public void SetBlockchain(string chain)
        {
            Blockchain = chain;
        }

        /// <summary>
        /// Updates account information by retrieving the current nonce
        /// Maps to Go: func (a *CEPAccount) UpdateAccount() bool
        /// </summary>
        public bool UpdateAccount()
        {
            try
            {
                LastError = "";

                if (string.IsNullOrEmpty(Address))
                {
                    LastError = "Account not open";
                    return false;
                }

                if (string.IsNullOrEmpty(NAGURL))
                {
                    LastError = "Network not set";
                    return false;
                }

                // Build request URL - matches Go implementation exactly
                string url = NAGURL + "Circular_GetWalletNonce_";
                if (!string.IsNullOrEmpty(NetworkNode))
                {
                    url += NetworkNode;
                }

                var requestData = new
                {
                    Blockchain = CircularEnterpriseApis.HexFix(Blockchain), // Remove 0x prefix
                    Address = CircularEnterpriseApis.HexFix(Address),       // Remove 0x prefix
                    Version = CodeVersion
                };

                string jsonRequest = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Use synchronous HTTP call to match Go exactly
                HttpResponseMessage response = httpClient.PostAsync(url, content).Result;
                string responseContent = response.Content.ReadAsStringAsync().Result;

                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    if (doc.RootElement.TryGetProperty("Result", out JsonElement resultElement))
                    {
                        int resultCode = resultElement.GetInt32();

                        if (resultCode == 200 && doc.RootElement.TryGetProperty("Response", out JsonElement responseElement))
                        {
                            // Handle both "Nonce" and "nonce" field variations (Go vs other implementations)
                            JsonElement nonceElement;
                            bool foundNonce = responseElement.TryGetProperty("Nonce", out nonceElement) ||
                                            responseElement.TryGetProperty("nonce", out nonceElement);

                            if (foundNonce)
                            {
                                // Handle both int and string nonce values
                                if (nonceElement.ValueKind == JsonValueKind.Number)
                                {
                                    Nonce = nonceElement.GetInt64() + 1; // Increment like Go implementation
                                }
                                else if (nonceElement.ValueKind == JsonValueKind.String)
                                {
                                    if (long.TryParse(nonceElement.GetString(), out long nonceValue))
                                    {
                                        Nonce = nonceValue + 1; // Increment like Go implementation
                                    }
                                    else
                                    {
                                        LastError = "Invalid nonce format in response";
                                        return false;
                                    }
                                }
                                else
                                {
                                    LastError = "Unexpected nonce type in response";
                                    return false;
                                }

                                LastError = "";
                                return true;
                            }
                            else
                            {
                                LastError = "Nonce field not found in response";
                                return false;
                            }
                        }
                        else if (resultCode == 114)
                        {
                            LastError = "Rejected: Invalid Blockchain";
                            return false;
                        }
                        else if (resultCode == 115)
                        {
                            LastError = "Rejected: Insufficient balance";
                            return false;
                        }
                        else if (resultCode != 200)
                        {
                            string errorMsg = "Unknown error";
                            if (doc.RootElement.TryGetProperty("Response", out JsonElement errorElement))
                            {
                                errorMsg = errorElement.GetString() ?? errorMsg;
                            }
                            LastError = $"Server error {resultCode}: {errorMsg}";
                            return false;
                        }
                    }
                }

                LastError = "Unable to retrieve nonce from response";
                return false;
            }
            catch (Exception ex)
            {
                LastError = $"UpdateAccount failed: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Submits a certificate to the blockchain
        /// Maps to Go: func (a *CEPAccount) SubmitCertificate(pdata string, privateKeyHex string)
        /// </summary>
        public void SubmitCertificate(string pdata, string privateKeyHex)
        {
            try
            {
                LastError = "";

                if (string.IsNullOrEmpty(Address))
                {
                    LastError = "Account is not open";
                    return;
                }

                if (string.IsNullOrEmpty(pdata))
                {
                    LastError = "Certificate data cannot be empty";
                    return;
                }

                if (string.IsNullOrEmpty(privateKeyHex))
                {
                    LastError = "Private key cannot be empty";
                    return;
                }

                // Note: Manual UpdateAccount() call required before SubmitCertificate (matches Go behavior)
                // User must call UpdateAccount() explicitly to get the latest nonce

                // Prepare transaction data
                string timestamp = CircularEnterpriseApis.GetFormattedTimestamp();
                string fromHex = CircularEnterpriseApis.HexFix(Address);    // Remove 0x prefix for hash calc
                string toHex = CircularEnterpriseApis.HexFix(Address);      // Self-send for certificate

                // Create payload object with Action and Data wrapper (matches Go implementation)
                var payloadObject = new
                {
                    Action = "CP_CERTIFICATE",
                    Data = CircularEnterpriseApis.StringToHex(pdata)
                };
                string jsonStr = JsonSerializer.Serialize(payloadObject);
                string payloadHex = CircularEnterpriseApis.StringToHex(jsonStr);

                string blockchainHex = CircularEnterpriseApis.HexFix(Blockchain);

                // Calculate transaction ID (matches Go implementation - no "0x" prefix)
                string txDataForId = blockchainHex + fromHex + toHex + payloadHex + Nonce.ToString() + timestamp;
                LatestTxID = CryptoUtils.Sha256Hex(txDataForId);

                // Get public key for pre-validation
                string publicKey = CryptoUtils.GetPublicKeyFromPrivateKey(privateKeyHex);

                // Create signature
                string signature = CryptoUtils.SignMessage(privateKeyHex, LatestTxID);

                // Create transaction payload matching Go implementation exactly
                var transaction = new
                {
                    ID = LatestTxID,                    // No 0x prefix (matches Go)
                    From = fromHex,                     // No 0x prefix
                    To = toHex,                         // No 0x prefix
                    Timestamp = timestamp,              // YYYY:MM:DD-hh:mm:ss format
                    Payload = payloadHex,               // No 0x prefix
                    Nonce = Nonce.ToString(),           // STRING format (matches Go)
                    Signature = signature,              // No 0x prefix
                    Blockchain = blockchainHex,         // No 0x prefix
                    Type = "C_TYPE_CERTIFICATE",       // Transaction type
                    Version = Constants.LibVersion     // Code version
                    // REMOVED: PublicKey field (not in Go implementation)
                };

                string jsonRequest = JsonSerializer.Serialize(transaction);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                string url = NAGURL + "Circular_AddTransaction_";
                if (!string.IsNullOrEmpty(NetworkNode))
                {
                    url += NetworkNode;
                }

                // Use synchronous HTTP call to match Go exactly
                HttpResponseMessage response = httpClient.PostAsync(url, content).Result;
                string responseContent = response.Content.ReadAsStringAsync().Result;

                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    if (doc.RootElement.TryGetProperty("Result", out JsonElement resultElement))
                    {
                        int resultCode = resultElement.GetInt32();

                        if (resultCode == 200)
                        {
                            // Success - increment nonce for next transaction
                            Nonce++;
                            LastError = "";
                        }
                        else
                        {
                            // Error occurred
                            string errorMsg = "Unknown error";
                            if (doc.RootElement.TryGetProperty("Response", out JsonElement errorElement))
                            {
                                errorMsg = errorElement.GetString() ?? errorMsg;
                            }
                            LastError = $"Certificate submission failed (code {resultCode}): {errorMsg}";
                        }
                    }
                    else
                    {
                        LastError = "Invalid response format from server";
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = $"SubmitCertificate failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves a specific transaction by its block ID and transaction ID
        /// Maps to Go: func (a *CEPAccount) GetTransaction(blockID string, transactionID string) map[string]interface{}
        /// </summary>
        public Dictionary<string, object>? GetTransaction(string blockID, string transactionID)
        {
            try
            {
                if (string.IsNullOrEmpty(blockID))
                {
                    LastError = "blockID cannot be empty";
                    return null;
                }

                if (string.IsNullOrEmpty(transactionID))
                {
                    LastError = "transactionID cannot be empty";
                    return null;
                }

                // Parse blockID to long for range search (matches Go implementation exactly)
                if (!long.TryParse(blockID, out long startBlock))
                {
                    LastError = "invalid blockID format";
                    return null;
                }

                // Call internal method with single block range (matches Go: GetTransaction calls getTransactionByID)
                return GetTransactionByID(transactionID, startBlock, startBlock);
            }
            catch (Exception ex)
            {
                LastError = $"GetTransaction failed: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Internal method to retrieve transaction by ID within a block range
        /// Maps to Go: func (a *CEPAccount) getTransactionByID(transactionID string, startBlock, endBlock int64) (map[string]interface{}, error)
        /// </summary>
        private Dictionary<string, object>? GetTransactionByID(string transactionID, long startBlock, long endBlock)
        {
            try
            {
                if (string.IsNullOrEmpty(NAGURL))
                {
                    LastError = "network is not set";
                    return null;
                }

                var requestData = new
                {
                    Blockchain = CircularEnterpriseApis.HexFix(Blockchain),
                    ID = CircularEnterpriseApis.HexFix(transactionID),
                    Start = startBlock.ToString(),
                    End = endBlock.ToString(),
                    Version = CodeVersion
                };

                string jsonRequest = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                string url = NAGURL + "Circular_GetTransactionbyID_";
                if (!string.IsNullOrEmpty(NetworkNode))
                {
                    url += NetworkNode;
                }

                // Use synchronous HTTP call to match Go exactly
                HttpResponseMessage response = httpClient.PostAsync(url, content).Result;
                string responseContent = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    LastError = $"network request failed with status: {response.StatusCode}, body: {responseContent}";
                    return null;
                }

                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    // Return the raw transaction details as Dictionary (matches Go behavior)
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    LastError = "";
                    return result;
                }
            }
            catch (Exception ex)
            {
                LastError = $"getTransactionByID failed: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Polls for transaction outcome until found or timeout
        /// Maps to Go: func (a *CEPAccount) GetTransactionOutcome(txID string, timeoutSec int, intervalSec int) map[string]interface{}
        /// EXACTLY matches Go implementation: uses polling with getTransactionByID, checks status != "Pending"
        /// </summary>
        public Dictionary<string, object>? GetTransactionOutcome(string txID, int timeoutSec, int intervalSec)
        {
            try
            {
                if (string.IsNullOrEmpty(txID))
                {
                    LastError = "Transaction ID cannot be empty";
                    return null;
                }

                if (string.IsNullOrEmpty(NAGURL))
                {
                    LastError = "network is not set";
                    return null;
                }

                var startTime = DateTime.UtcNow;
                var timeout = TimeSpan.FromSeconds(timeoutSec);
                var interval = TimeSpan.FromSeconds(intervalSec);

                while (DateTime.UtcNow - startTime < timeout)
                {
                    // Use getTransactionByID like Go implementation (search recent blocks 0-10)
                    var data = GetTransactionByID(txID, 0, 10);
                    if (data == null)
                    {
                        // Log non-critical errors and continue polling (matches Go exactly)
                        System.Threading.Thread.Sleep(interval);
                        continue;
                    }

                    // Check Go-style response structure: Result == 200
                    if (data.TryGetValue("Result", out var resultObj))
                    {
                        // Handle both int and JsonElement result types
                        int resultCode = 0;
                        if (resultObj is JsonElement resultElement && resultElement.ValueKind == JsonValueKind.Number)
                        {
                            resultCode = resultElement.GetInt32();
                        }
                        else if (resultObj is int intResult)
                        {
                            resultCode = intResult;
                        }
                        else if (resultObj is double doubleResult)
                        {
                            resultCode = (int)doubleResult;
                        }

                        if (resultCode == 200 && data.TryGetValue("Response", out var responseObj))
                        {
                            Dictionary<string, object>? responseDict = null;

                            // Handle JsonElement response
                            if (responseObj is JsonElement responseElement)
                            {
                                responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(responseElement.GetRawText());
                            }
                            // Handle already deserialized dictionary
                            else if (responseObj is Dictionary<string, object> directDict)
                            {
                                responseDict = directDict;
                            }

                            if (responseDict != null && responseDict.TryGetValue("Status", out var statusObj))
                            {
                                string status = "";

                                // Handle JsonElement status
                                if (statusObj is JsonElement statusElement && statusElement.ValueKind == JsonValueKind.String)
                                {
                                    status = statusElement.GetString() ?? "";
                                }
                                // Handle string status
                                else if (statusObj is string statusString)
                                {
                                    status = statusString;
                                }

                                // If status is not "Pending", transaction is finalized (matches Go exactly)
                                if (status != "Pending")
                                {
                                    LastError = "";
                                    return responseDict; // Transaction finalized
                                }
                            }
                        }
                    }

                    // Wait before next poll - use synchronous delay to match Go exactly
                    System.Threading.Thread.Sleep(interval);
                }

                LastError = "timeout exceeded while waiting for transaction outcome";
                return null;
            }
            catch (Exception ex)
            {
                LastError = $"GetTransactionOutcome failed: {ex.Message}";
                return null;
            }
        }

        #endregion
    }
}