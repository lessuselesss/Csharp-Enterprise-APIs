using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CircularEnterpriseApis.Crypto;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Primary client for interacting with the Circular Protocol blockchain.
    /// Provides methods for account management, certificate submission, and transaction tracking.
    /// </summary>
    /// <example>
    /// Basic usage:
    /// <code>
    /// var account = new CEPAccount();
    /// account.Open("0xYourWalletAddress");
    /// await account.SetNetworkAsync("testnet");
    /// await account.UpdateAccountAsync();
    ///
    /// var cert = new CCertificate();
    /// cert.SetData("Your data to certify");
    /// await account.SubmitCertificateAsync(cert.GetJSONCertificate(), "your_private_key");
    /// </code>
    /// </example>
    public class CEPAccount
    {
        #region Properties

        /// <summary>
        /// The blockchain address associated with this account (hex format with optional 0x prefix).
        /// Set this by calling <see cref="Open(string)"/>.
        /// </summary>
        public string Address { get; set; } = "";

        /// <summary>
        /// The public key associated with this account (hex format).
        /// </summary>
        public string PublicKey { get; set; } = "";

        /// <summary>
        /// Additional account information returned from blockchain queries.
        /// </summary>
        public object? Info { get; set; }

        /// <summary>
        /// The library version identifier used in blockchain requests.
        /// </summary>
        public string CodeVersion { get; set; } = Constants.LibVersion;

        /// <summary>
        /// Contains the last error message if an operation failed, or null if the last operation succeeded.
        /// Always check this property after operations that return void or bool to determine if errors occurred.
        /// </summary>
        public string? LastError { get; set; } = null;

        /// <summary>
        /// The Network Access Gateway (NAG) URL used for blockchain communication.
        /// Automatically set by <see cref="SetNetworkAsync(string)"/>.
        /// </summary>
        public string NAGURL { get; set; } = Constants.DefaultNAG;

        /// <summary>
        /// The network identifier (testnet, mainnet, devnet) currently in use.
        /// Automatically set by <see cref="SetNetworkAsync(string)"/>.
        /// </summary>
        public string NetworkNode { get; set; } = "";

        /// <summary>
        /// The blockchain identifier (hex format) for the specific blockchain network.
        /// Set this using <see cref="SetBlockchain(string)"/> or use the default.
        /// </summary>
        public string Blockchain { get; set; } = Constants.DefaultChain;

        /// <summary>
        /// The transaction ID of the most recently submitted certificate.
        /// Use this to track transaction status with <see cref="GetTransactionOutcomeAsync(string, int, int)"/>.
        /// </summary>
        public string LatestTxID { get; set; } = "";

        /// <summary>
        /// The current transaction nonce for this account.
        /// Updated automatically by <see cref="UpdateAccountAsync"/> and <see cref="SubmitCertificateAsync(string, string)"/>.
        /// </summary>
        public long Nonce { get; set; }

        /// <summary>
        /// Default polling interval in seconds for transaction outcome checks. Default is 2 seconds.
        /// </summary>
        public int IntervalSec { get; set; } = 2;

        /// <summary>
        /// The base URL for network discovery and NAG resolution.
        /// </summary>
        public string NetworkURL { get; set; } = Constants.NetworkURL;

        #endregion

        #region Private fields

        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new CEPAccount instance with default configuration.
        /// After creating, call <see cref="Open(string)"/> to initialize with your blockchain address.
        /// </summary>
        public CEPAccount()
        {
            // Initialize with default values (already set by property initializers)
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the account with a blockchain address.
        /// This must be called before performing any blockchain operations.
        /// </summary>
        /// <param name="address">Your wallet address in hex format (with or without 0x prefix)</param>
        /// <returns>True if the address was successfully set, false if the address is invalid</returns>
        /// <example>
        /// <code>
        /// var account = new CEPAccount();
        /// if (!account.Open("0x1234567890abcdef..."))
        /// {
        ///     Console.WriteLine($"Error: {account.LastError}");
        /// }
        /// </code>
        /// </example>
        public bool Open(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                LastError = "invalid address format";
                return false;
            }

            Address = address;
            LastError = null;
            return true;
        }

        /// <summary>
        /// Clears all account data and resets the instance to its initial state.
        /// Use this when you're done with an account or want to start fresh.
        /// </summary>
        public void Close()
        {
            Address = "";
            PublicKey = "";
            Info = null;
            LastError = null;
            NAGURL = Constants.DefaultNAG;
            NetworkNode = "";
            Blockchain = Constants.DefaultChain;
            LatestTxID = "";
            Nonce = 0;
            IntervalSec = 0;
        }

        /// <summary>
        /// Sets the blockchain identifier for transactions.
        /// Use this to target a specific blockchain network.
        /// </summary>
        /// <param name="chain">The blockchain identifier in hex format</param>
        public void SetBlockchain(string chain)
        {
            Blockchain = chain;
        }

        /// <summary>
        /// Signs data using the provided private key.
        /// This method can be used to sign arbitrary data with your private key.
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <param name="privateKeyHex">Private key in hex format</param>
        /// <returns>Signature in hex format, or empty string on error</returns>
        /// <example>
        /// <code>
        /// var account = new CEPAccount();
        /// account.Open("your-wallet-address");
        /// string signature = account.SignData("Hello World", "your-private-key");
        /// Console.WriteLine($"Signature: {signature}");
        /// </code>
        /// </example>
        public string SignData(string data, string privateKeyHex)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    LastError = "Data cannot be empty";
                    return "";
                }

                if (string.IsNullOrEmpty(privateKeyHex))
                {
                    LastError = "Private key cannot be empty";
                    return "";
                }

                LastError = null;
                return CryptoUtils.SignMessage(privateKeyHex, data);
            }
            catch (Exception ex)
            {
                LastError = $"SignData failed: {ex.Message}";
                return "";
            }
        }

        #endregion

        #region Async Methods

        /// <summary>
        /// Configures the account to use a specific blockchain network.
        /// This discovers and sets the appropriate Network Access Gateway (NAG) URL for the network.
        /// </summary>
        /// <param name="network">Network identifier: "testnet", "mainnet", or "devnet"</param>
        /// <returns>The NAG URL on success, or empty string on failure (check <see cref="LastError"/>)</returns>
        /// <example>
        /// <code>
        /// var nagUrl = await account.SetNetworkAsync("testnet");
        /// if (string.IsNullOrEmpty(nagUrl))
        /// {
        ///     Console.WriteLine($"Network setup failed: {account.LastError}");
        /// }
        /// </code>
        /// </example>
        public async Task<string> SetNetworkAsync(string network)
        {
            // Use async GetNAG for network discovery
            var (url, error) = await CircularEnterpriseApis.GetNAGAsync(network);
            if (error != null)
            {
                LastError = $"network discovery failed: {error}";
                return "";
            }

            NAGURL = url;
            NetworkNode = network;
            LastError = null;

            return NAGURL;
        }

        /// <summary>
        /// Retrieves the current nonce from the blockchain for this account.
        /// Must be called before submitting certificates to ensure the correct transaction sequence.
        /// The nonce is automatically incremented after successful certificate submission.
        /// </summary>
        /// <returns>True if the account was updated successfully, false otherwise (check <see cref="LastError"/>)</returns>
        /// <example>
        /// <code>
        /// if (!await account.UpdateAccountAsync())
        /// {
        ///     Console.WriteLine($"Update failed: {account.LastError}");
        ///     return;
        /// }
        /// Console.WriteLine($"Current nonce: {account.Nonce}");
        /// </code>
        /// </example>
        public async Task<bool> UpdateAccountAsync()
        {
            try
            {
                LastError = null;

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

                // Build request URL
                string url = NAGURL + "Circular_GetWalletNonce_";
                if (!string.IsNullOrEmpty(NetworkNode))
                {
                    url += NetworkNode;
                }

                var requestData = new
                {
                    Blockchain = CircularEnterpriseApis.HexFix(Blockchain),
                    Address = CircularEnterpriseApis.HexFix(Address),
                    Version = CodeVersion
                };

                string jsonRequest = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    if (doc.RootElement.TryGetProperty("Result", out JsonElement resultElement))
                    {
                        int resultCode = resultElement.GetInt32();

                        if (resultCode == 200 && doc.RootElement.TryGetProperty("Response", out JsonElement responseElement))
                        {
                            JsonElement nonceElement;
                            bool foundNonce = responseElement.TryGetProperty("Nonce", out nonceElement) ||
                                            responseElement.TryGetProperty("nonce", out nonceElement);

                            if (foundNonce)
                            {
                                if (nonceElement.ValueKind == JsonValueKind.Number)
                                {
                                    Nonce = nonceElement.GetInt64() + 1;
                                }
                                else if (nonceElement.ValueKind == JsonValueKind.String)
                                {
                                    if (long.TryParse(nonceElement.GetString(), out long nonceValue))
                                    {
                                        Nonce = nonceValue + 1;
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

                                LastError = null;
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
        /// Submits a certificate to the blockchain for permanent storage.
        /// Ensure you call <see cref="UpdateAccountAsync"/> before submitting to get the latest nonce.
        /// On success, the transaction ID is stored in <see cref="LatestTxID"/>.
        /// </summary>
        /// <param name="pdata">Certificate data (typically JSON from <see cref="CCertificate.GetJSONCertificate"/>)</param>
        /// <param name="privateKeyHex">Your private key in hex format (with or without 0x prefix)</param>
        /// <returns>A Task that completes when the submission finishes (check <see cref="LastError"/> for errors)</returns>
        /// <example>
        /// <code>
        /// await account.UpdateAccountAsync();
        ///
        /// var cert = new CCertificate();
        /// cert.SetData("Document hash: abc123...");
        ///
        /// await account.SubmitCertificateAsync(
        ///     cert.GetJSONCertificate(),
        ///     "0x1234567890abcdef..."
        /// );
        ///
        /// if (!string.IsNullOrEmpty(account.LastError))
        /// {
        ///     Console.WriteLine($"Submission failed: {account.LastError}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"Certificate submitted! TX ID: {account.LatestTxID}");
        /// }
        /// </code>
        /// </example>
        public async Task SubmitCertificateAsync(string pdata, string privateKeyHex)
        {
            try
            {
                LastError = null;

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

                // Prepare transaction data
                string timestamp = CircularEnterpriseApis.GetFormattedTimestamp();
                string fromHex = CircularEnterpriseApis.HexFix(Address);
                string toHex = CircularEnterpriseApis.HexFix(Address);

                // Create payload with Action and Data wrapper
                var payloadObject = new
                {
                    Action = "CP_CERTIFICATE",
                    Data = CircularEnterpriseApis.StringToHex(pdata)
                };
                string jsonStr = JsonSerializer.Serialize(payloadObject);
                string payloadHex = CircularEnterpriseApis.StringToHex(jsonStr);

                string blockchainHex = CircularEnterpriseApis.HexFix(Blockchain);

                // Calculate transaction ID
                string txDataForId = blockchainHex + fromHex + toHex + payloadHex + Nonce.ToString() + timestamp;
                LatestTxID = CryptoUtils.Sha256Hex(txDataForId);

                // Get public key for pre-validation
                string publicKey = CryptoUtils.GetPublicKeyFromPrivateKey(privateKeyHex);

                // Create signature
                string signature = CryptoUtils.SignMessage(privateKeyHex, LatestTxID);

                // Create transaction payload
                var transaction = new
                {
                    ID = LatestTxID,
                    From = fromHex,
                    To = toHex,
                    Timestamp = timestamp,
                    Payload = payloadHex,
                    Nonce = Nonce.ToString(),
                    Signature = signature,
                    Blockchain = blockchainHex,
                    Type = "C_TYPE_CERTIFICATE",
                    Version = Constants.LibVersion
                };

                string jsonRequest = JsonSerializer.Serialize(transaction);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                string url = NAGURL + "Circular_AddTransaction_";
                if (!string.IsNullOrEmpty(NetworkNode))
                {
                    url += NetworkNode;
                }

                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    if (doc.RootElement.TryGetProperty("Result", out JsonElement resultElement))
                    {
                        int resultCode = resultElement.GetInt32();

                        if (resultCode == 200)
                        {
                            // Success - increment nonce for next transaction
                            Nonce++;
                            LastError = null;
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
        /// Retrieves details of a specific transaction from the blockchain.
        /// </summary>
        /// <param name="blockID">The block number containing the transaction</param>
        /// <param name="transactionID">The transaction ID (hash) to retrieve</param>
        /// <returns>
        /// A dictionary containing transaction details on success, or null on failure (check <see cref="LastError"/>).
        /// The dictionary structure matches the blockchain's transaction format.
        /// </returns>
        public async Task<Dictionary<string, object>?> GetTransactionAsync(string blockID, string transactionID)
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

                if (!long.TryParse(blockID, out long startBlock))
                {
                    LastError = "invalid blockID format";
                    return null;
                }

                return await GetTransactionByIDAsync(transactionID, startBlock, startBlock);
            }
            catch (Exception ex)
            {
                LastError = $"GetTransaction failed: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Retrieves a transaction by ID within a block range.
        /// This method allows searching for a transaction across multiple blocks when the exact block is unknown.
        /// </summary>
        /// <param name="transactionID">The transaction ID (hash) to search for</param>
        /// <param name="startBlock">Starting block number. If endBlock is 0, this indicates the number of blocks from the latest</param>
        /// <param name="endBlock">Ending block number. If 0, search from latest blocks backwards</param>
        /// <returns>
        /// A dictionary containing transaction details on success, or null on failure (check <see cref="LastError"/>).
        /// </returns>
        /// <example>
        /// <code>
        /// // Search in last 10 blocks from the latest block
        /// var tx = await account.GetTransactionByIDAsync(txID, 0, 10);
        ///
        /// // Search in specific block range (blocks 100 to 200)
        /// var tx = await account.GetTransactionByIDAsync(txID, 100, 200);
        /// </code>
        /// </example>
        public async Task<Dictionary<string, object>?> GetTransactionByIDAsync(string transactionID, long startBlock, long endBlock)
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

                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    LastError = $"network request failed with status: {response.StatusCode}, body: {responseContent}";
                    return null;
                }

                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    LastError = null;
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
        /// Polls the blockchain for transaction confirmation until the transaction is finalized or the timeout is reached.
        /// This is useful after submitting a certificate to wait for blockchain confirmation.
        /// </summary>
        /// <param name="txID">The transaction ID to monitor (typically from <see cref="LatestTxID"/>)</param>
        /// <param name="timeoutSec">Maximum time to wait in seconds before giving up</param>
        /// <param name="intervalSec">Time between polling attempts in seconds</param>
        /// <returns>
        /// A dictionary containing the final transaction outcome on success, or null if timeout is reached or an error occurs.
        /// Check <see cref="LastError"/> if null is returned.
        /// </returns>
        /// <example>
        /// <code>
        /// await account.SubmitCertificateAsync(certJson, privateKey);
        ///
        /// var outcome = await account.GetTransactionOutcomeAsync(
        ///     account.LatestTxID,
        ///     timeoutSec: 30,
        ///     intervalSec: 2
        /// );
        ///
        /// if (outcome != null)
        /// {
        ///     Console.WriteLine($"Transaction confirmed: {outcome["Status"]}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"Timeout or error: {account.LastError}");
        /// }
        /// </code>
        /// </example>
        public async Task<Dictionary<string, object>?> GetTransactionOutcomeAsync(string txID, int timeoutSec, int intervalSec)
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
                    var data = await GetTransactionByIDAsync(txID, 0, 10);
                    if (data == null)
                    {
                        await Task.Delay(interval);
                        continue;
                    }

                    if (data.TryGetValue("Result", out var resultObj))
                    {
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

                            if (responseObj is JsonElement responseElement)
                            {
                                responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(responseElement.GetRawText());
                            }
                            else if (responseObj is Dictionary<string, object> directDict)
                            {
                                responseDict = directDict;
                            }

                            if (responseDict != null && responseDict.TryGetValue("Status", out var statusObj))
                            {
                                string status = "";

                                if (statusObj is JsonElement statusElement && statusElement.ValueKind == JsonValueKind.String)
                                {
                                    status = statusElement.GetString() ?? "";
                                }
                                else if (statusObj is string statusString)
                                {
                                    status = statusString;
                                }

                                if (status != "Pending")
                                {
                                    LastError = null;
                                    return responseDict;
                                }
                            }
                        }
                    }

                    await Task.Delay(interval);
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
