using System;
using System.Collections.Generic;
using Xunit;
using CircularEnterpriseApis;

namespace CircularEnterpriseApis.UnitTests
{
    /// <summary>
    /// Tests to validate exact API compatibility with Go reference implementation
    /// Ensures C# provides identical developer experience to Go
    /// </summary>
    public class ApiCompatibilityTests
    {
        [Fact]
        public void Utils_StringToHex_ReturnsUppercaseHex_MatchingGo()
        {
            // Go reference: StringToHex("Hello") -> "48656C6C6F"
            string result = Utils.StringToHex("Hello");
            Assert.Equal("48656C6C6F", result);
        }

        [Fact]
        public void Utils_StringToHex_EmptyString_ReturnsEmpty()
        {
            // Go reference: StringToHex("") -> ""
            string result = Utils.StringToHex("");
            Assert.Equal("", result);
        }

        [Fact]
        public void Utils_HexToString_HandlesUppercaseHex()
        {
            // Go reference: HexToString("48656C6C6F") -> "Hello"
            string result = Utils.HexToString("48656C6C6F");
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void Utils_HexToString_HandlesLowercaseHex()
        {
            // Go reference: HexToString("48656c6c6f") -> "Hello"
            string result = Utils.HexToString("48656c6c6f");
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void Utils_HexFix_RemovesPrefixAndPads()
        {
            // Go reference: HexFix("0xA") -> "0a"
            string result = Utils.HexFix("0xA");
            Assert.Equal("0a", result);
        }

        [Fact]
        public void Utils_PadNumber_FormatsSingleDigit()
        {
            // Go reference: PadNumber(5) -> "05"
            string result = Utils.PadNumber(5);
            Assert.Equal("05", result);
        }

        [Fact]
        public void Constants_AccessibleDirectly()
        {
            // Test that constants are accessible at package level like Go
            Assert.Equal("1.0.13", Constants.LibVersion);
            Assert.Equal("0x8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2", Constants.DefaultChain);
            Assert.Equal("https://nag.circularlabs.io/NAG.php?cep=", Constants.DefaultNAG);
        }

        [Fact]
        public void CEPAccount_NewCEPAccount_FactoryMethod()
        {
            // Go reference: account := circular_enterprise_apis.NewCEPAccount()
            var account = new CEPAccount();

            Assert.NotNull(account);
            Assert.Equal(Constants.LibVersion, account.CodeVersion);
            Assert.Equal(Constants.DefaultChain, account.Blockchain);
            Assert.Equal(Constants.DefaultNAG, account.NAGURL);
            Assert.Equal(0, account.Nonce);
        }

        [Fact]
        public void CEPAccount_Open_ValidatesAddress()
        {
            // Go reference: account.Open("") -> false, sets LastError
            var account = new CEPAccount();

            bool result = account.Open("");
            Assert.False(result);
            Assert.Equal("invalid address format", account.LastError);
        }

        [Fact]
        public void CEPAccount_Close_ClearsAllData()
        {
            // Go reference: account.Close() clears all fields
            var account = new CEPAccount();
            account.Address = "test";
            account.PublicKey = "test";
            account.LatestTxID = "test";
            account.Nonce = 123;

            account.Close();

            Assert.Equal("", account.Address);
            Assert.Equal("", account.PublicKey);
            Assert.Equal("", account.LatestTxID);
            Assert.Equal(0, account.Nonce);
        }

        [Fact]
        public void CCertificate_NewCCertificate_FactoryMethod()
        {
            // Go reference: cert := circular_enterprise_apis.NewCCertificate()
            var cert = new CCertificate();

            Assert.NotNull(cert);
            Assert.Equal("", cert.Data);
            Assert.Equal("", cert.PreviousTxID);
            Assert.Equal("", cert.PreviousBlock);
            Assert.Equal(Constants.LibVersion, cert.Version);
        }

        [Fact]
        public void CCertificate_SetData_ConvertsToHex()
        {
            // Go reference: cert.SetData("test") sets Data to hex
            var cert = new CCertificate();
            cert.SetData("test");

            Assert.Equal("74657374", cert.Data); // "test" in uppercase hex
        }

        [Fact]
        public void CCertificate_GetData_ConvertsFromHex()
        {
            // Go reference: cert.GetData() converts hex back to string
            var cert = new CCertificate();
            cert.Data = "74657374"; // "test" in hex

            string result = cert.GetData();
            Assert.Equal("test", result);
        }

        [Fact]
        public void CCertificate_GetJSONCertificate_ProducesValidJson()
        {
            // Go reference: cert.GetJSONCertificate() returns JSON string
            var cert = new CCertificate();
            cert.SetData("test");
            cert.PreviousTxID = "0x123");
            cert.PreviousBlock = "0x456");

            string json = cert.GetJSONCertificate();

            Assert.Contains("\"data\":\"74657374\"", json);
            Assert.Contains("\"previousTxID\":\"0x123\"", json);
            Assert.Contains("\"previousBlock\":\"0x456\"", json);
            Assert.Contains($"\"version\":\"{Constants.LibVersion}\"", json);
        }

        [Fact]
        public void CCertificate_GetCertificateSize_ReturnsJsonByteLength()
        {
            // Go reference: cert.GetCertificateSize() returns byte length
            var cert = new CCertificate();
            cert.SetData("test");

            int size = cert.GetCertificateSize();
            string json = cert.GetJSONCertificate();
            int expectedSize = System.Text.Encoding.UTF8.GetByteCount(json);

            Assert.Equal(expectedSize, size);
        }

        [Theory]
        [InlineData("testnet")]
        [InlineData("mainnet")]
        [InlineData("devnet")]
        public void CEPAccount_SetNetwork_HandlesValidNetworks(string network)
        {
            // Go reference: account.SetNetwork("testnet") sets NetworkNode
            var account = new CEPAccount();

            string initialNAGURL = account.NAGURL;

            // Network calls succeed in this environment
            string result = account.SetNetwork(network);

            // On success: returns NAG URL, clears LastError, sets NetworkNode
            Assert.NotEqual("", result);
            Assert.StartsWith("https://", result);
            Assert.Equal("", account.LastError);
            Assert.Equal(network, account.NetworkNode);
            Assert.Equal(result, account.NAGURL);
        }

        [Fact]
        public void CEPAccount_GetTransaction_ValidatesParameters()
        {
            // Go reference: account.GetTransaction("", "txid") -> error
            var account = new CEPAccount();

            var result = account.GetTransaction("", "txid");
            Assert.Null(result);
            Assert.Equal("blockID cannot be empty", account.LastError);

            // Reset and test transaction ID validation
            account.LastError = "";
            result = account.GetTransaction("123", "");
            Assert.Null(result);
            Assert.Equal("transactionID cannot be empty", account.LastError);
        }

        [Fact]
        public void Utils_GetFormattedTimestamp_MatchesGoFormat()
        {
            // Go reference: GetFormattedTimestamp() -> "YYYY:MM:DD-HH:MM:SS"
            string timestamp = Utils.GetFormattedTimestamp();

            // Should match format: 2024:01:02-15:04:05
            Assert.Matches(@"^\d{4}:\d{2}:\d{2}-\d{2}:\d{2}:\d{2}$", timestamp);
        }

        [Fact]
        public void ErrorHandling_MatchesGoSemantics()
        {
            // Go returns errors explicitly, C# should set LastError consistently
            var account = new CEPAccount();

            // Test empty network (should set LastError)
            try
            {
                Common.GetNAG("");
                Assert.True(false, "Should have thrown exception");
            }
            catch (ArgumentException ex)
            {
                Assert.Contains("network identifier cannot be empty", ex.Message);
            }
        }
    }
}