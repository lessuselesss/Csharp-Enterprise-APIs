# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **C# implementation** of the Circular Protocol Enterprise APIs that **aligns with reference implementations** (Node.js, PHP, Java) to provide consistent API surface, ergonomics, and developer experience across all supported languages.

**Critical Requirement**: This C# implementation maintains API compatibility with reference implementations - same method names, parameter names, return types, and behavioral patterns.

## Reference Architecture (from Node.js/PHP/Java)

### Core API Surface

**Primary Classes:**
- `CEPAccount` / `CEP_Account` - Main client interface for blockchain operations
- `CCertificate` / `C_CERTIFICATE` - Certificate data structure and operations
- `Utils` - Utility functions for hex/string conversion

**Key Methods (aligned with reference implementations):**
```csharp
// CEPAccount (matches Node.js/PHP/Java)
new CEPAccount()                                                    // Constructor
bool Open(string address)
string SetNetwork(string network)
void SetBlockchain(string chain)
bool UpdateAccount()
string SignData(string data, string privateKeyHex)               // Node.js/Java
void SubmitCertificate(string pdata, string privateKeyHex)
Dictionary<string, object> GetTransaction(string blockID, string txID)
Dictionary<string, object> GetTransactionOutcome(string txID, int timeoutSec, int intervalSec)
void Close()

// CCertificate (matches Node.js/Java)
new CCertificate()                                                 // Constructor
void SetData(string data)
string GetData()
string GetJSONCertificate()
int GetCertificateSize()

// Utils (package-level functions)
string StringToHex(string s)
string HexToString(string hexStr)
string HexFix(string hexStr)
string PadNumber(int num)
string GetFormattedTimestamp()
```

**Essential Properties:**
```csharp
// CEPAccount properties (direct access like PHP)
public string Address { get; set; }
public string LastError { get; set; }              // Access directly, no GetLastError()
public string LatestTxID { get; set; }
public long Nonce { get; set; }
public string NAGURL { get; set; }
public string NetworkNode { get; set; }
public string Blockchain { get; set; }

// CCertificate properties (for advanced chaining)
public string PreviousTxID { get; set; }          // Direct property access
public string PreviousBlock { get; set; }         // Direct property access
```

### API Design Patterns from Reference Implementations

1. **Error Handling**: No exceptions thrown - errors stored in `LastError` property (PHP pattern)
2. **Instantiation**: Standard constructors `new CEPAccount()` (Node.js/PHP/Java pattern)
3. **Hex Encoding**: All blockchain data in hex format with automatic conversion
4. **Network Discovery**: Dynamic NAG URL resolution via HTTP calls
5. **Cryptographic Requirements**: ECDSA with secp256k1, SHA-256 hashing, RFC 6979 deterministic signatures

## Development Commands

### Building and Testing
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/CircularEnterpriseApis/

# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=CEPAccountTests"

# Run single test method
dotnet test --filter "MethodName=TestSetNetwork"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Project Structure
```
/
├── src/
│   └── CircularEnterpriseApis/          # Main library project
│       ├── CEPAccount.cs
│       ├── CCertificate.cs
│       ├── CircularEnterpriseApis.cs
│       ├── Common.cs
│       ├── Crypto/
│       └── Utils/
├── tests/
│   ├── CircularEnterpriseApis.UnitTests/
│   ├── CircularEnterpriseApis.IntegrationTests/
│   └── CircularEnterpriseApis.E2ETests/
├── examples/                            # Example usage project
└── CircularEnterpriseApis.sln
```

### Code Quality and Formatting
```bash
# Format code
dotnet format

# Static analysis
dotnet analyze

# Security scan
dotnet list package --vulnerable
```

### Package Management
```bash
# Restore packages
dotnet restore

# Add package
dotnet add package [PackageName]

# Update packages
dotnet outdated
```

## Architecture Requirements

### Namespace Organization
- **Main namespace**: `CircularEnterpriseApis`
- **Utilities**: `CircularEnterpriseApis.Utils`
- **Constants**: Keep global constants in main namespace to match Go package structure

### Required NuGet Dependencies
- **Cryptography**: `System.Security.Cryptography` or `BouncyCastle` for secp256k1 ECDSA
- **HTTP Client**: Built-in `HttpClient`
- **JSON**: `System.Text.Json` or `Newtonsoft.Json`
- **Environment**: `Microsoft.Extensions.Configuration` for `.env` file support

### Critical Implementation Constraints

1. **API Compatibility**: Method signatures must match Go implementation exactly
2. **Error Handling**: Use `LastError` property pattern, no thrown exceptions from public APIs
3. **Factory Methods**: Prefer `NewCEPAccount()` over constructors for consistency
4. **Hex Handling**: Automatic conversion between string and hex data
5. **Network Patterns**: Match exact HTTP request/response formats from Go version
6. **Property Names**: Case-sensitive matching with Go implementation where possible

### Constants (must match Go values exactly)
```csharp
public const string LibVersion = "1.0.13";
public const string DefaultChain = "0x8a20baa40c45dc5055aeb26197c203e576ef389d9acb171bd62da11dc5ad72b2";
public const string DefaultNAG = "https://nag.circularlabs.io/NAG.php?cep=";
public static string NetworkURL = "https://circularlabs.io/network/getNAG?network=";
```

### Testing Strategy

**Test Organization** (mirror Go structure):
- **Unit Tests**: Test individual methods and functions
- **Integration Tests**: Test network interactions with testnet
- **E2E Tests**: Complete certificate submission workflows

**Environment Setup** for tests:
```bash
# Copy environment template
cp .env.example .env
# Configure CIRCULAR_PRIVATE_KEY and CIRCULAR_ADDRESS in .env file
```

### JSON Request Formats (must match Go exactly)

**Certificate Submission**:
```json
{
  "ID": "transaction_hash",
  "From": "hex_address",
  "To": "hex_address",
  "Timestamp": "YYYY:MM:DD-HH:MM:SS",
  "Payload": "hex_encoded_certificate_data",
  "Nonce": "nonce_string",
  "Signature": "hex_ecdsa_signature",
  "Blockchain": "hex_blockchain_id",
  "Type": "C_TYPE_CERTIFICATE",
  "Version": "1.0.13"
}
```

## Development Workflow

1. **Reference Implementation Compatibility**: Check Circular Protocol's reference implementations (Node.js, PHP, Java) for exact API patterns and ergonomics
2. **Test-Driven Development**: Write tests that mirror reference implementation test patterns first
3. **API Compatibility**: Validate that C# API provides identical developer experience to other language implementations
4. **Network Testing**: Use testnet for integration testing to avoid mainnet costs
5. **Cryptographic Validation**: Ensure signatures are RFC 6979 compliant and match reference implementations

## Enhanced Features (Feature Branch)

The `feature/certificate-chaining` branch contains C#-specific enhancements not present in reference implementations:

- Certificate chaining methods (SetPreviousTxID, GetPreviousTxID, SetPreviousBlock, GetPreviousBlock)
- GetLastError() convenience method

These are maintained separately for developers who need advanced functionality beyond the standard API surface.