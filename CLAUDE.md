# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **C# implementation** of the Circular Protocol Enterprise APIs that must **STRICTLY adhere** to the API surface and ergonomics established by the reference Go implementation found in `@go-repomix-output.xml`. The goal is to provide identical developer experience so IDE autocompletion works exactly the same across both languages.

**Critical Requirement**: This C# implementation must honor the Go API surface exactly - same method names, parameter names, return types, and behavioral patterns.

## Reference Architecture (from Go Implementation)

### Core API Surface to Replicate

**Primary Classes:**
- `CEPAccount` - Main client interface for blockchain operations
- `CCertificate` - Certificate data structure and operations
- `Utils` - Utility functions for hex/string conversion

**Key Methods (must match exactly):**
```csharp
// CEPAccount
public static CEPAccount NewCEPAccount()
public bool Open(string address)
public string SetNetwork(string network)
public bool UpdateAccount()
public void SubmitCertificate(string pdata, string privateKeyHex)
public Dictionary<string, object> GetTransactionOutcome(string txID, int timeoutSec, int intervalSec)

// CCertificate
public static CCertificate NewCCertificate()
public void SetData(string data)
public string GetJSONCertificate()
public int GetCertificateSize()

// Utils
public static string StringToHex(string s)
public static string HexToString(string hexStr)
public static string HexFix(string hexStr)
public static string PadNumber(int num)
public static string GetFormattedTimestamp()
```

**Essential Properties:**
```csharp
// CEPAccount properties
public string Address { get; set; }
public string LastError { get; set; }
public string LatestTxID { get; set; }
public long Nonce { get; set; }
public string NAGURL { get; set; }
public string NetworkNode { get; set; }
public string Blockchain { get; set; }
```

### API Design Patterns from Go Reference

1. **Error Handling**: No exceptions thrown - errors stored in `LastError` property
2. **Factory Pattern**: Use static `New*()` methods instead of constructors
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
│   ├── CircularEnterpriseApis/          # Main library project
│   │   ├── CEPAccount.cs
│   │   ├── CCertificate.cs
│   │   ├── Common.cs
│   │   └── Utils/
│   │       └── Utils.cs
│   └── CircularEnterpriseApis.Examples/ # Example usage
├── tests/
│   ├── CircularEnterpriseApis.UnitTests/
│   ├── CircularEnterpriseApis.IntegrationTests/
│   └── CircularEnterpriseApis.E2ETests/
├── examples/
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
cp appsettings.example.json appsettings.Development.json
# Configure CIRCULAR_PRIVATE_KEY and CIRCULAR_ADDRESS
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

1. **Reference Go Implementation**: Always check `@go-repomix-output.xml` for exact API patterns
2. **Test-Driven Development**: Write tests that mirror Go test patterns first
3. **API Compatibility**: Validate that C# API provides identical IntelliSense experience
4. **Network Testing**: Use testnet for integration testing to avoid mainnet costs
5. **Cryptographic Validation**: Ensure signatures are RFC 6979 compliant and match Go output

## Context7 Integration

Use Context7 to reference Circular Protocol Enterprise API documentation when implementing blockchain-specific functionality:

```bash
# Get Circular Protocol documentation
context7 resolve-library-id "Circular Protocol"
context7 get-library-docs <library-id> --topic "enterprise apis"
```