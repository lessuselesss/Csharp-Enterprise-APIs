# Circular Enterprise APIs - C# Implementation

## Overview
This is the official C# implementation of the Circular Protocol Enterprise APIs for data certification. The repository provides tools for blockchain interaction, account management, and certificate handling.

## Key Features
The library supports account management with blockchain interaction, certificate creation and submission capabilities, transaction tracking and verification, and secure digital signatures using ECDSA (secp256k1) with RFC 6979 compliant deterministic signatures.

## Technical Requirements
- .NET 6.0 or higher
- NuGet package manager

## Dependencies
The project relies on BouncyCastle.Cryptography for cryptographic operations (secp256k1), System.Text.Json for JSON processing, and System.Net.Http for network requests.

## Installation
Install via NuGet Package Manager:

```bash
dotnet add package CircularEnterpriseApis
```

Or clone the repository and build using:

```bash
dotnet build
```

## Main API Classes

**CEPAccount Class** provides core blockchain functionality including:
- Opening accounts with `Open(string address)`
- Closing accounts via `Close()`
- Network configuration through `SetNetwork(string network)`
- Blockchain identifier setting with `SetBlockchain(string chain)`
- Data signing with `SignData(string data, string privateKeyHex)`
- Account updates using `UpdateAccount()`
- Certificate submission through `SubmitCertificate(string pdata, string privateKeyHex)`
- Transaction retrieval with `GetTransaction(string blockID, string transactionID)`
- Transaction outcome polling via `GetTransactionOutcome(string txID, int timeoutSec, int intervalSec)`

**CCertificate Class** manages certificate operations:
- Setting certificate data with `SetData(string data)`
- Retrieving data via `GetData()`
- JSON format export through `GetJSONCertificate()`
- Size calculation with `GetCertificateSize()`

**Properties:** The CCertificate class includes `PreviousTxID` and `PreviousBlock` properties for certificate chaining support.

## Usage Examples

See `examples/SimpleCertificateSubmission.cs` for a complete example of certificate submission. Run the example with:

```bash
dotnet run --project examples
```

Basic usage pattern:

```csharp
using CircularEnterpriseApis;

// Create and configure account
var account = new CEPAccount();
account.Open("0xYourWalletAddress");
account.SetNetwork("testnet");
account.UpdateAccount();

// Create and submit certificate
var certificate = new CCertificate();
certificate.SetData("Your data to certify");
account.SubmitCertificate(certificate.GetJSONCertificate(), "your_private_key_hex");

// Get transaction outcome
var outcome = account.GetTransactionOutcome(account.LatestTxID, 30, 5);
```

## Testing Setup
Tests require environment variables: `CIRCULAR_PRIVATE_KEY` (64-character hex string) and `CIRCULAR_ADDRESS` (Ethereum-style address with 0x prefix). Set these in your environment or create a `.env` file:

```bash
cp .env.example .env
# Edit .env with your credentials
```

Run tests with:

```bash
dotnet test
```

## Building
Package the project using:

```bash
dotnet pack
```

## Licensing & Attribution
Licensed under MIT. Created by Circular Global Ledgers, Inc. (USA), with original JavaScript version by Gianluca De Novi, PhD, and C# implementation by Danny De Novi.