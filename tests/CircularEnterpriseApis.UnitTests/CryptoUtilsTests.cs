using Xunit;
using FluentAssertions;
using CircularEnterpriseApis.Crypto;
using System;

namespace CircularEnterpriseApis.UnitTests
{
    /// <summary>
    /// Unit tests for CryptoUtils class
    /// Critical for compatibility with Go implementation
    /// </summary>
    public class CryptoUtilsTests
    {
        // Test vectors for consistency with Go implementation
        private const string TestPrivateKey = "3077eb3b0c8e5d4f0c1c0c8e5d4f0c1c0c8e5d4f0c1c0c8e5d4f0c1c0c8e5d4f";
        private const string TestMessage = "test message for signing";

        [Fact]
        public void Sha256_EmptyData_ReturnsExpectedHash()
        {
            byte[] result = CryptoUtils.Sha256(new byte[0]);

            // SHA-256 of empty data
            string expectedHex = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            string actualHex = BitConverter.ToString(result).Replace("-", "").ToLowerInvariant();

            actualHex.Should().Be(expectedHex);
        }

        [Fact]
        public void Sha256_KnownData_ReturnsExpectedHash()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes("hello world");
            byte[] result = CryptoUtils.Sha256(data);

            // Known SHA-256 of "hello world"
            string expectedHex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";
            string actualHex = BitConverter.ToString(result).Replace("-", "").ToLowerInvariant();

            actualHex.Should().Be(expectedHex);
        }

        [Fact]
        public void Sha256_StringOverload_WorksCorrectly()
        {
            string result = CryptoUtils.Sha256Hex("hello world");

            string expected = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";
            result.Should().Be(expected);
        }

        [Fact]
        public void CreatePrivateKey_ValidHex_CreatesKey()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";

            var privateKey = CryptoUtils.CreatePrivateKey(privateKeyHex);

            privateKey.Should().NotBeNull();
            privateKey.D.Should().NotBeNull();
        }

        [Fact]
        public void CreatePrivateKey_WithPrefix_HandlesCorrectly()
        {
            string privateKeyHex = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";

            var privateKey = CryptoUtils.CreatePrivateKey(privateKeyHex);

            privateKey.Should().NotBeNull();
        }

        [Fact]
        public void CreatePrivateKey_InvalidHex_ThrowsException()
        {
            string invalidHex = "invalid_hex_string";

            Action act = () => CryptoUtils.CreatePrivateKey(invalidHex);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetPublicKey_ValidPrivateKey_ReturnsPublicKey()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
            var privateKey = CryptoUtils.CreatePrivateKey(privateKeyHex);

            var publicKey = CryptoUtils.GetPublicKey(privateKey);

            publicKey.Should().NotBeNull();
            publicKey.Q.Should().NotBeNull();
        }

        [Fact]
        public void SignMessage_ValidInput_ProducesSignature()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
            string message = "test message";

            string signature = CryptoUtils.SignMessage(privateKeyHex, message);

            signature.Should().NotBeEmpty();
            signature.Length.Should().Be(128); // 64 bytes = 128 hex chars
            signature.Should().MatchRegex("^[0-9a-f]+$"); // Only hex characters
        }

        [Fact]
        public void SignMessage_SameInputs_ProducesSameSignature()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
            string message = "deterministic test";

            string signature1 = CryptoUtils.SignMessage(privateKeyHex, message);
            string signature2 = CryptoUtils.SignMessage(privateKeyHex, message);

            // RFC 6979 deterministic signatures should be identical
            signature1.Should().Be(signature2);
        }

        [Fact]
        public void SignMessage_DifferentMessages_ProduceDifferentSignatures()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";

            string signature1 = CryptoUtils.SignMessage(privateKeyHex, "message1");
            string signature2 = CryptoUtils.SignMessage(privateKeyHex, "message2");

            signature1.Should().NotBe(signature2);
        }

        [Fact]
        public void VerifySignature_ValidSignature_ReturnsTrue()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
            string message = "test verification message";

            // Create signature
            string signature = CryptoUtils.SignMessage(privateKeyHex, message);

            // Get public key for verification
            var privateKey = CryptoUtils.CreatePrivateKey(privateKeyHex);
            var publicKey = CryptoUtils.GetPublicKey(privateKey);
            byte[] publicKeyBytes = publicKey.Q.GetEncoded(false); // Uncompressed format
            string publicKeyHex = BitConverter.ToString(publicKeyBytes).Replace("-", "").ToLowerInvariant();

            // Verify signature
            bool isValid = CryptoUtils.VerifySignature(publicKeyHex, message, signature);

            isValid.Should().BeTrue();
        }

        [Fact]
        public void VerifySignature_WrongMessage_ReturnsFalse()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
            string originalMessage = "original message";
            string wrongMessage = "wrong message";

            // Create signature for original message
            string signature = CryptoUtils.SignMessage(privateKeyHex, originalMessage);

            // Get public key
            var privateKey = CryptoUtils.CreatePrivateKey(privateKeyHex);
            var publicKey = CryptoUtils.GetPublicKey(privateKey);
            byte[] publicKeyBytes = publicKey.Q.GetEncoded(false);
            string publicKeyHex = BitConverter.ToString(publicKeyBytes).Replace("-", "").ToLowerInvariant();

            // Verify with wrong message
            bool isValid = CryptoUtils.VerifySignature(publicKeyHex, wrongMessage, signature);

            isValid.Should().BeFalse();
        }

        [Fact]
        public void VerifySignature_InvalidSignature_ReturnsFalse()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
            string message = "test message";
            string invalidSignature = "invalid_signature_format";

            // Get public key
            var privateKey = CryptoUtils.CreatePrivateKey(privateKeyHex);
            var publicKey = CryptoUtils.GetPublicKey(privateKey);
            byte[] publicKeyBytes = publicKey.Q.GetEncoded(false);
            string publicKeyHex = BitConverter.ToString(publicKeyBytes).Replace("-", "").ToLowerInvariant();

            // Verify with invalid signature
            bool isValid = CryptoUtils.VerifySignature(publicKeyHex, message, invalidSignature);

            isValid.Should().BeFalse();
        }

        [Fact]
        public void GetEthereumAddress_ValidPublicKey_ReturnsAddress()
        {
            string privateKeyHex = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
            var privateKey = CryptoUtils.CreatePrivateKey(privateKeyHex);
            var publicKey = CryptoUtils.GetPublicKey(privateKey);

            string address = CryptoUtils.GetEthereumAddress(publicKey);

            address.Should().NotBeEmpty();
            address.Should().StartWith("0x");
            address.Length.Should().Be(42); // 0x + 40 hex chars
            address.Should().MatchRegex("^0x[0-9a-f]+$");
        }

        [Theory]
        [InlineData("test")]
        [InlineData("hello world")]
        [InlineData("")]
        [InlineData("special chars: !@#$%^&*()")]
        public void Sha256Hex_VariousInputs_ProducesConsistentResults(string input)
        {
            string result1 = CryptoUtils.Sha256Hex(input);
            string result2 = CryptoUtils.Sha256Hex(input);

            result1.Should().Be(result2);
            result1.Length.Should().Be(64); // SHA-256 = 32 bytes = 64 hex chars
            result1.Should().MatchRegex("^[0-9a-f]+$");
        }
    }
}