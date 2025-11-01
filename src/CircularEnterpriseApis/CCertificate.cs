using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CircularEnterpriseApis
{
    /// <summary>
    /// Represents a certificate for blockchain submission.
    /// Certificates are used to permanently store and verify data on the Circular Protocol blockchain.
    /// </summary>
    /// <example>
    /// Basic usage:
    /// <code>
    /// var cert = new CCertificate();
    /// cert.SetData("Document hash: abc123def456");
    ///
    /// // Submit to blockchain
    /// await account.SubmitCertificateAsync(cert.GetJSONCertificate(), privateKey);
    ///
    /// // Later, retrieve and verify
    /// string originalData = cert.GetData();
    /// Console.WriteLine($"Certified data: {originalData}");
    /// </code>
    /// </example>
    public class CCertificate
    {
        /// <summary>
        /// The certificate data payload stored in hex format.
        /// Use <see cref="SetData(string)"/> and <see cref="GetData"/> to work with the data as strings.
        /// </summary>
        [JsonPropertyName("data")]
        public string Data { get; set; } = "";

        /// <summary>
        /// Links this certificate to a previous transaction, creating a chain of certificates.
        /// Optional - only needed when building certificate chains.
        /// </summary>
        [JsonPropertyName("previousTxID")]
        public string PreviousTxID { get; set; } = "";

        /// <summary>
        /// Links this certificate to a previous block, creating a chain of certificates.
        /// Optional - only needed when building certificate chains.
        /// </summary>
        [JsonPropertyName("previousBlock")]
        public string PreviousBlock { get; set; } = "";

        /// <summary>
        /// The version identifier for the certificate format.
        /// Automatically set to the current library version.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = Constants.LibVersion;

        /// <summary>
        /// Creates a new certificate instance ready for data certification.
        /// </summary>
        public CCertificate()
        {
            // Initialize with default values (already set by property initializers)
        }

        /// <summary>
        /// Sets the data to be certified on the blockchain.
        /// The data is automatically converted to hex format for storage.
        /// </summary>
        /// <param name="data">The data to certify (can be any string: document hash, JSON, plain text, etc.)</param>
        /// <example>
        /// <code>
        /// var cert = new CCertificate();
        /// cert.SetData("Purchase Order #12345: $1,500.00");
        /// // Data is now stored in hex format internally
        /// </code>
        /// </example>
        public void SetData(string data)
        {
            Data = CircularEnterpriseApis.StringToHex(data ?? "");
        }

        /// <summary>
        /// Retrieves the original data that was certified.
        /// Automatically converts from hex format back to the original string.
        /// </summary>
        /// <returns>The original data that was set with <see cref="SetData(string)"/></returns>
        /// <example>
        /// <code>
        /// var cert = new CCertificate();
        /// cert.SetData("Hello World");
        ///
        /// string original = cert.GetData();
        /// Console.WriteLine(original); // Output: "Hello World"
        /// </code>
        /// </example>
        public string GetData()
        {
            return CircularEnterpriseApis.HexToString(Data);
        }

        /// <summary>
        /// Converts the certificate to JSON format for blockchain submission.
        /// This is the format required by <see cref="CEPAccount.SubmitCertificateAsync(string, string)"/>.
        /// </summary>
        /// <returns>JSON string representation of the certificate, or empty string on error</returns>
        /// <example>
        /// <code>
        /// var cert = new CCertificate();
        /// cert.SetData("Important document hash");
        ///
        /// string json = cert.GetJSONCertificate();
        /// await account.SubmitCertificateAsync(json, privateKey);
        /// </code>
        /// </example>
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
        /// Calculates the size of the certificate in bytes when serialized to JSON.
        /// Useful for checking certificate size limits before submission.
        /// </summary>
        /// <returns>The size in bytes, or 0 on error</returns>
        /// <example>
        /// <code>
        /// var cert = new CCertificate();
        /// cert.SetData("Some data to certify");
        ///
        /// int size = cert.GetCertificateSize();
        /// Console.WriteLine($"Certificate size: {size} bytes");
        /// </code>
        /// </example>
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
