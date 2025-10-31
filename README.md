# Circular Enterprise APIs - C# Implementation

## Overview
This is the official C# implementation of the Circular Protocol Enterprise APIs for data certification on blockchain. The repository provides an **async-only API** for blockchain interaction, account management, and certificate handling, aligned with the Rust reference implementation at version **1.0.13**.

## Key Features
The library supports:
- **Async-first API design** - all I/O operations use async/await patterns for optimal performance
- **Account management** with blockchain interaction
- **Certificate creation and submission** to Circular Protocol blockchain
- **Transaction tracking and verification** with configurable polling
- **Secure digital signatures** using ECDSA (secp256k1) with RFC 6979 compliant deterministic signatures
- **DER signature encoding** matching Rust reference implementation

## Technical Requirements
- **.NET 6.0 or higher** (targets .NET Standard 2.1)
- NuGet package manager
- **Testnet or mainnet credentials** for blockchain operations

## Dependencies
The project relies on:
- **BouncyCastle.Cryptography** - cryptographic operations (secp256k1 ECDSA)
- **System.Text.Json** - JSON serialization/deserialization
- **System.Net.Http** - asynchronous network requests

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

### CEPAccount Class
Core blockchain client providing async operations:

**Synchronous Setup Methods:**
- `Open(string address)` - Initialize account with blockchain address
- `Close()` - Securely clear account data
- `SetBlockchain(string chain)` - Set blockchain identifier

**Async I/O Operations** (all methods return Task):
- `SetNetworkAsync(string network)` - Configure network (testnet/mainnet) with NAG discovery
- `UpdateAccountAsync()` - Refresh account state and retrieve current nonce from blockchain
- `SubmitCertificateAsync(string certificateJson, string privateKeyHex)` - Submit certificate to blockchain
- `GetTransactionAsync(string blockID, string transactionID)` - Retrieve specific transaction details
- `GetTransactionOutcomeAsync(string txID, int timeoutSec, int intervalSec)` - Poll for transaction confirmation

**Key Properties:**
- `Address` - Account blockchain address
- `LatestTxID` - Most recent transaction ID
- `Nonce` - Current transaction counter
- `LastError` - Last error message (null if no error)
- `NAGURL` - Network Access Gateway URL
- `Blockchain` - Current blockchain identifier

### CCertificate Class
Certificate data structure and operations:

**Methods:**
- `SetData(string data)` - Set certificate data (automatically converts to hex)
- `GetData()` - Retrieve original certificate data (converts from hex)
- `GetJSONCertificate()` - Export certificate as JSON string
- `GetCertificateSize()` - Calculate certificate size in bytes

**Properties:**
- `Data` - Certificate payload (hex-encoded)
- `PreviousTxID` - Previous transaction ID for chaining
- `PreviousBlock` - Previous block for chaining
- `Version` - Certificate format version (currently 1.0.13)

## Usage Examples

See `examples/SimpleCertificateSubmission.cs` for a complete example of certificate submission. Run the example with:

```bash
dotnet run --project examples
```

Basic usage pattern (async):

```csharp
using CircularEnterpriseApis;
using System.Threading.Tasks;

public async Task CertifyDataAsync()
{
    // Create and configure account
    var account = new CEPAccount();
    account.Open("0xYourWalletAddress");
    account.SetBlockchain(Constants.DefaultChain);

    // Set network and update account (async operations)
    await account.SetNetworkAsync("testnet");
    await account.UpdateAccountAsync();

    // Create and submit certificate
    var certificate = new CCertificate();
    certificate.SetData("Your data to certify");
    await account.SubmitCertificateAsync(
        certificate.GetJSONCertificate(),
        "your_private_key_hex"
    );

    // Poll for transaction outcome (30 second timeout, 2 second intervals)
    var outcome = await account.GetTransactionOutcomeAsync(
        account.LatestTxID,
        timeoutSec: 30,
        intervalSec: 2
    );

    if (outcome != null)
    {
        Console.WriteLine($"Transaction confirmed: {outcome["Status"]}");
    }
    else
    {
        Console.WriteLine($"Error: {account.LastError}");
    }
}
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