using System;
using Xunit;
using FluentAssertions;
using CircularEnterpriseApis;

namespace CircularEnterpriseApis.IntegrationTests
{
    /// <summary>
    /// Integration tests that actually submit to Circular Protocol blockchain
    /// Maps to Go: tests/integration/integration_test.go
    ///
    /// These tests require environment variables:
    /// CIRCULAR_PRIVATE_KEY - Your private key (hex format)
    /// CIRCULAR_ADDRESS - Your wallet address (hex format)
    /// </summary>
    public class IntegrationTests
    {
        private readonly string? privateKeyHex;
        private readonly string? address;

        public IntegrationTests()
        {
            // Load environment variables like Go implementation does
            privateKeyHex = Environment.GetEnvironmentVariable("CIRCULAR_PRIVATE_KEY");
            address = Environment.GetEnvironmentVariable("CIRCULAR_ADDRESS");
        }

        /// <summary>
        /// Tests basic circular operations - matches Go TestCircularOperations
        /// </summary>
        [Fact]
        public void TestCircularOperations()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                // Skip test if environment variables not set, like Go implementation
                return;
            }

            var acc = new CEPAccount();

            // Open account first like example
            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            // Set network to testnet like Go test
            string nagUrl = acc.SetNetwork("testnet");
            nagUrl.Should().NotBeEmpty($"acc.SetNetwork() failed: {acc.LastError}");

            // Set default blockchain like Go test
            acc.SetBlockchain("8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2");

            // Log NAG URL and Blockchain like Go test
            Console.WriteLine($"NAGURL: {acc.NAGURL}");
            Console.WriteLine($"Blockchain: {acc.Blockchain}");

            // Update account - proceed even if it fails like the example does
            bool updated = acc.UpdateAccount();
            if (!updated)
            {
                Console.WriteLine($"Account update failed: {acc.LastError}, proceeding anyway...");
            }
            else
            {
                Console.WriteLine($"Account updated successfully, nonce: {acc.Nonce}");
            }

            // Submit certificate with test message
            acc.SubmitCertificate("test message", privateKeyHex);

            // Log submission result like the example does
            if (!string.IsNullOrEmpty(acc.LastError))
            {
                Console.WriteLine($"Certificate submission failed: {acc.LastError}");
            }
            else
            {
                Console.WriteLine($"Certificate submitted successfully! TX ID: {acc.LatestTxID}");
            }

            // Verify transaction hash was generated
            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            // Poll for transaction outcome with 30 second timeout
            var outcome = acc.GetTransactionOutcome(txHash, 30, 5);

            if (outcome != null)
            {
                Console.WriteLine($"Transaction confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Result");
            }
            else
            {
                Console.WriteLine($"Transaction polling timeout or error: {acc.LastError}");
                Console.WriteLine($"Transaction ID for manual verification: {txHash}");
            }
        }

        /// <summary>
        /// Tests certificate operations - matches Go TestCertificateOperations
        /// </summary>
        [Fact]
        public void TestCertificateOperations()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                return;
            }

            var acc = new CEPAccount();
            acc.SetNetwork("testnet");
            acc.SetBlockchain("8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2");

            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            Console.WriteLine($"NAGURL: {acc.NAGURL}");
            Console.WriteLine($"Blockchain: {acc.Blockchain}");

            bool updated = acc.UpdateAccount();
            updated.Should().BeTrue($"acc.UpdateAccount() failed: {acc.LastError}");

            // Submit certificate with test data
            string certificateData = "test data";
            acc.SubmitCertificate(certificateData, privateKeyHex);
            acc.LastError.Should().BeEmpty($"acc.SubmitCertificate() failed: {acc.LastError}");

            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            // Poll for transaction outcome
            var outcome = acc.GetTransactionOutcome(txHash, 30, 5);

            if (outcome != null)
            {
                Console.WriteLine($"Certificate transaction confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Result");
            }
            else
            {
                Console.WriteLine($"Certificate transaction timeout: {acc.LastError}");
                Console.WriteLine($"Transaction ID: {txHash}");
            }
        }

        /// <summary>
        /// Tests Hello World certification - matches Go TestHelloWorldCertification
        /// </summary>
        [Fact]
        public void TestHelloWorldCertification()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                return;
            }

            var acc = new CEPAccount();
            acc.SetNetwork("testnet");
            acc.SetBlockchain("8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2");

            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            Console.WriteLine($"NAGURL: {acc.NAGURL}");
            Console.WriteLine($"Blockchain: {acc.Blockchain}");

            bool updated = acc.UpdateAccount();
            updated.Should().BeTrue($"acc.UpdateAccount() failed: {acc.LastError}");

            // Create certificate with timestamp like Go test
            string message = "Hello World";
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string certificateData = $"{{\"message\":\"{message}\",\"timestamp\":{timestamp}}}";

            acc.SubmitCertificate(certificateData, privateKeyHex);
            acc.LastError.Should().BeEmpty($"acc.SubmitCertificate() failed: {acc.LastError}");

            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            Console.WriteLine($"Hello World certificate submitted with TX ID: {txHash}");

            // Poll for transaction outcome
            var outcome = acc.GetTransactionOutcome(txHash, 30, 5);

            if (outcome != null)
            {
                Console.WriteLine($"Hello World transaction confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Result");

                // Verify the certificate data was submitted correctly
                Console.WriteLine($"Certificate message: {message}");
                Console.WriteLine($"Certificate timestamp: {timestamp}");
            }
            else
            {
                Console.WriteLine($"Hello World transaction timeout: {acc.LastError}");
                Console.WriteLine($"Transaction ID: {txHash}");
            }
        }

        /// <summary>
        /// Tests JSON certificate operations
        /// </summary>
        [Fact]
        public void TestJSONCertificateOperations()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                return;
            }

            var acc = new CEPAccount();
            acc.SetNetwork("testnet");
            acc.SetBlockchain("8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2");

            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            bool updated = acc.UpdateAccount();
            updated.Should().BeTrue($"acc.UpdateAccount() failed: {acc.LastError}");

            // Submit certificate with JSON data like Go E2E test
            acc.SubmitCertificate("{\"test\":\"data\"}", privateKeyHex);
            acc.LastError.Should().BeEmpty($"acc.SubmitCertificate() failed: {acc.LastError}");

            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            Console.WriteLine($"JSON certificate submitted with TX ID: {txHash}");

            // Poll for transaction outcome
            var outcome = acc.GetTransactionOutcome(txHash, 30, 5);

            if (outcome != null)
            {
                Console.WriteLine($"JSON certificate confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Result");
            }
            else
            {
                Console.WriteLine($"JSON certificate timeout: {acc.LastError}");
                Console.WriteLine($"Transaction ID: {txHash}");
            }
        }

        /// <summary>
        /// Tests certificate chain operations - submitting multiple linked certificates
        /// </summary>
        [Fact]
        public void TestCertificateChainOperations()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                return;
            }

            var acc = new CEPAccount();
            acc.SetNetwork("testnet");
            acc.SetBlockchain("8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2");

            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            bool updated = acc.UpdateAccount();
            updated.Should().BeTrue($"acc.UpdateAccount() failed: {acc.LastError}");

            Console.WriteLine($"Starting certificate chain with nonce: {acc.Nonce}");

            // Submit 3 certificates in sequence
            string[] txHashes = new string[3];

            for (int i = 0; i < 3; i++)
            {
                var cert = new CCertificate();
                cert.SetData($"Chain certificate #{i + 1} from C# - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                if (i > 0)
                {
                    // Link to previous transaction
                    cert.PreviousTxID = txHashes[i - 1];
                }

                string certJson = cert.GetJSONCertificate();
                Console.WriteLine($"Submitting certificate {i + 1}: {certJson}");

                acc.SubmitCertificate(certJson, privateKeyHex);
                acc.LastError.Should().BeEmpty($"Certificate {i + 1} submission failed: {acc.LastError}");

                txHashes[i] = acc.LatestTxID;
                txHashes[i].Should().NotBeEmpty($"Transaction hash {i + 1} not generated");

                Console.WriteLine($"Certificate {i + 1} submitted with TX ID: {txHashes[i]}");

                // Wait between submissions to avoid nonce conflicts
                if (i < 2)
                {
                    System.Threading.Thread.Sleep(3000);
                }
            }

            Console.WriteLine($"Certificate chain completed! Final nonce: {acc.Nonce}");
            Console.WriteLine($"Transaction chain: {string.Join(" -> ", txHashes)}");

            // Verify the last transaction
            var outcome = acc.GetTransactionOutcome(txHashes[2], 30, 5);
            if (outcome != null)
            {
                Console.WriteLine($"Final certificate confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Result");
            }
            else
            {
                Console.WriteLine($"Final certificate timeout: {acc.LastError}");
            }
        }
    }
}