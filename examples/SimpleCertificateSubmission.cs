using System;
using System.Threading.Tasks;
using CircularEnterpriseApis;

namespace CircularEnterpriseApis.Examples
{
    /// <summary>
    /// Simple example demonstrating certificate submission to Circular Protocol blockchain
    /// Maps to Go: examples/simple_certificate_submission.go
    /// </summary>
    public class SimpleCertificateSubmission
    {
        public static async Task RunExample()
        {
            try
            {
                Console.WriteLine("=== Circular Protocol C# Enterprise API Example ===");
                Console.WriteLine($"Library Version: {Constants.LibVersion}");
                Console.WriteLine();

                // Get environment variables (same as Go implementation)
                string? privateKey = Environment.GetEnvironmentVariable("CIRCULAR_PRIVATE_KEY");
                string? address = Environment.GetEnvironmentVariable("CIRCULAR_ADDRESS");

                if (string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(address))
                {
                    Console.WriteLine("❌ Error: Required environment variables not set:");
                    Console.WriteLine("   CIRCULAR_PRIVATE_KEY - Your private key (hex format)");
                    Console.WriteLine("   CIRCULAR_ADDRESS - Your wallet address (hex format)");
                    Console.WriteLine();
                    Console.WriteLine("Example:");
                    Console.WriteLine("   export CIRCULAR_PRIVATE_KEY=0x1234567890abcdef...");
                    Console.WriteLine("   export CIRCULAR_ADDRESS=0xabcdef1234567890...");
                    return;
                }

                // Create new account instance
                Console.WriteLine("🔧 Creating new CEP account...");
                var account = new CEPAccount();

                // Open account with address
                Console.WriteLine($"📂 Opening account: {address}");
                bool opened = account.Open(address);
                if (!opened)
                {
                    Console.WriteLine($"❌ Failed to open account: {account.LastError}");
                    return;
                }

                // Set network (use testnet by default)
                Console.WriteLine("🌐 Setting network to testnet...");
                string nagUrl = account.SetNetwork("testnet");
                Console.WriteLine($"📡 NAG URL: {nagUrl}");

                if (!string.IsNullOrEmpty(account.LastError))
                {
                    Console.WriteLine($"⚠️  Network warning: {account.LastError}");
                }

                // Update account information
                Console.WriteLine("🔄 Updating account information...");
                bool updated = account.UpdateAccount();
                if (updated)
                {
                    Console.WriteLine($"✅ Account updated. Current nonce: {account.Nonce}");
                }
                else
                {
                    Console.WriteLine($"⚠️  Account update failed: {account.LastError}");
                    Console.WriteLine("🚀 Proceeding with certificate submission anyway...");
                }

                // Create a test certificate
                Console.WriteLine();
                Console.WriteLine("📋 Creating test certificate...");
                var certificate = new CCertificate();
                certificate.SetData($"Test certificate created at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC using C# API");

                string certificateJson = certificate.GetJSONCertificate();
                Console.WriteLine($"📄 Certificate JSON: {certificateJson}");
                Console.WriteLine($"📏 Certificate size: {certificate.GetCertificateSize()} bytes");

                // Submit certificate to blockchain
                Console.WriteLine();
                Console.WriteLine("🚀 Submitting certificate to blockchain...");
                account.SubmitCertificate(certificateJson, privateKey);

                if (!string.IsNullOrEmpty(account.LastError))
                {
                    Console.WriteLine($"❌ Certificate submission failed: {account.LastError}");
                    return;
                }

                string txId = account.LatestTxID;
                Console.WriteLine($"✅ Certificate submitted successfully!");
                Console.WriteLine($"📝 Transaction ID: {txId}");

                // Poll for transaction outcome
                Console.WriteLine();
                Console.WriteLine("⏳ Waiting for transaction confirmation...");
                Console.WriteLine("(This may take up to 30 seconds)");

                var outcome = account.GetTransactionOutcome(txId, 30, 5);
                if (outcome != null)
                {
                    Console.WriteLine("🎉 Transaction confirmed!");
                    Console.WriteLine($"📊 Transaction outcome: {System.Text.Json.JsonSerializer.Serialize(outcome, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
                }
                else
                {
                    Console.WriteLine($"⏰ Transaction confirmation timeout or error: {account.LastError}");
                    Console.WriteLine($"💡 You can check the transaction later using ID: {txId}");
                }

                Console.WriteLine();
                Console.WriteLine("🏁 Example completed successfully!");
                Console.WriteLine($"📈 Final account nonce: {account.Nonce}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Unexpected error: {ex.Message}");
                Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Demonstrates certificate chain submission with multiple certificates
        /// </summary>
        public static async Task RunChainSubmissionExample()
        {
            try
            {
                Console.WriteLine("=== Multiple Certificate Chain Submission Example ===");
                Console.WriteLine();

                string? privateKey = Environment.GetEnvironmentVariable("CIRCULAR_PRIVATE_KEY");
                string? address = Environment.GetEnvironmentVariable("CIRCULAR_ADDRESS");

                if (string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(address))
                {
                    Console.WriteLine("❌ Environment variables not set. Please set CIRCULAR_PRIVATE_KEY and CIRCULAR_ADDRESS");
                    return;
                }

                var account = new CEPAccount();
                account.Open(address);
                account.SetNetwork("testnet");
                account.UpdateAccount();

                Console.WriteLine($"🔗 Starting chain submission with nonce: {account.Nonce}");

                // Submit 3 certificates in sequence
                for (int i = 1; i <= 3; i++)
                {
                    Console.WriteLine($"\n📋 Creating certificate {i}/3...");

                    var cert = new CCertificate();
                    cert.SetData($"Chain certificate #{i} - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                    if (i > 1)
                    {
                        // Link to previous transaction (using property directly)
                        cert.PreviousTxID = account.LatestTxID;
                    }

                    Console.WriteLine($"🚀 Submitting certificate {i}...");
                    account.SubmitCertificate(cert.GetJSONCertificate(), privateKey);

                    if (!string.IsNullOrEmpty(account.LastError))
                    {
                        Console.WriteLine($"❌ Certificate {i} submission failed: {account.LastError}");
                        break;
                    }

                    Console.WriteLine($"✅ Certificate {i} submitted: {account.LatestTxID}");

                    // Wait between submissions
                    if (i < 3)
                    {
                        Console.WriteLine("⏳ Waiting 5 seconds before next submission...");
                        await Task.Delay(5000);
                    }
                }

                Console.WriteLine($"\n🏁 Chain submission completed! Final nonce: {account.Nonce}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Chain submission error: {ex.Message}");
            }
        }
    }
}