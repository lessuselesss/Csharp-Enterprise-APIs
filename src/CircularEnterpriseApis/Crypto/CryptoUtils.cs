using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Encoders;
using static CircularEnterpriseApis.Utils;
using BCECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace CircularEnterpriseApis.Crypto
{
    /// <summary>
    /// Cryptographic utilities for ECDSA signing with secp256k1
    /// Must match Go implementation exactly for compatibility
    /// </summary>
    public static class CryptoUtils
    {
        private static readonly ECDomainParameters secp256k1;

        static CryptoUtils()
        {
            // Initialize secp256k1 curve parameters
            var curve = SecNamedCurves.GetByName("secp256k1");
            secp256k1 = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
        }

        /// <summary>
        /// Helper method to parse hex strings for .NET Standard 2.1 compatibility
        /// </summary>
        private static byte[] HexStringToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have even length");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Computes SHA-256 hash of input data
        /// Matches Go: crypto/sha256.Sum256()
        /// </summary>
        public static byte[] Sha256(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        /// <summary>
        /// Computes SHA-256 hash of input string
        /// </summary>
        public static byte[] Sha256(string data)
        {
            return Sha256(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Computes SHA-256 hash and returns as hex string
        /// </summary>
        public static string Sha256Hex(string data)
        {
            byte[] hash = Sha256(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Creates ECDSA private key from hex string
        /// Matches Go secp256k1 private key handling
        /// </summary>
        public static ECPrivateKeyParameters CreatePrivateKey(string privateKeyHex)
        {
            try
            {
                privateKeyHex = HexFix(privateKeyHex);
                byte[] keyBytes = HexStringToBytes(privateKeyHex);
                BigInteger d = new BigInteger(1, keyBytes);
                return new ECPrivateKeyParameters(d, secp256k1);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid private key format: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the public key corresponding to a private key
        /// </summary>
        public static ECPublicKeyParameters GetPublicKey(ECPrivateKeyParameters privateKey)
        {
            BCECPoint q = secp256k1.G.Multiply(privateKey.D);
            return new ECPublicKeyParameters(q, secp256k1);
        }

        /// <summary>
        /// Signs a message hash using ECDSA with RFC 6979 deterministic signatures
        /// This MUST produce identical signatures to the Go implementation
        /// </summary>
        public static string SignMessage(string privateKeyHex, string message)
        {
            try
            {
                ECPrivateKeyParameters privateKey = CreatePrivateKey(privateKeyHex);
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] messageHash = Sha256(messageBytes);

                // Use RFC 6979 deterministic ECDSA with SHA-256
                var signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha256Digest()));
                signer.Init(true, privateKey);

                BigInteger[] signature = signer.GenerateSignature(messageHash);
                BigInteger r = signature[0];
                BigInteger s = signature[1];

                // Ensure low S value (canonical signature)
                BigInteger halfOrder = secp256k1.N.ShiftRight(1);
                if (s.CompareTo(halfOrder) > 0)
                {
                    s = secp256k1.N.Subtract(s);
                }

                // Convert to hex format matching Go output
                byte[] rBytes = r.ToByteArrayUnsigned();
                byte[] sBytes = s.ToByteArrayUnsigned();

                // Pad to 32 bytes each
                byte[] rPadded = new byte[32];
                byte[] sPadded = new byte[32];

                Array.Copy(rBytes, 0, rPadded, 32 - rBytes.Length, rBytes.Length);
                Array.Copy(sBytes, 0, sPadded, 32 - sBytes.Length, sBytes.Length);

                // Concatenate r and s
                byte[] fullSignature = new byte[64];
                Array.Copy(rPadded, 0, fullSignature, 0, 32);
                Array.Copy(sPadded, 0, fullSignature, 32, 32);

                return BitConverter.ToString(fullSignature).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Signature generation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifies an ECDSA signature
        /// </summary>
        public static bool VerifySignature(string publicKeyHex, string message, string signatureHex)
        {
            try
            {
                // Parse signature
                signatureHex = HexFix(signatureHex);
                if (signatureHex.Length != 128) // 64 bytes = 128 hex chars
                    return false;

                byte[] rBytes = HexStringToBytes(signatureHex.Substring(0, 64));
                byte[] sBytes = HexStringToBytes(signatureHex.Substring(64, 64));

                BigInteger r = new BigInteger(1, rBytes);
                BigInteger s = new BigInteger(1, sBytes);

                // Parse public key
                publicKeyHex = HexFix(publicKeyHex);
                byte[] pubKeyBytes = HexStringToBytes(publicKeyHex);

                BCECPoint pubKeyPoint = secp256k1.Curve.DecodePoint(pubKeyBytes);
                ECPublicKeyParameters publicKey = new ECPublicKeyParameters(pubKeyPoint, secp256k1);

                // Hash message
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] messageHash = Sha256(messageBytes);

                // Verify signature
                var verifier = new ECDsaSigner();
                verifier.Init(false, publicKey);
                return verifier.VerifySignature(messageHash, r, s);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Derives the public key from a private key
        /// Matches Go: crypto/ecdsa PublicKey derivation
        /// </summary>
        public static string GetPublicKeyFromPrivateKey(string privateKeyHex)
        {
            try
            {
                ECPrivateKeyParameters privateKey = CreatePrivateKey(privateKeyHex);
                BCECPoint q = secp256k1.G.Multiply(privateKey.D);

                // Get uncompressed public key (04 + x + y coordinates)
                byte[] publicKeyBytes = q.GetEncoded(false);

                // Return as hex string without 0x prefix
                return BitConverter.ToString(publicKeyBytes).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to derive public key: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Derives Ethereum-style address from public key
        /// Not used in Circular Protocol but included for completeness
        /// </summary>
        public static string GetEthereumAddress(ECPublicKeyParameters publicKey)
        {
            try
            {
                byte[] pubKeyBytes = publicKey.Q.GetEncoded(false); // Uncompressed format
                byte[] pubKeyNoPrefix = new byte[64]; // Remove 0x04 prefix
                Array.Copy(pubKeyBytes, 1, pubKeyNoPrefix, 0, 64);

                byte[] addressBytes = Sha256(pubKeyNoPrefix);
                byte[] last20Bytes = new byte[20];
                Array.Copy(addressBytes, addressBytes.Length - 20, last20Bytes, 0, 20);

                return "0x" + BitConverter.ToString(last20Bytes).Replace("-", "").ToLowerInvariant();
            }
            catch
            {
                return "";
            }
        }
    }
}