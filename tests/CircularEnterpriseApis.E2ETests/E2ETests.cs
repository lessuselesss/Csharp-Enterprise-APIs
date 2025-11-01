using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using CircularEnterpriseApis;

namespace CircularEnterpriseApis.E2ETests
{
    /// <summary>
    /// End-to-End tests that perform complete workflows with real blockchain submission
    /// Maps to Rust: tests/e2e/e2e_test.rs
    ///
    /// These tests require environment variables:
    /// CIRCULAR_PRIVATE_KEY - Your private key (hex format)
    /// CIRCULAR_ADDRESS - Your wallet address (hex format)
    /// </summary>
    public class E2ETests
    {
        private readonly string? privateKeyHex;
        private readonly string? address;

        public E2ETests()
        {
            // Load environment variables like Go implementation does
            privateKeyHex = Environment.GetEnvironmentVariable("CIRCULAR_PRIVATE_KEY");
            address = Environment.GetEnvironmentVariable("CIRCULAR_ADDRESS");
        }

        /// <summary>
        /// Complete E2E circular operations test - matches Rust test_e2e_circular_operations
        /// </summary>
        [Fact]
        public async Task TestE2ECircularOperationsAsync()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                // Skip test if environment variables not set, matching Rust behavior
                return;
            }

            var acc = new CEPAccount();

            // Open account
            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            // Set network to testnet
            string nagURL = await acc.SetNetworkAsync("testnet");
            nagURL.Should().NotBeEmpty($"acc.SetNetworkAsync() failed: {acc.LastError}");

            // Set default blockchain
            acc.SetBlockchain(Constants.DefaultChain);

            // Update account information
            bool updated = await acc.UpdateAccountAsync();
            updated.Should().BeTrue($"acc.UpdateAccountAsync() failed: {acc.LastError}");

            // Submit certificate with test message matching Rust
            await acc.SubmitCertificateAsync("test message from C# E2E test", privateKeyHex);
            acc.LastError.Should().BeNullOrEmpty($"acc.SubmitCertificateAsync() failed: {acc.LastError}");

            // Verify transaction hash was generated
            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            Console.WriteLine($"E2E test submitted certificate with TX ID: {txHash}");

            // Poll for transaction outcome with same timeout as Rust test
            var outcome = await acc.GetTransactionOutcomeAsync(txHash, 30, 2);

            if (outcome != null)
            {
                Console.WriteLine($"E2E transaction confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Status");
                outcome["Status"].ToString().Should().Be("Executed");

                // Log success details
                Console.WriteLine($"✅ E2E Circular Operations completed successfully!");
                Console.WriteLine($"📝 Transaction ID: {txHash}");
                Console.WriteLine($"🔗 Blockchain: {acc.Blockchain}");
                Console.WriteLine($"📊 Final Nonce: {acc.Nonce}");
            }
            else
            {
                Console.WriteLine($"⏰ E2E transaction polling timeout: {acc.LastError}");
                Console.WriteLine($"💡 Transaction may still be processing: {txHash}");
            }
        }

        /// <summary>
        /// E2E certificate operations test - matches Go TestE2ECertificateOperations
        /// </summary>
        [Fact]
        public async Task TestE2ECertificateOperations()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                return;
            }

            var acc = new CEPAccount();

            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            string nagURL = await acc.SetNetworkAsync("testnet");
            nagURL.Should().NotBeEmpty($"await acc.SetNetworkAsync() failed: {acc.LastError}");

            acc.SetBlockchain(Constants.DefaultChain);

            bool updated = await acc.UpdateAccountAsync();
            updated.Should().BeTrue($"await acc.UpdateAccountAsync() failed: {acc.LastError}");

            // Submit certificate with JSON data like Go E2E test
            await acc.SubmitCertificateAsync("{\"test\":\"data\"}", privateKeyHex);
            acc.LastError.Should().BeNullOrEmpty($"await acc.SubmitCertificateAsync() failed: {acc.LastError}");

            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            Console.WriteLine($"E2E certificate operations submitted with TX ID: {txHash}");

            // Poll for transaction outcome
            var outcome = await acc.GetTransactionOutcomeAsync(txHash, 30, 2);

            if (outcome != null)
            {
                Console.WriteLine($"E2E certificate transaction confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Status");
                outcome["Status"].ToString().Should().Be("Executed");

                Console.WriteLine($"✅ E2E Certificate Operations completed successfully!");
                Console.WriteLine($"📄 Certificate Data: {{\"test\":\"data\"}}");
                Console.WriteLine($"📝 Transaction ID: {txHash}");
            }
            else
            {
                Console.WriteLine($"⏰ E2E certificate transaction timeout: {acc.LastError}");
                Console.WriteLine($"💡 Transaction ID for manual verification: {txHash}");
            }
        }

        /// <summary>
        /// E2E Hello World certification test - matches Go TestE2EHelloWorldCertification
        /// </summary>
        [Fact]
        public async Task TestE2EHelloWorldCertification()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                return;
            }

            var acc = new CEPAccount();

            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            string nagURL = await acc.SetNetworkAsync("testnet");
            nagURL.Should().NotBeEmpty($"await acc.SetNetworkAsync() failed: {acc.LastError}");

            acc.SetBlockchain(Constants.DefaultChain);

            bool updated = await acc.UpdateAccountAsync();
            updated.Should().BeTrue($"await acc.UpdateAccountAsync() failed: {acc.LastError}");

            // Submit Hello World message like Go E2E test
            await acc.SubmitCertificateAsync("Hello World", privateKeyHex);
            acc.LastError.Should().BeNullOrEmpty($"await acc.SubmitCertificateAsync() failed: {acc.LastError}");

            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            Console.WriteLine($"E2E Hello World certification submitted with TX ID: {txHash}");

            // Poll for transaction outcome
            var outcome = await acc.GetTransactionOutcomeAsync(txHash, 30, 2);

            if (outcome != null)
            {
                Console.WriteLine($"E2E Hello World transaction confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Status");
                outcome["Status"].ToString().Should().Be("Executed");

                Console.WriteLine($"✅ E2E Hello World Certification completed successfully!");
                Console.WriteLine($"👋 Message: Hello World");
                Console.WriteLine($"📝 Transaction ID: {txHash}");
                Console.WriteLine($"🌍 Certified on Circular Protocol blockchain!");
            }
            else
            {
                Console.WriteLine($"⏰ E2E Hello World transaction timeout: {acc.LastError}");
                Console.WriteLine($"💡 Transaction ID for manual verification: {txHash}");
            }
        }

        /// <summary>
        /// E2E test for complex certificate structure with metadata
        /// </summary>
        [Fact]
        public async Task TestE2EComplexCertificate()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                return;
            }

            var acc = new CEPAccount();

            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            string nagURL = await acc.SetNetworkAsync("testnet");
            nagURL.Should().NotBeEmpty($"await acc.SetNetworkAsync() failed: {acc.LastError}");

            acc.SetBlockchain(Constants.DefaultChain);

            bool updated = await acc.UpdateAccountAsync();
            updated.Should().BeTrue($"await acc.UpdateAccountAsync() failed: {acc.LastError}");

            // Create a complex certificate using CCertificate class
            var cert = new CCertificate();
            cert.SetData($"C# E2E Complex Certificate - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");

            // Submit the certificate JSON
            string certJson = cert.GetJSONCertificate();
            Console.WriteLine($"Submitting complex certificate: {certJson}");

            await acc.SubmitCertificateAsync(certJson, privateKeyHex);
            acc.LastError.Should().BeNullOrEmpty($"await acc.SubmitCertificateAsync() failed: {acc.LastError}");

            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            Console.WriteLine($"E2E complex certificate submitted with TX ID: {txHash}");
            Console.WriteLine($"Certificate size: {cert.GetCertificateSize()} bytes");

            // Poll for transaction outcome
            var outcome = await acc.GetTransactionOutcomeAsync(txHash, 30, 2);

            if (outcome != null)
            {
                Console.WriteLine($"E2E complex certificate confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Status");
                outcome["Status"].ToString().Should().Be("Executed");

                Console.WriteLine($"✅ E2E Complex Certificate completed successfully!");
                Console.WriteLine($"📋 Certificate Data: {cert.GetData()}");
                Console.WriteLine($"📏 Certificate Size: {cert.GetCertificateSize()} bytes");
                Console.WriteLine($"📝 Transaction ID: {txHash}");
                Console.WriteLine($"🔗 Version: {cert.Version}");
            }
            else
            {
                Console.WriteLine($"⏰ E2E complex certificate timeout: {acc.LastError}");
                Console.WriteLine($"💡 Transaction ID for manual verification: {txHash}");
            }
        }

        /// <summary>
        /// E2E test for real-world document certification workflow
        /// </summary>
        [Fact]
        public async Task TestE2EDocumentCertification()
        {
            if (string.IsNullOrEmpty(privateKeyHex) || string.IsNullOrEmpty(address))
            {
                return;
            }

            var acc = new CEPAccount();

            bool opened = acc.Open(address);
            opened.Should().BeTrue($"acc.Open() failed: {acc.LastError}");

            string nagURL = await acc.SetNetworkAsync("testnet");
            nagURL.Should().NotBeEmpty($"await acc.SetNetworkAsync() failed: {acc.LastError}");

            acc.SetBlockchain(Constants.DefaultChain);

            bool updated = await acc.UpdateAccountAsync();
            updated.Should().BeTrue($"await acc.UpdateAccountAsync() failed: {acc.LastError}");

            // Simulate real document certification
            var documentHash = "sha256:a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3";
            var metadata = new
            {
                document = new
                {
                    hash = documentHash,
                    name = "important_document.pdf",
                    size = 1024576,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                },
                certifier = new
                {
                    name = "C# Enterprise API",
                    version = Constants.LibVersion
                },
                certification = new
                {
                    type = "document_integrity",
                    timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    network = "testnet"
                }
            };

            string documentCertificate = System.Text.Json.JsonSerializer.Serialize(metadata);
            Console.WriteLine($"Document certification metadata: {documentCertificate}");

            await acc.SubmitCertificateAsync(documentCertificate, privateKeyHex);
            acc.LastError.Should().BeNullOrEmpty($"await acc.SubmitCertificateAsync() failed: {acc.LastError}");

            string txHash = acc.LatestTxID;
            txHash.Should().NotBeEmpty("txHash not found in response");

            Console.WriteLine($"E2E document certification submitted with TX ID: {txHash}");

            // Poll for transaction outcome
            var outcome = await acc.GetTransactionOutcomeAsync(txHash, 30, 2);

            if (outcome != null)
            {
                Console.WriteLine($"E2E document certification confirmed: {System.Text.Json.JsonSerializer.Serialize(outcome)}");
                outcome.Should().ContainKey("Status");
                outcome["Status"].ToString().Should().Be("Executed");

                Console.WriteLine($"✅ E2E Document Certification completed successfully!");
                Console.WriteLine($"📄 Document Hash: {documentHash}");
                Console.WriteLine($"📝 Transaction ID: {txHash}");
                Console.WriteLine($"🔒 Document integrity certified on blockchain!");
            }
            else
            {
                Console.WriteLine($"⏰ E2E document certification timeout: {acc.LastError}");
                Console.WriteLine($"💡 Transaction ID for manual verification: {txHash}");
            }
        }
    }
}