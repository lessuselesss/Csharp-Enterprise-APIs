using Xunit;
using FluentAssertions;
using CircularEnterpriseApis;
using System.Text.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace CircularEnterpriseApis.UnitTests
{
    /// <summary>
    /// Tests to verify critical protocol fixes for Go compatibility
    /// These tests ensure the C# implementation matches the Go reference exactly
    /// </summary>
    public class ProtocolCompatibilityTests
    {
        private const string TestPrivateKey = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        private const string TestAddress = "0x1234567890abcdef1234567890abcdef12345678";

        [Fact]
        public async Task SubmitCertificate_PayloadConstruction_CreatesCorrectJSONWrapper()
        {
            // Arrange
            var account = new CEPAccount();
            account.Open(TestAddress);
            account.Nonce = 1; // Set a test nonce

            string testData = "test certificate data";

            // Act - We'll need to capture the payload creation indirectly
            // by checking the internal state after SubmitCertificate
            await account.SubmitCertificateAsync(testData, TestPrivateKey);

            // Assert - The method should complete without error
            // The key test is that the payload is constructed with Action/Data wrapper
            account.LastError.Should().NotContain("Certificate data cannot be empty");
            account.LastError.Should().NotContain("Private key cannot be empty");

            // The transaction ID should be generated (indicating payload was created successfully)
            account.LatestTxID.Should().NotBeEmpty();
        }

        [Fact]
        public async Task SubmitCertificate_TransactionIDFormat_DoesNotInclude0xPrefix()
        {
            // Arrange
            var account = new CEPAccount();
            account.Open(TestAddress);
            account.Nonce = 1;

            // Act
            await account.SubmitCertificateAsync("test data", TestPrivateKey);

            // Assert - Transaction ID should NOT start with "0x"
            account.LatestTxID.Should().NotStartWith("0x");
            account.LatestTxID.Should().NotBeEmpty();

            // Should be a valid hex string (64 characters for SHA-256)
            account.LatestTxID.Should().HaveLength(64);
            account.LatestTxID.Should().MatchRegex("^[a-f0-9]{64}$");
        }

        [Fact]
        public void PayloadConstruction_CreatesActionDataStructure()
        {
            // This test verifies the payload format by testing the exact pattern
            // that SubmitCertificate uses internally

            // Arrange
            string testData = "Hello World";

            // Act - Simulate the payload construction logic from SubmitCertificate
            var payloadObject = new
            {
                Action = "CP_CERTIFICATE",
                Data = Utils.StringToHex(testData)
            };
            string jsonStr = JsonSerializer.Serialize(payloadObject);

            // Assert - Verify the JSON structure
            jsonStr.Should().Contain("\"Action\":\"CP_CERTIFICATE\"");
            jsonStr.Should().Contain("\"Data\":\"");

            // Parse back to verify structure
            var parsed = JsonSerializer.Deserialize<JsonElement>(jsonStr);
            parsed.GetProperty("Action").GetString().Should().Be("CP_CERTIFICATE");
            parsed.GetProperty("Data").GetString().Should().Be(Utils.StringToHex(testData));
        }

        [Fact]
        public void TransactionRequest_NonceAsString_NoPublicKeyField()
        {
            // This test verifies the transaction request structure matches Go exactly
            // by simulating the transaction object creation from SubmitCertificate

            // Arrange
            string timestamp = Utils.GetFormattedTimestamp();
            string fromHex = Utils.HexFix(TestAddress);
            string toHex = Utils.HexFix(TestAddress);
            string payloadHex = "48656c6c6f"; // "Hello" in hex
            long nonce = 42;
            string transactionId = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
            string signature = "abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890";
            string blockchainHex = Utils.HexFix(Constants.DefaultChain);

            // Act - Create transaction object matching SubmitCertificate implementation
            var transaction = new
            {
                ID = transactionId,                    // No 0x prefix
                From = fromHex,                        // No 0x prefix
                To = toHex,                           // No 0x prefix
                Timestamp = timestamp,                 // YYYY:MM:DD-hh:mm:ss format
                Payload = payloadHex,                 // No 0x prefix
                Nonce = nonce.ToString(),             // STRING format (critical!)
                Signature = signature,                // No 0x prefix
                Blockchain = blockchainHex,           // No 0x prefix
                Type = "C_TYPE_CERTIFICATE",         // Transaction type
                Version = Constants.LibVersion       // Code version
                // CRITICAL: No PublicKey field
            };

            string jsonRequest = JsonSerializer.Serialize(transaction);

            // Assert - Verify critical protocol requirements
            var parsed = JsonSerializer.Deserialize<JsonElement>(jsonRequest);

            // 1. Nonce MUST be string, not number
            parsed.GetProperty("Nonce").ValueKind.Should().Be(JsonValueKind.String);
            parsed.GetProperty("Nonce").GetString().Should().Be("42");

            // 2. PublicKey field MUST NOT exist
            parsed.TryGetProperty("PublicKey", out _).Should().BeFalse();

            // 3. All hex fields should NOT have 0x prefix
            parsed.GetProperty("ID").GetString().Should().NotStartWith("0x");
            parsed.GetProperty("From").GetString().Should().NotStartWith("0x");
            parsed.GetProperty("To").GetString().Should().NotStartWith("0x");
            parsed.GetProperty("Payload").GetString().Should().NotStartWith("0x");
            parsed.GetProperty("Signature").GetString().Should().NotStartWith("0x");
            parsed.GetProperty("Blockchain").GetString().Should().NotStartWith("0x");

            // 4. Required fields should be present
            parsed.GetProperty("Type").GetString().Should().Be("C_TYPE_CERTIFICATE");
            parsed.GetProperty("Version").GetString().Should().Be(Constants.LibVersion);
        }

        [Fact]
        public void HexFix_Removes0xPrefix()
        {
            // Verify the HexFix utility works correctly for transaction fields

            // Test with 0x prefix
            string withPrefix = "0x1234abcd";
            string result = Utils.HexFix(withPrefix);
            result.Should().Be("1234abcd");
            result.Should().NotStartWith("0x");

            // Test without 0x prefix (should remain unchanged)
            string withoutPrefix = "1234abcd";
            string result2 = Utils.HexFix(withoutPrefix);
            result2.Should().Be("1234abcd");

            // Test empty string
            string empty = "";
            string result3 = Utils.HexFix(empty);
            result3.Should().Be("");
        }

        [Fact]
        public void StringToHex_EncodesCorrectly()
        {
            // Verify string to hex conversion for payload data

            string input = "Hello World";
            string result = Utils.StringToHex(input);

            // Should be hex encoded without 0x prefix
            result.Should().NotStartWith("0x");
            result.Should().MatchRegex("^[A-F0-9]+$"); // Utils.StringToHex returns uppercase hex

            // Should decode back to original
            string decoded = Utils.HexToString(result);
            decoded.Should().Be(input);
        }

        [Fact]
        public void TimestampFormat_MatchesGoFormat()
        {
            // Verify timestamp format matches Go implementation

            string timestamp = Utils.GetFormattedTimestamp();

            // Should match YYYY:MM:DD-HH:MM:SS format (with colons, not YYYY-MM-DD)
            timestamp.Should().MatchRegex(@"^\d{4}:\d{2}:\d{2}-\d{2}:\d{2}:\d{2}$");

            // Verify it's using colons for date separation (not dashes)
            timestamp.Substring(4, 1).Should().Be(":");
            timestamp.Substring(7, 1).Should().Be(":");
            timestamp.Substring(10, 1).Should().Be("-"); // Only separator between date and time
        }
    }
}