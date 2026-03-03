# Method Comparison

## At a Glance

| | METHOD 1: Entra ID | METHOD 2: Connection String |
|---|---|---|
| **Secrets in code/env** | ✅ None | ❌ Account key |
| **Credential rotation** | ✅ Not needed | ❌ Required |
| **Access scope** | ✅ Least privilege (IAM) | ❌ Full account |
| **Local dev** | ✅ `az login` | ✅ Copy-paste string |
| **Azure hosted** | ✅ Managed Identity | ✅ App Settings |
| **Production ready** | ✅ Yes | ⚠️ Use with caution |
| **Audit trail** | ✅ Azure AD logs | ⚠️ Limited |

---

## How the Code Decides — One Ternary

```csharp
// ── 2. Choose authentication method ──────────────────────────────────────
//    AccountName set  →  Entra ID  (no secrets, works locally & in Azure)
//    ConnectionString →  Account key  (fallback, less secure)
var container = options.AccountName is { } account
    ? new BlobContainerClient(new Uri($"https://{account}.blob.core.windows.net/{options.ContainerName}"), new DefaultAzureCredential())
    : new BlobContainerClient(options.ConnectionString, options.ContainerName);
```

Set `AZURE_STORAGE_ACCOUNT_NAME` → Method 1 runs.  
Leave it unset, set `AZURE_STORAGE_CONNECTION_STRING` → Method 2 runs.

---

## DefaultAzureCredential Chain

```
1. EnvironmentCredential
   ↓
2. ManagedIdentityCredential   ← kicks in when deployed to Azure
   ↓
3. VisualStudioCredential
   ↓
4. AzureCliCredential          ← kicks in locally after az login
   ↓
5. InteractiveBrowserCredential
```

---

## Use Method 1 When

- Building for production
- Running in Azure (App Service, Functions, Containers)
- Team project — no shared secrets
- Need per-user audit logs

## Use Method 2 When

- Quick local testing
- External users without Azure AD access
- Migrating a legacy integration (temporary)
