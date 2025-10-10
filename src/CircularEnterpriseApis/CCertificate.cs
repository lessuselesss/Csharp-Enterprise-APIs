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
        /// Factory method matching Go: NewCCertificate() *CCertificate
        /// Creates a new certificate instance with default values
        /// </summary>
        public static CCertificate NewCCertificate()
        {
            return new CCertificate
            {
                Data = "",
                PreviousTxID = "",
                PreviousBlock = "",
                Version = Constants.LibVersion
            };
        }

        /// <summary>
        /// Sets the certificate data
        /// Maps to Go: func (c *CCertificate) SetData(data string)
        /// EXACTLY matches Go behavior: converts string to hex automatically
        /// </summary>
        public void SetData(string data)
        {
            Data = CircularEnterpriseApis.StringToHex(data ?? "");
        }

        /// <summary>
        /// Gets the certificate data
        /// Maps to Go: func (c *CCertificate) GetData() string
        /// EXACTLY matches Go behavior: converts hex back to original string
        /// </summary>
        public string GetData()
        {
            return CircularEnterpriseApis.HexToString(Data);
        }

        /// <summary>
        /// Sets the previous transaction ID
        /// Maps to Go: func (c *CCertificate) SetPreviousTxID(txID string)
        /// </summary>
        public void SetPreviousTxID(string txID)
        {
            PreviousTxID = txID ?? "";
        }

        /// <summary>
        /// Gets the previous transaction ID
        /// Maps to Go: func (c *CCertificate) GetPreviousTxID() string
        /// </summary>
        public string GetPreviousTxID()
        {
            return PreviousTxID;
        }

        /// <summary>
        /// Sets the previous block
        /// Maps to Go: func (c *CCertificate) SetPreviousBlock(block string)
        /// </summary>
        public void SetPreviousBlock(string block)
        {
            PreviousBlock = block ?? "";
        }

        /// <summary>
        /// Gets the previous block
        /// Maps to Go: func (c *CCertificate) GetPreviousBlock() string
        /// </summary>
        public string GetPreviousBlock()
        {
            return PreviousBlock;
        }

        /// <summary>
        /// Converts certificate to JSON string format
        /// Maps to Go: func (c *CCertificate) GetJSONCertificate() string
        /// Must produce identical JSON output to Go implementation
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
        /// Maps to Go: func (c *CCertificate) GetCertificateSize() int
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

    }
}