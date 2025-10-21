# TODO: C# Implementation API Alignment

This document outlines the changes needed to align the C# implementation with the Rust reference implementation and other official language ports (Go, C++).

## Critical Issues (Breaking Changes - Must Fix)

### 1. Convert Synchronous Methods to Async/Await ❌ CRITICAL

**Problem:** Methods that perform network I/O are synchronous and block threads using `.Result`, violating .NET async best practices.

**Current Implementation:**
```csharp
public bool UpdateAccount()
{
    var response = httpClient.PostAsync(url, content).Result;  // BLOCKS THREAD!
    // ...
}

public string SetNetwork(string network)
{
    var response = httpClient.GetAsync(url).Result;  // BLOCKS THREAD!
    // ...
}

public void SubmitCertificate(string pdata, string privateKeyHex)
{
    var response = httpClient.PostAsync(url, content).Result;  // BLOCKS THREAD!
    // ...
}
```

**Required Changes:**
```csharp
public async Task<bool> UpdateAccountAsync()
{
    var response = await httpClient.PostAsync(url, content);
    // ...
}

public async Task<string> SetNetworkAsync(string network)
{
    var response = await httpClient.GetAsync(url);
    // ...
}

public async Task SubmitCertificateAsync(string pdata, string privateKeyHex)
{
    var response = await httpClient.PostAsync(url, content);
    // ...
}
```

**Impact:**
- ✅ Prevents thread pool starvation
- ✅ Safe for ASP.NET Core usage
- ✅ Follows .NET async naming conventions (Async suffix)
- ✅ Allows proper async/await composition
- ⚠️ Breaking change: All callers must be updated to use `await`

**Files to Update:**
- `src/CircularEnterpriseApis/CEPAccount.cs`
- All calling code in examples and tests

---

### 2. Remove or Make `SignData()` Internal ❌ CRITICAL

**Problem:** `CEPAccount.SignData()` is public but not present in Rust/Go reference implementations.

**Current Implementation:**
```csharp
public string SignData(string data, string privateKeyHex)
{
    // Signing logic
}
```

**Option 1 (Recommended): Make Internal**
```csharp
internal string SignData(string data, string privateKeyHex)
{
    // Signing logic - only used internally by SubmitCertificate
}
```

**Option 2: Remove Entirely**
- Move signing logic inline into `SubmitCertificate()`
- Matches Rust implementation pattern

**Rationale:**
- Rust reference: `sign_data()` is **private** (`fn sign_data(&self, ...)`)
- Go reference: `signData()` is **private** (`func (a *CEPAccount) signData(...)`)
- C++: `sign_data()` is **private**
- Signing should be an internal operation, not exposed to API consumers

**Files to Update:**
- `src/CircularEnterpriseApis/CEPAccount.cs`

---

### 3. Make CryptoUtils Internal ❌ CRITICAL

**Problem:** `CryptoUtils` class exposes 9 low-level cryptographic methods publicly that are not present in reference implementations.

**Current Implementation:**
```csharp
public static class CryptoUtils
{
    public static string SignMessage(string privateKeyHex, string message)
    public static string GetPublicKeyFromPrivateKey(string privateKeyHex)
    public static bool VerifySignature(string publicKeyHex, string message, string signatureHex)
    // ... 6 more public methods
}
```

**Required Change:**
```csharp
internal static class CryptoUtils  // Make internal
{
    internal static string SignMessage(string privateKeyHex, string message)
    internal static string GetPublicKeyFromPrivateKey(string privateKeyHex)
    internal static bool VerifySignature(string publicKeyHex, string message, string signatureHex)
    // ... 6 more internal methods
}
```

**Rationale:**
- Rust reference: Crypto operations are **private** implementation details
- Go reference: Crypto operations are **not exposed** as utilities
- C++: Crypto operations are **private** methods
- Exposing low-level crypto increases attack surface
- Users might misuse primitives and create security vulnerabilities

**Files to Update:**
- `src/CircularEnterpriseApis/Crypto/CryptoUtils.cs`

---

## Medium Priority Issues (API Surface Alignment)

### 4. Add CCertificate Methods for Cross-Language Compatibility ⚠️

**Problem:** C# uses properties for `PreviousTxID` and `PreviousBlock`, while Rust/Go use getter/setter methods.

**Current C# (Property-Based):**
```csharp
public class CCertificate
{
    public string PreviousTxID { get; set; }
    public string PreviousBlock { get; set; }
}

// Usage:
cert.PreviousTxID = "0x123...";
var txId = cert.PreviousTxID;
```

**Rust Reference (Method-Based):**
```rust
impl CCertificate {
    pub fn set_previous_tx_id(&mut self, tx_id: &str)
    pub fn get_previous_tx_id(&self) -> String
    pub fn set_previous_block(&mut self, block: &str)
    pub fn get_previous_block(&self) -> String
}
```

**Recommended Solution (Keep Both):**
```csharp
public class CCertificate
{
    // Keep properties for C# developers
    public string PreviousTxID { get; set; }
    public string PreviousBlock { get; set; }

    // Add methods for cross-language API compatibility
    public void SetPreviousTxId(string txId) => PreviousTxID = txId;
    public string GetPreviousTxId() => PreviousTxID;

    public void SetPreviousBlock(string block) => PreviousBlock = block;
    public string GetPreviousBlock() => PreviousBlock;
}
```

**Rationale:**
- Properties are more idiomatic for C#
- Methods provide API compatibility with Rust/Go
- Both patterns can coexist without conflict
- Zero performance impact (methods are thin wrappers)

**Files to Update:**
- `src/CircularEnterpriseApis/CCertificate.cs`

---

### 5. Add GetLastError() Method ⚠️

**Problem:** C# uses a `LastError` property, while Rust/Go use a `get_last_error()` method returning `Option<String>`.

**Current Implementation:**
```csharp
public string LastError { get; set; }  // Empty string = no error
```

**Rust Reference:**
```rust
pub fn get_last_error(&self) -> Option<String>  // None = no error
```

**Recommended Solution:**
```csharp
// Keep property for C# developers
public string? LastError { get; set; } = null;  // Make nullable

// Add method for cross-language API compatibility
public string? GetLastError() => string.IsNullOrEmpty(LastError) ? null : LastError;
```

**Rationale:**
- Null properly represents "no error" in C#
- Method provides API compatibility with Rust/Go
- Both patterns can coexist

**Files to Update:**
- `src/CircularEnterpriseApis/CEPAccount.cs`

---

## Low Priority Issues (Code Quality)

### 6. Fix Nullable Reference Type Annotations ⚠️

**Problem:** Inconsistent use of nullable reference types.

**Changes Needed:**
```csharp
// Current:
public string LastError { get; set; }  // Should be string?

// Fixed:
public string? LastError { get; set; } = null;

// Current:
public object? Info { get; set; }  // Correct, but inconsistent with others

// Review all return types:
public Dictionary<string, object>? GetTransaction(...)  // Already nullable ✓
```

**Files to Update:**
- `src/CircularEnterpriseApis/CEPAccount.cs`

---

### 7. Remove Unused Ethereum Address Method ⚠️

**Problem:** `CryptoUtils.GetEthereumAddress()` is not used anywhere and not part of the API spec.

**Action:**
```csharp
// Remove:
public static string GetEthereumAddress(ECPublicKeyParameters publicKey)
```

**Files to Update:**
- `src/CircularEnterpriseApis/Crypto/CryptoUtils.cs`

---

## Documentation Updates

### 8. Update README with Async/Await Examples

After converting to async, update all examples:

```csharp
// Old:
var account = new CEPAccount();
account.UpdateAccount();

// New:
var account = new CEPAccount();
await account.UpdateAccountAsync();
```

**Files to Update:**
- `README.md`
- `examples/SimpleCertificateSubmission.cs`
- All example files

---

### 9. Add Migration Guide for Breaking Changes

Create a `MIGRATION.md` document explaining:
- How to update from sync to async methods
- Why `SignData()` was made internal
- Property vs method usage for CCertificate

---

## Testing Requirements

### 10. Update All Tests for Async

**Files to Update:**
- `tests/CircularEnterpriseApis.UnitTests/**/*.cs`
- `tests/CircularEnterpriseApis.IntegrationTests/**/*.cs`
- `tests/CircularEnterpriseApis.E2ETests/**/*.cs`

**Pattern:**
```csharp
// Old:
[Fact]
public void UpdateAccount_ShouldWork()
{
    account.UpdateAccount();
}

// New:
[Fact]
public async Task UpdateAccountAsync_ShouldWork()
{
    await account.UpdateAccountAsync();
}
```

---

## Implementation Checklist

### Phase 1: Critical Fixes (Breaking Changes)
- [ ] Convert `UpdateAccount()` → `UpdateAccountAsync()`
- [ ] Convert `SetNetwork()` → `SetNetworkAsync()`
- [ ] Convert `SubmitCertificate()` → `SubmitCertificateAsync()`
- [ ] Convert `GetTransactionOutcome()` → `GetTransactionOutcomeAsync()`
- [ ] Make `CEPAccount.SignData()` internal or remove
- [ ] Make `CryptoUtils` class internal
- [ ] Update all async methods to use `await` instead of `.Result`
- [ ] Update all tests to use async/await
- [ ] Update all examples to use async/await

### Phase 2: API Compatibility
- [ ] Add `SetPreviousTxId()` / `GetPreviousTxId()` to CCertificate
- [ ] Add `SetPreviousBlock()` / `GetPreviousBlock()` to CCertificate
- [ ] Add `GetLastError()` method to CEPAccount
- [ ] Fix nullable annotations on `LastError` property

### Phase 3: Cleanup
- [ ] Remove `GetEthereumAddress()` method
- [ ] Review and fix all nullable reference type annotations
- [ ] Update README.md with async examples
- [ ] Create MIGRATION.md guide

### Phase 4: Documentation
- [ ] Update XML doc comments for async methods
- [ ] Add remarks about internal crypto methods
- [ ] Document property vs method access patterns
- [ ] Add async/await best practices section to README

---

## API Surface Summary

After these changes, the C# implementation will have:

| Component | Count | Notes |
|-----------|-------|-------|
| **Utilities (Utils)** | 6 | ✅ Matches Rust/Go/C++ |
| **CryptoUtils** | 9 | ✅ Internal only (not public API) |
| **Account Methods** | 12 async | ✅ Matches Rust/Go/C++ |
| **Certificate Methods** | 9 | ✅ 4 methods + 4 properties + 4 compat methods |

**Total Public API:** 27 methods (matching Rust reference exactly)

---

## Reference Implementations

Compare against:
- **Rust (reference)**: https://github.com/lessuselesss/Rust-CEP-APIs
- **Go**: https://github.com/lessuselesss/Go-CEP-APIs
- **C++**: https://github.com/lessuselesss/Cpp-Enterprise-APIs

All implementations should provide the same functionality with language-appropriate idioms.

---

## Notes

- **Property vs Method Trade-off**: C# properties are more idiomatic than getter/setter methods. The hybrid approach (keep properties, add methods) provides both C# ergonomics and cross-language compatibility.

- **Async/Await is Non-Negotiable**: Blocking async operations with `.Result` is an anti-pattern in .NET and can cause deadlocks in ASP.NET Core. This must be fixed.

- **Breaking Changes Required**: The async conversion is a breaking change, but necessary for correctness. Consider versioning: bump to v2.0.0.

- **Security Consideration**: Making CryptoUtils internal reduces attack surface and prevents users from misusing cryptographic primitives.
