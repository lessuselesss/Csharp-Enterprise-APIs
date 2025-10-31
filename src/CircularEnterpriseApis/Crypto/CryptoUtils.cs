using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1;
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
    /// INTERNAL: Not part of public API - matches Rust/Go reference implementations
    /// Crypto operations are private implementation details for security
    /// </summary>
    internal static class CryptoUtils
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
        /// Uses DER encoding to match Rust reference implementation exactly
        /// Rust: sig.serialize_der() produces ASN.1 DER encoded signature
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

                // Encode signature in DER format (ASN.1) to match Rust reference
                // Rust: sig.serialize_der() -> SEQUENCE { INTEGER r, INTEGER s }
                var derSignature = new DerSequence(
                    new DerInteger(r),
                    new DerInteger(s)
                ).GetDerEncoded();

                return BitConverter.ToString(derSignature).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Signature generation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifies an ECDSA signature (DER encoded format)
        /// </summary>
        public static bool VerifySignature(string publicKeyHex, string message, string signatureHex)
        {
            try
            {
                // Parse DER-encoded signature
                signatureHex = HexFix(signatureHex);
                byte[] signatureBytes = HexStringToBytes(signatureHex);

                // Decode DER signature: SEQUENCE { INTEGER r, INTEGER s }
                var derSequence = (DerSequence)Asn1Object.FromByteArray(signatureBytes);
                var r = ((DerInteger)derSequence[0]).Value;
                var s = ((DerInteger)derSequence[1]).Value;

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

        // GetEthereumAddress removed - not part of Circular Protocol API spec
        // Not present in Rust/Go/C++ reference implementations
    }
}