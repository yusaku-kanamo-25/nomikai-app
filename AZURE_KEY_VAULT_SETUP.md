# Azure Key Vault Setup Guide

## Secrets to Store in Azure Key Vault

Store the following secrets in your Azure Key Vault. Replace the placeholder values with your actual credentials.

### 1. Database Connection String
**Secret Name:** `DatabaseConnectionString`
```
Server=tcp:m3hkanamofunctiondb.database.windows.net,1433;Initial Catalog=m3h-kanamo-functionDB;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### 2. (Optional) API Keys for Frontend
**Secret Name:** `ApiUrl`
```
https://nomikai-funcapp.azurewebsites.net
```

---

## Azure Key Vault Details

**Key Vault Name:** `m3h-keyvault` (or your preferred name)

**Region:** Same region as your Function App

**Access Control:** Assign the Function App's Managed Identity access to the Key Vault

---

## How to Create These Secrets via Azure Portal

1. **Navigate to Key Vault**
   - Search for "Key Vaults" in Azure Portal
   - Click on your Key Vault

2. **Add a Secret**
   - In the left menu, click **Secrets**
   - Click **+ Generate/Import**
   - Name: `DatabaseConnectionString`
   - Value: Your full connection string
   - Click **Create**

3. **Repeat for other secrets**

---

## How to Create via Azure CLI

```bash
# Login to Azure
az login

# Create Key Vault (if not exists)
az keyvault create --name m3h-keyvault --resource-group your-resource-group --location eastus

# Set the connection string secret
az keyvault secret set --vault-name m3h-keyvault --name DatabaseConnectionString --value "Server=tcp:m3hkanamofunctiondb.database.windows.net,1433;Initial Catalog=m3h-kanamo-functionDB;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Retrieve the secret to verify
az keyvault secret show --vault-name m3h-keyvault --name DatabaseConnectionString
```

---

## Function App Configuration

### 1. Enable Managed Identity
- Go to your Function App (nomikai-funcapp)
- In the left menu, click **Identity**
- Enable **System assigned** managed identity
- Click **Save**

### 2. Grant Key Vault Access to Function App
- Go to your Key Vault
- Click **Access policies** in the left menu
- Click **+ Create**
- Select permissions:
  - Secret permissions: **Get**, **List**
- Select principal: Search for your Function App name
- Click **Review + create**, then **Create**

### 3. Add Key Vault Reference to Function App
- Go to your Function App
- Click **Configuration** in the left menu
- Under **Application settings**, click **+ New application setting**
- Name: `KeyVaultUri`
- Value: `https://m3h-keyvault.vault.azure.net/` (replace with your Key Vault URL)
- Click **OK**, then **Save**

---

## Secrets Summary Table

| Secret Name | Value | Notes |
|------------|-------|-------|
| `DatabaseConnectionString` | Your SQL connection string | Used in backend for database access |
| `ApiUrl` | https://nomikai-funcapp.azurewebsites.net | (Optional) For frontend configuration |

---

## Next Steps

1. ✅ Manually create secrets in Azure Key Vault
2. ✅ Enable Managed Identity on Function App
3. ✅ Grant access policies to Key Vault
4. ✅ Deploy updated backend code (see updated `Function1.cs`)
5. ✅ Update frontend with environment variables (see `.env` file)

