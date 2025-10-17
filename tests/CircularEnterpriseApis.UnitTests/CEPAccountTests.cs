using Xunit;
using FluentAssertions;
using CircularEnterpriseApis;

namespace CircularEnterpriseApis.UnitTests
{
    /// <summary>
    /// Unit tests for CEPAccount class
    /// Maps to Go: pkg/account_test.go
    /// </summary>
    public class CEPAccountTests
    {
        [Fact]
        public void NewCEPAccount_ReturnsValidInstance()
        {
            var account = new CEPAccount();

            account.Should().NotBeNull();
            account.Address.Should().Be("");
            account.PublicKey.Should().Be("");
            account.Info.Should().BeNull();
            account.CodeVersion.Should().Be(Common.LibVersion);
            account.LastError.Should().Be("");
            account.NAGURL.Should().Be(Common.DefaultNAG);
            account.NetworkNode.Should().Be("");
            account.Blockchain.Should().Be(Common.DefaultChain);
            account.LatestTxID.Should().Be("");
            account.Nonce.Should().Be(0);
            account.IntervalSec.Should().Be(5);
            account.NetworkURL.Should().Be(Common.NetworkURL);
        }

        [Fact]
        public void Open_ValidAddress_ReturnsTrue()
        {
            var account = new CEPAccount();
            string testAddress = "0x1234567890abcdef";

            bool result = account.Open(testAddress);

            result.Should().BeTrue();
            account.Address.Should().Be(testAddress); // Go stores address as-is
            account.LastError.Should().Be("");
        }

        [Fact]
        public void Open_EmptyAddress_ReturnsFalse()
        {
            var account = new CEPAccount();

            bool result = account.Open("");

            result.Should().BeFalse();
            account.LastError.Should().Be("invalid address format"); // Exact Go error message
        }

        [Fact]
        public void Close_ClearsAllData()
        {
            var account = new CEPAccount();
            account.Open("0x1234567890abcdef");
            account.PublicKey = "pubkey123";
            account.Info = new { test = "data" };
            account.LatestTxID = "tx123";
            account.Nonce = 5;

            account.Close();

            account.Address.Should().Be("");
            account.PublicKey.Should().Be("");
            account.Info.Should().BeNull();
            account.LastError.Should().Be("");
            account.LatestTxID.Should().Be("");
            account.Nonce.Should().Be(0);
        }

        [Fact]
        public void SetNetwork_ValidNetwork_ReturnsNAGURL()
        {
            var account = new CEPAccount();

            string result = account.SetNetwork("testnet");

            // Should return some URL (actual network call may fail in test, but method should handle it)
            result.Should().NotBeNull();
            account.NetworkNode.Should().Be("testnet");
            // NAGURL should be set to either discovered URL or default
            account.NAGURL.Should().NotBeEmpty();
        }

        [Fact]
        public void SetNetwork_EmptyNetwork_SetsError()
        {
            var account = new CEPAccount();

            string result = account.SetNetwork("");

            result.Should().Be("");
            account.LastError.Should().Contain("network identifier cannot be empty");
        }

        [Fact]
        public void SetBlockchain_ValidChain_SetsCorrectly()
        {
            var account = new CEPAccount();
            string testChain = "0xABCDEF1234567890";

            account.SetBlockchain(testChain);

            account.Blockchain.Should().Be(testChain); // Go stores chain as-is
        }

        [Fact]
        public void SetBlockchain_EmptyChain_SetsEmpty()
        {
            var account = new CEPAccount();

            account.SetBlockchain("");

            account.Blockchain.Should().Be(""); // Go sets exactly what you pass
        }

        [Fact]
        public void GetLastError_ReturnsCurrentError()
        {
            var account = new CEPAccount();
            account.Open(""); // This should set an error

            string error = account.LastError;

            error.Should().NotBeEmpty();
            error.Should().Be(account.LastError);
        }

        [Fact]
        public void UpdateAccount_NoAddress_ReturnsFalse()
        {
            var account = new CEPAccount();

            bool result = account.UpdateAccount();

            result.Should().BeFalse();
            account.LastError.Should().Be("Account not open"); // Exact Go error message
        }

        [Fact]
        public void SubmitCertificate_EmptyData_SetsError()
        {
            var account = new CEPAccount();
            account.Open("0x1234567890abcdef");

            account.SubmitCertificate("", "privatekey123");

            account.LastError.Should().Contain("Certificate data cannot be empty");
        }

        [Fact]
        public void SubmitCertificate_EmptyPrivateKey_SetsError()
        {
            var account = new CEPAccount();
            account.Open("0x1234567890abcdef");

            account.SubmitCertificate("test data", "");

            account.LastError.Should().Contain("Private key cannot be empty");
        }

        [Fact]
        public void Properties_CanBeSetAndRetrieved()
        {
            var account = new CEPAccount();

            // Test all properties can be set and retrieved
            account.Address = "test_address";
            account.PublicKey = "test_pubkey";
            account.Info = new { test = "info" };
            account.CodeVersion = "test_version";
            account.LastError = "test_error";
            account.NAGURL = "test_nag";
            account.NetworkNode = "test_node";
            account.Blockchain = "test_chain";
            account.LatestTxID = "test_tx";
            account.Nonce = 123;
            account.IntervalSec = 10;
            account.NetworkURL = "test_network_url";

            account.Address.Should().Be("test_address");
            account.PublicKey.Should().Be("test_pubkey");
            account.Info.Should().NotBeNull();
            account.CodeVersion.Should().Be("test_version");
            account.LastError.Should().Be("test_error");
            account.NAGURL.Should().Be("test_nag");
            account.NetworkNode.Should().Be("test_node");
            account.Blockchain.Should().Be("test_chain");
            account.LatestTxID.Should().Be("test_tx");
            account.Nonce.Should().Be(123);
            account.IntervalSec.Should().Be(10);
            account.NetworkURL.Should().Be("test_network_url");
        }

        [Fact]
        public void GetTransaction_NullParameters_ReturnsNull()
        {
            var account = new CEPAccount();

            var result = account.GetTransaction("", "");

            result.Should().BeNull();
            account.LastError.Should().NotBeEmpty();
        }

        [Fact]
        public void GetTransactionOutcome_EmptyTxID_ReturnsNull()
        {
            var account = new CEPAccount();

            var result = account.GetTransactionOutcome("", 30, 5);

            result.Should().BeNull();
            account.LastError.Should().NotBeEmpty();
        }
    }
}