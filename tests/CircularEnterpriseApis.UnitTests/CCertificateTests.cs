using Xunit;
using FluentAssertions;
using CircularEnterpriseApis;
using System.Text.Json;

namespace CircularEnterpriseApis.UnitTests
{
    /// <summary>
    /// Unit tests for CCertificate class
    /// Maps to Go: pkg/certificate_test.go
    /// </summary>
    public class CCertificateTests
    {
        [Fact]
        public void NewCCertificate_ReturnsValidInstance()
        {
            var cert = new CCertificate();

            cert.Should().NotBeNull();
            cert.Data.Should().Be("");
            cert.PreviousTxID.Should().Be("");
            cert.PreviousBlock.Should().Be("");
            cert.Version.Should().Be(Common.LibVersion);
        }

        [Fact]
        public void SetData_GetData_WorksCorrectly()
        {
            var cert = new CCertificate();
            string testData = "test certificate data";

            cert.SetData(testData);
            cert.GetData().Should().Be(testData); // Should convert back from hex
            cert.Data.Should().NotBe(testData); // Should be stored as hex
            cert.Data.Should().Be(Utils.StringToHex(testData)); // Verify hex conversion
        }

        [Fact]
        public void SetData_NullData_SetsEmpty()
        {
            var cert = new CCertificate();

            cert.SetData(null!);
            cert.GetData().Should().Be("");
        }

        [Fact]
        public void PreviousTxID_Property_WorksCorrectly()
        {
            var cert = new CCertificate();
            string testTxID = "abc123def456";

            cert.PreviousTxID = testTxID;
            cert.PreviousTxID.Should().Be(testTxID);
        }

        [Fact]
        public void PreviousBlock_Property_WorksCorrectly()
        {
            var cert = new CCertificate();
            string testBlock = "block123";

            cert.PreviousBlock = testBlock;
            cert.PreviousBlock.Should().Be(testBlock);
        }

        [Fact]
        public void GetJSONCertificate_ReturnsValidJSON()
        {
            var cert = new CCertificate();
            cert.SetData("test data");
            cert.PreviousTxID = "tx123";
            cert.PreviousBlock = "block456";

            string json = cert.GetJSONCertificate();

            json.Should().NotBeEmpty();

            // Should be valid JSON
            var parsed = JsonDocument.Parse(json);
            parsed.Should().NotBeNull();

            // Should contain expected properties with Go-compatible field names
            json.Should().Contain("\"data\":"); // lowercase 'data' like Go
            json.Should().Contain("\"previousTxID\":\"tx123\"");
            json.Should().Contain("\"previousBlock\":\"block456\"");
            json.Should().Contain($"\"version\":\"{Common.LibVersion}\""); // lowercase 'version' like Go
        }

        [Fact]
        public void GetCertificateSize_ReturnsCorrectSize()
        {
            var cert = new CCertificate();
            cert.SetData("test");

            int size = cert.GetCertificateSize();

            // Should be positive
            size.Should().BeGreaterThan(0);

            // Should match JSON byte count
            string json = cert.GetJSONCertificate();
            int expectedSize = System.Text.Encoding.UTF8.GetByteCount(json);
            size.Should().Be(expectedSize);
        }

        [Fact]
        public void FromJSON_ValidJSON_ReturnsCorrectCertificate()
        {
            string json = "{\"data\":\"746573742064617461\",\"previousTxID\":\"tx123\",\"previousBlock\":\"block456\",\"version\":\"1.0.13\"}"; // 'test data' as hex

            var cert = CCertificate.FromJSON(json);

            cert.Should().NotBeNull();
            cert!.GetData().Should().Be("test data"); // Use GetData() which converts from hex
            cert.Data.Should().Be("746573742064617461"); // Raw Data field should be hex
            cert.PreviousTxID.Should().Be("tx123");
            cert.PreviousBlock.Should().Be("block456");
            cert.Version.Should().Be("1.0.13");
        }

        [Fact]
        public void FromJSON_InvalidJSON_ReturnsNull()
        {
            string invalidJson = "invalid json";

            var cert = CCertificate.FromJSON(invalidJson);

            cert.Should().BeNull();
        }

        [Fact]
        public void FromJSON_EmptyString_ReturnsNull()
        {
            var cert = CCertificate.FromJSON("");

            cert.Should().BeNull();
        }

        [Fact]
        public void IsValid_WithData_ReturnsTrue()
        {
            var cert = new CCertificate();
            cert.SetData("some data");

            cert.IsValid().Should().BeTrue();
        }

        [Fact]
        public void IsValid_WithoutData_ReturnsFalse()
        {
            var cert = new CCertificate();

            cert.IsValid().Should().BeFalse();
        }

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            var original = new CCertificate();
            original.SetData("original data");
            original.PreviousTxID = "original tx";

            var clone = original.Clone();

            clone.Should().NotBeSameAs(original);
            clone.Data.Should().Be(original.Data);
            clone.PreviousTxID.Should().Be(original.PreviousTxID);

            // Modify clone - should not affect original
            clone.SetData("modified data");
            original.GetData().Should().Be("original data"); // Check using GetData() for proper comparison
        }

        [Fact]
        public void JSON_Serialization_RoundTrip()
        {
            var original = new CCertificate();
            original.SetData("test data for round trip");
            original.PreviousTxID = "tx12345";
            original.PreviousBlock = "block67890";

            string json = original.GetJSONCertificate();
            var restored = CCertificate.FromJSON(json);

            restored.Should().NotBeNull();
            restored!.Data.Should().Be(original.Data);
            restored.PreviousTxID.Should().Be(original.PreviousTxID);
            restored.PreviousBlock.Should().Be(original.PreviousBlock);
            restored.Version.Should().Be(original.Version);
        }

        [Fact]
        public void Go_Compatibility_HexConversion()
        {
            // Test that matches EXACTLY how Go implementation works
            var cert = new CCertificate();
            string originalText = "Hello World";

            // Go: c.Data = utils.StringToHex(data)
            cert.SetData(originalText);

            // Verify Data field contains hex (like Go)
            cert.Data.Should().Be("48656C6C6F20576F726C64"); // "Hello World" in hex (uppercase)

            // Go: return utils.HexToString(c.Data)
            string retrievedText = cert.GetData();
            retrievedText.Should().Be(originalText);
        }

        [Fact]
        public void Go_Compatibility_JSONStructure()
        {
            // Verify JSON structure matches Go exactly
            var cert = new CCertificate();
            cert.SetData("test");
            cert.PreviousTxID = "prev123";
            cert.PreviousBlock = "block456";

            string json = cert.GetJSONCertificate();

            // Parse to verify structure
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Must have Go-compatible field names
            root.TryGetProperty("data", out _).Should().BeTrue();
            root.TryGetProperty("previousTxID", out _).Should().BeTrue();
            root.TryGetProperty("previousBlock", out _).Should().BeTrue();
            root.TryGetProperty("version", out _).Should().BeTrue();

            // Data should be hex-encoded
            root.GetProperty("data").GetString().Should().Be("74657374"); // "test" in hex
        }
    }
}