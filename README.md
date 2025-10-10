# Circular Enterprise APIs - C# Implementation

Official Circular Protocol Enterprise APIs for Data Certification - C# Implementation

## Features

- Account management and blockchain interaction
- Certificate creation and submission
- Transaction tracking and verification
- Secure digital signatures using ECDSA (secp256k1)
- RFC 6979 compliant deterministic signatures

## Requirements

- .NET 6.0 or higher

## Dependencies

- `System.Security.Cryptography` for secp256k1 elliptic curve operations
- `System.Text.Json` for JSON serialization
- `System.Net.Http` for network requests

## Installation

To use this library in your project, you can use `dotnet add package`:

```bash
dotnet add package CircularEnterpriseApis
```

## Usage Example

See `examples/Program.cs` for a basic example of how to use the API to submit a certificate. You can run it with:

```bash
dotnet run --project examples
```

A more detailed example can be found in `examples/SimpleCertificateSubmission.cs`.

## API Documentation

### CEPAccount Class

Main class for interacting with the Circular blockchain:

- `NewCEPAccount() CEPAccount` - Factory function to create a new `CEPAccount` instance.
- `Open(string address) bool` - Initializes the account with a specified blockchain address.
- `Close()` - Clears all sensitive and operational data from the account.
- `SetNetwork(string network) string` - Configures the account to operate on a specific blockchain network.
- `SetBlockchain(string chain)` - Explicitly sets the blockchain identifier for the account.
- `UpdateAccount() bool` - Fetches the latest nonce for the account from the NAG.
- `SubmitCertificate(string pdata, string privateKeyHex)` - Creates, signs, and submits a data certificate to the blockchain.
- `GetTransaction(string blockID, string transactionID) Dictionary<string, object>` - Retrieves transaction details by block and transaction ID.
- `GetTransactionOutcome(string txID, int timeoutSec, int intervalSec) Dictionary<string, object>` - Polls for the final status of a transaction.
- `GetLastError() string` - Retrieves the last error message.

### CCertificate Class

Class for managing certificates:

- `NewCCertificate() CCertificate` - Factory function to create a new `CCertificate` instance.
- `SetData(string data)` - Sets the primary data content of the certificate.
- `GetData() string` - Retrieves the primary data content from the certificate.
- `GetJSONCertificate() string` - Serializes the certificate object into a JSON string.
- `GetCertificateSize() int` - Calculates the size of the JSON-serialized certificate in bytes.
- `SetPreviousTxID(string txID)` - Sets the transaction ID of the preceding certificate.
- `SetPreviousBlock(string block)` - Sets the block identifier of the preceding certificate.
- `GetPreviousTxID() string` - Retrieves the transaction ID of the preceding certificate.
- `GetPreviousBlock() string` - Retrieves the block identifier of the preceding certificate.

## Testing

To run the tests, you need to set up a `.env` file in the project root. You can copy the `.env.example` file to get started:

```bash
cp .env.example .env
```

Then, edit the `.env` file with your credentials:

```
CIRCULAR_PRIVATE_KEY="your_64_character_private_key_here"
CIRCULAR_ADDRESS="your_wallet_address_here"
```

The private key should be a 64-character (32-byte) hex string, and the address should be a valid Ethereum-style address (40 characters + 0x prefix).

### Running Tests

```bash
dotnet test
```

## License

MIT License - see LICENSE file for details

## Credits

CIRCULAR GLOBAL LEDGERS, INC. - USA

- Original JS Version: Gianluca De Novi, PhD
- Go Implementation: Danny De Novi