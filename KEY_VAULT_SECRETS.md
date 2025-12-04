# Azure Key Vault Secrets Summary

## Quick Reference: What to Store in Azure Key Vault

### 📌 Secret 1: Database Connection String
**Name:** `DatabaseConnectionString`  
**Type:** String  
**Value Example:**
```
Server=tcp:m3hkanamofunctiondb.database.windows.net,1433;Initial Catalog=m3h-kanamo-functionDB;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

**Replace with your actual values:**
- `m3hkanamofunctiondb` → Your SQL Server name
- `m3h-kanamo-functionDB` → Your database name
- `your-username` → Your SQL username
- `your-password` → Your SQL password

---

## How to Store in Key Vault

### Via Azure Portal
1. Open **Azure Portal** → search "Key Vaults"
2. Click on **m3h-keyvault** (or your Key Vault name)
3. In left menu, click **Secrets**
4. Click **+ Generate/Import**
5. Fill in:
   - **Name:** `DatabaseConnectionString`
   - **Value:** (Paste your connection string)
6. Click **Create**

### Via Azure CLI
```bash
az keyvault secret set \
  --vault-name m3h-keyvault \
  --name DatabaseConnectionString \
  --value "Server=tcp:m3hkanamofunctiondb.database.windows.net,1433;Initial Catalog=m3h-kanamo-functionDB;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### Via PowerShell
```powershell
$secretvalue = ConvertTo-SecureString "Server=tcp:..." -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName "m3h-keyvault" -Name "DatabaseConnectionString" -SecretValue $secretvalue
```

---

## Verify Secret Was Created

### Via Azure CLI
```bash
az keyvault secret show --vault-name m3h-keyvault --name DatabaseConnectionString
```

**Expected Output:**
```json
{
  "attributes": {
    "created": "2025-11-28T...",
    "enabled": true,
    "expires": null,
    "notBefore": null,
    "recoveryLevel": "Recoverable+Purgeable",
    "updated": "2025-11-28T..."
  },
  "contentType": null,
  "id": "https://m3h-keyvault.vault.azure.net/secrets/DatabaseConnectionString/...",
  "name": "DatabaseConnectionString",
  "value": "Server=tcp:m3hkanamofunctiondb.database.windows.net,1433;..."
}
```

---

## Configure Function App to Use Key Vault

### Step 1: Enable Managed Identity
```bash
az functionapp identity assign \
  --name m3h-beerkn-functionapp \
  --resource-group your-resource-group
```

### Step 2: Grant Access to Key Vault
```bash
# Get Managed Identity Principal ID
$principalId = az functionapp identity show \
  --name m3h-beerkn-functionapp \
  --resource-group your-resource-group \
  --query principalId -o tsv

# Set Key Vault Access Policy
az keyvault set-policy \
  --name m3h-keyvault \
  --object-id $principalId \
  --secret-permissions get list
```

### Step 3: Add App Settings to Function App
```bash
az functionapp config appsettings set \
  --name m3h-beerkn-functionapp \
  --resource-group your-resource-group \
  --settings KeyVaultUri="https://m3h-keyvault.vault.azure.net/"
```

---

## Backend Code Integration

The updated backend code automatically retrieves the connection string from Key Vault:

```csharp
private static async Task<string> GetConnectionStringFromKeyVault()
{
    try
    {
        var client = new SecretClient(
            new Uri(keyVaultUrl), 
            new DefaultAzureCredential()
        );
        KeyVaultSecret secret = await client.GetSecretAsync("DatabaseConnectionString");
        return secret.Value;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            $"Failed to retrieve connection string from Key Vault: {ex.Message}", 
            ex
        );
    }
}
```

Then in each function:
```csharp
string connectionString = await GetConnectionStringFromKeyVault();
using (SqlConnection conn = new SqlConnection(connectionString))
{
    // Database operations...
}
```

---

## Files to Replace/Update

| File | Action | Notes |
|------|--------|-------|
| `Function1.cs` | Replace with `Function1_Updated.cs` | Contains Key Vault integration |
| `FunctionbeerAPI.csproj` | Already updated | Added Azure.Identity & Azure.Security.KeyVault.Secrets NuGet packages |

---

## Security Benefits

✅ **No hardcoded credentials** in source code  
✅ **Managed Identity** - No credentials stored in app settings  
✅ **Azure audit logs** - Track who accessed secrets and when  
✅ **Encryption at rest** - Secrets encrypted in Key Vault  
✅ **Access control** - Only authorized services can read secrets  
✅ **Secret rotation** - Easy to update without redeploying code  

---

## Testing the Integration

### Local Testing (Requires Azure Login)
```bash
# Login to Azure
az login

# Run locally
dotnet run
```

### After Deployment
```bash
# Check Function App logs
az functionapp log tail \
  --name m3h-beerkn-functionapp \
  --resource-group your-resource-group

# Test endpoint
curl -X GET https://nomikai-funcapp.azurewebsites.net/api/nomikai/search?eventdate=2025-11-28
```

---

## Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| "403 Forbidden" from Key Vault | Function App doesn't have permission | Re-run Step 2 (Grant Access) |
| "404 Secret Not Found" | Wrong secret name | Verify secret name is exactly `DatabaseConnectionString` |
| "Connection timeout" | SQL credentials invalid | Verify connection string credentials match SQL Server |
| "Managed Identity not found" | Identity not enabled | Run: `az functionapp identity assign --name nomikai-funcapp --resource-group your-resource-group` |

---

## Next Steps

1. ✅ Create secrets in Key Vault (see steps above)
2. ✅ Enable Managed Identity on Function App
3. ✅ Grant access policies
4. ✅ Add App Settings
5. ✅ Replace `Function1.cs` with `Function1_Updated.cs`
6. ✅ Update NuGet packages in `.csproj`
7. ✅ Deploy backend to Azure
8. ✅ Test endpoints
9. ✅ Update frontend with environment variables (see `FRONTEND_ENV_CONFIG.md`)

