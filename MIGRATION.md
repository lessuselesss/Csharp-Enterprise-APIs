# Migration Guide: v1.x → v2.0

This guide helps you migrate from the synchronous API (v1.x) to the fully async API (v2.0).

## Table of Contents

- [Overview](#overview)
- [Why Async/Await?](#why-asyncawait)
- [Breaking Changes Summary](#breaking-changes-summary)
- [Timeline and Support](#timeline-and-support)
- [Quick Start](#quick-start)
- [Method Mapping](#method-mapping)
- [Common Migration Patterns](#common-migration-patterns)
- [Troubleshooting](#troubleshooting)
- [Support](#support)

---

## Overview

### What's Changing?

Starting in **v1.2.0**, all synchronous methods are marked as obsolete and will show compiler warnings. These methods will be **completely removed in v2.0.0**.

**Why?** The synchronous methods use `.Result` which blocks threads and can cause deadlocks, especially in ASP.NET Core applications. The async/await pattern is the modern .NET best practice for I/O operations.

### What's Not Changing?

- All data structures (CEPAccount, CCertificate, etc.)
- Properties (Address, Nonce, LastError, etc.)
- Utility functions (StringToHex, HexToString, etc.)
- Core functionality and behavior

---

## Why Async/Await?

### Problems with Synchronous Methods

```csharp
// ❌ BAD: Blocks thread, can cause deadlocks
public bool UpdateAccount()
{
    var response = httpClient.PostAsync(url, content).Result;  // BLOCKS!
    // ...
}
```

**Issues:**
- **Thread Pool Starvation**: Blocks valuable thread pool threads
- **Deadlocks in ASP.NET Core**: Can hang your application
- **Poor Scalability**: Limits concurrent request handling
- **Not Aligned with .NET Best Practices**: Goes against modern .NET guidelines

### Benefits of Async/Await

```csharp
// ✅ GOOD: Non-blocking, scales better
public async Task<bool> UpdateAccountAsync()
{
    var response = await httpClient.PostAsync(url, content);  // NON-BLOCKING!
    // ...
}
```

**Benefits:**
- **Better Scalability**: Handles more concurrent operations
- **No Deadlocks**: Safe for ASP.NET Core and UI applications
- **Modern .NET**: Follows current best practices
- **Aligns with Rust Reference**: Matches reference implementation

---

## Breaking Changes Summary

### v1.2.0 (Current - Deprecation Warnings)

- All sync methods marked with `[Obsolete]`
- Compiler warnings guide you to async alternatives
- **Both sync and async methods work** - no code breaks
- Recommended: Start migrating now

### v2.0.0 (Future - Breaking Changes)

- All sync methods **removed**
- Only async methods available
- **Your code MUST be updated** to compile
- Support: v1.x receives security patches for 1 year

---

## Timeline and Support

| Version | Release | Status | Support |
|---------|---------|--------|---------|
| v1.0.14 | Current | ✅ Active | Internal changes only |
| v1.1.0 | Current | ✅ Active | Async methods added |
| v1.2.0 | Current | ✅ Active | Deprecation warnings |
| v1.x | - | ✅ Supported | Security patches for 1 year |
| v2.0.0 | Future | ⏳ Planned | Breaking changes |

**Recommendation:** Migrate to async during the v1.2.0 deprecation period to ensure a smooth transition.

---

## Quick Start

### Before (v1.x - Sync)

```csharp
using CircularEnterpriseApis;

var account = new CEPAccount();
account.Open(address);
account.SetNetwork("testnet");
account.UpdateAccount();
account.SubmitCertificate(data, privateKey);

var outcome = account.GetTransactionOutcome(txID, 30, 2);
```

### After (v2.0 - Async)

```csharp
using CircularEnterpriseApis;

var account = new CEPAccount();
account.Open(address);  // No async needed
await account.SetNetworkAsync("testnet");
await account.UpdateAccountAsync();
await account.SubmitCertificateAsync(data, privateKey);

var outcome = await account.GetTransactionOutcomeAsync(txID, 30, 2);
```

**Key Changes:**
1. Add `await` before async method calls
2. Add `Async` suffix to method names
3. Ensure calling method is `async` (return `Task` or `Task<T>`)

---

## Method Mapping

| v1.x (Sync) | v2.0 (Async) | Breaking |
|-------------|--------------|----------|
| `SetNetwork(network)` | `SetNetworkAsync(network)` | ✅ Yes |
| `UpdateAccount()` | `UpdateAccountAsync()` | ✅ Yes |
| `SubmitCertificate(pdata, key)` | `SubmitCertificateAsync(pdata, key)` | ✅ Yes |
| `GetTransaction(blockID, txID)` | `GetTransactionAsync(blockID, txID)` | ✅ Yes |
| `GetTransactionOutcome(txID, timeout, interval)` | `GetTransactionOutcomeAsync(txID, timeout, interval)` | ✅ Yes |
| `GetNAG(network)` | `GetNAGAsync(network)` | ✅ Yes |
| `GetNAGInternal(network)` | `GetNAGAsync(network)` | ✅ Yes |
| `Open(address)` | `Open(address)` | ❌ No change |
| `Close()` | `Close()` | ❌ No change |
| `SetBlockchain(chain)` | `SetBlockchain(chain)` | ❌ No change |
| **Properties** | **No changes** | ❌ All stay same |

---

## Common Migration Patterns

### Pattern 1: Console Applications

**Before:**
```csharp
class Program
{
    static void Main(string[] args)
    {
        var account = new CEPAccount();
        account.Open(address);
        account.SetNetwork("testnet");
        account.UpdateAccount();
    }
}
```

**After:**
```csharp
class Program
{
    static async Task Main(string[] args)  // Add 'async Task'
    {
        var account = new CEPAccount();
        account.Open(address);
        await account.SetNetworkAsync("testnet");  // Add 'await' and 'Async'
        await account.UpdateAccountAsync();  // Add 'await' and 'Async'
    }
}
```

---

### Pattern 2: ASP.NET Core Controllers

**Before:**
```csharp
public class CertificateController : ControllerBase
{
    [HttpPost]
    public IActionResult Submit([FromBody] CertificateRequest request)
    {
        var account = new CEPAccount();
        account.Open(request.Address);
        account.SetNetwork("testnet");
        account.UpdateAccount();
        account.SubmitCertificate(request.Data, request.PrivateKey);

        return Ok(new { txId = account.LatestTxID });
    }
}
```

**After:**
```csharp
public class CertificateController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CertificateRequest request)  // Add 'async Task<T>'
    {
        var account = new CEPAccount();
        account.Open(request.Address);
        await account.SetNetworkAsync("testnet");  // Add 'await' and 'Async'
        await account.UpdateAccountAsync();  // Add 'await' and 'Async'
        await account.SubmitCertificateAsync(request.Data, request.PrivateKey);  // Add 'await' and 'Async'

        return Ok(new { txId = account.LatestTxID });
    }
}
```

---

### Pattern 3: Background Services

**Before:**
```csharp
public class CertificatePollingService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>  // Awkward wrapper
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _account.UpdateAccount();
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }, stoppingToken);
    }
}
```

**After:**
```csharp
public class CertificatePollingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)  // Already async!
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _account.UpdateAccountAsync();  // Add 'await' and 'Async'
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);  // Use Task.Delay
        }
    }
}
```

---

### Pattern 4: Dependency Injection with Services

**Before:**
```csharp
public class CertificateService
{
    private readonly CEPAccount _account;

    public CertificateService(CEPAccount account)
    {
        _account = account;
    }

    public string SubmitCertificate(string data, string privateKey)
    {
        _account.UpdateAccount();
        _account.SubmitCertificate(data, privateKey);
        return _account.LatestTxID;
    }
}
```

**After:**
```csharp
public class CertificateService
{
    private readonly CEPAccount _account;

    public CertificateService(CEPAccount account)
    {
        _account = account;
    }

    public async Task<string> SubmitCertificateAsync(string data, string privateKey)  // Async all the way
    {
        await _account.UpdateAccountAsync();
        await _account.SubmitCertificateAsync(data, privateKey);
        return _account.LatestTxID;
    }
}
```

---

### Pattern 5: Unit Tests

**Before:**
```csharp
[Fact]
public void UpdateAccount_ShouldRetrieveNonce()
{
    var account = new CEPAccount();
    account.Open(testAddress);

    bool result = account.UpdateAccount();

    Assert.True(result);
    Assert.True(account.Nonce > 0);
}
```

**After:**
```csharp
[Fact]
public async Task UpdateAccountAsync_ShouldRetrieveNonce()  // Add 'async Task'
{
    var account = new CEPAccount();
    account.Open(testAddress);

    bool result = await account.UpdateAccountAsync();  // Add 'await' and 'Async'

    Assert.True(result);
    Assert.True(account.Nonce > 0);
}
```

---

## Troubleshooting

### Problem: "Cannot await in synchronous method"

**Error:**
```
CS4032: The 'await' operator can only be used within an async method.
```

**Solution:** Mark your method as `async`:
```csharp
// Before:
public void DoWork()

// After:
public async Task DoWork()
```

---

### Problem: "Dead code detected after await"

**Error:**
```
CS0162: Unreachable code detected
```

**Solution:** Don't use `.Result` or `.Wait()` with async methods:
```csharp
// ❌ WRONG:
var result = account.UpdateAccountAsync().Result;

// ✅ CORRECT:
var result = await account.UpdateAccountAsync();
```

---

### Problem: "This async method lacks 'await' operators"

**Warning:**
```
CS1998: This async method lacks 'await' operators and will run synchronously
```

**Solution:** Either:
1. Add `await` to async calls within the method
2. Remove `async` if no async operations exist

```csharp
// ❌ WRONG:
public async Task DoWork()
{
    _account.Close();  // No async calls
}

// ✅ CORRECT:
public void DoWork()  // Remove 'async'
{
    _account.Close();
}
```

---

### Problem: Deadlocks in ASP.NET Core

**Symptom:** Application hangs when calling async methods.

**Cause:** Mixing `.Result` or `.Wait()` with async code.

**Solution:** Use `await` all the way:
```csharp
// ❌ WRONG: Causes deadlock
public IActionResult Submit()
{
    _account.UpdateAccountAsync().Wait();  // DEADLOCK!
}

// ✅ CORRECT: Fully async
public async Task<IActionResult> Submit()
{
    await _account.UpdateAccountAsync();
}
```

---

### Problem: "Cannot implicitly convert Task<T> to T"

**Error:**
```
CS0029: Cannot implicitly convert type 'System.Threading.Tasks.Task<bool>' to 'bool'
```

**Solution:** Add `await`:
```csharp
// ❌ WRONG:
bool result = account.UpdateAccountAsync();

// ✅ CORRECT:
bool result = await account.UpdateAccountAsync();
```

---

## Best Practices

### 1. Async All the Way

Once you start using async, use it consistently throughout your call stack:

```csharp
// ✅ GOOD: Async all the way up
public async Task<IActionResult> Controller() => await Service();
private async Task<Result> Service() => await Repository();
private async Task<Result> Repository() => await account.UpdateAccountAsync();
```

### 2. Don't Block on Async Code

```csharp
// ❌ BAD: Blocks thread
var result = account.UpdateAccountAsync().Result;

// ❌ BAD: Blocks thread
account.UpdateAccountAsync().Wait();

// ✅ GOOD: Non-blocking
var result = await account.UpdateAccountAsync();
```

### 3. Use ConfigureAwait(false) in Libraries

If you're writing a library (not an application):

```csharp
// For library code:
var result = await account.UpdateAccountAsync().ConfigureAwait(false);
```

This improves performance by avoiding unnecessary context switching.

### 4. Return Task Directly When Possible

```csharp
// ✅ BETTER: No unnecessary async/await
public Task<bool> UpdateAccountWrapper()
{
    return _account.UpdateAccountAsync();
}

// Less efficient:
public async Task<bool> UpdateAccountWrapper()
{
    return await _account.UpdateAccountAsync();
}
```

---

## Support

### Getting Help

- **GitHub Issues**: https://github.com/lessuselesss/Csharp-Enterprise-APIs/issues
- **GitHub Discussions**: https://github.com/lessuselesss/Csharp-Enterprise-APIs/discussions
- **Stack Overflow**: Tag `circular-protocol` + `c#`

### Reporting Migration Issues

When reporting issues, please include:
1. Your current version (v1.x)
2. Target version (v2.0)
3. Code snippet showing the problem
4. Error message or unexpected behavior
5. Your environment (.NET version, OS, etc.)

---

## Additional Resources

- **Official Documentation**: https://docs.microsoft.com/en-us/dotnet/csharp/async
- **Async/Await Best Practices**: https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming
- **Common Async Mistakes**: https://devblogs.microsoft.com/pfxteam/

---

## Summary Checklist

Before upgrading to v2.0, ensure you've:

- [ ] Reviewed all compiler warnings from v1.2.0
- [ ] Updated all sync method calls to async equivalents
- [ ] Added `async`/`await` throughout your call stack
- [ ] Changed method return types to `Task` or `Task<T>`
- [ ] Updated your tests to be async
- [ ] Tested your application thoroughly
- [ ] Reviewed this migration guide
- [ ] Removed any `.Result` or `.Wait()` calls

**Ready to upgrade?** You're all set for v2.0! 🎉
