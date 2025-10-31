using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Certificate data structure and operations
    /// Maps to Go: CCertificate struct in pkg/certificate.go
    /// Must maintain exact API surface for compatibility
    /// </summary>
    public class CCertificate
    {
        /// <summary>
        /// Certificate data payload
        /// Maps to Go: Data string
        /// </summary>
        [JsonPropertyName("data")]
        public string Data { get; set; } = "";

        /// <summary>
        /// Previous transaction ID for linking certificates
        /// Maps to Go: PreviousTxID string
        /// </summary>
        [JsonPropertyName("previousTxID")]
        public string PreviousTxID { get; set; } = "";

        /// <summary>
        /// Previous block for linking certificates
        /// Maps to Go: PreviousBlock string
        /// </summary>
        [JsonPropertyName("previousBlock")]
        public string PreviousBlock { get; set; } = "";

        /// <summary>
        /// Version identifier for certificate format
        /// Maps to Go: Version string
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = Constants.LibVersion;

        /// <summary>
        /// Creates a new CCertificate instance
        /// Matches Node.js/PHP/Java: new CCertificate()
        /// </summary>
        public CCertificate()
        {
            // Initialize with default values (already set by property initializers)
        }

        /// <summary>
        /// Sets the certificate data
        /// Matches Node.js/Java: setData(data)
        /// Converts string to hex automatically
        /// </summary>
        public void SetData(string data)
        {
            Data = CircularEnterpriseApis.StringToHex(data ?? "");
        }

        /// <summary>
        /// Gets the certificate data
        /// Matches Node.js/Java: getData()
        /// Converts hex back to original string
        /// </summary>
        public string GetData()
        {
            return CircularEnterpriseApis.HexToString(Data);
        }

        /// <summary>
        /// Converts certificate to JSON string format
        /// Matches Node.js/Java: getJsonCertificate()
        /// </summary>
        public string GetJSONCertificate()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null, // Keep exact property names
                    WriteIndented = false
                };

                return JsonSerializer.Serialize(this, options);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the size of the certificate in bytes
        /// Matches Node.js/Java: getCertificateSize()
        /// </summary>
        public int GetCertificateSize()
        {
            try
            {
                string json = GetJSONCertificate();
                return System.Text.Encoding.UTF8.GetByteCount(json);
            }
            catch
            {
                return 0;
            }
        }

        #region Cross-Language API Compatibility Methods

        /// <summary>
        /// Sets the previous transaction ID
        /// Matches Rust: set_previous_tx_id(&mut self, tx_id: &str)
        /// Provides method-based API for cross-language compatibility
        /// </summary>
        /// <param name="txId">Previous transaction ID</param>
        public void SetPreviousTxId(string txId)
        {
            PreviousTxID = txId;
        }

        /// <summary>
        /// Gets the previous transaction ID
        /// Matches Rust: get_previous_tx_id(&self) -> String
        /// Provides method-based API for cross-language compatibility
        /// </summary>
        /// <returns>Previous transaction ID</returns>
        public string GetPreviousTxId()
        {
            return PreviousTxID;
        }

        /// <summary>
        /// Sets the previous block identifier
        /// Matches Rust: set_previous_block(&mut self, block: &str)
        /// Provides method-based API for cross-language compatibility
        /// </summary>
        /// <param name="block">Previous block identifier</param>
        public void SetPreviousBlock(string block)
        {
            PreviousBlock = block;
        }

        /// <summary>
        /// Gets the previous block identifier
        /// Matches Rust: get_previous_block(&self) -> String
        /// Provides method-based API for cross-language compatibility
        /// </summary>
        /// <returns>Previous block identifier</returns>
        public string GetPreviousBlock()
        {
            return PreviousBlock;
        }

        #endregion
    }
}