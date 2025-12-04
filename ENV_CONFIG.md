# Environment Configuration Guide

## Backend Configuration (Azure Functions)

### Priority Order
1. **Environment Variables** (Primary - Recommended)
2. **Azure Key Vault** (Optional Fallback)

### Local Development Setup

1. Edit `backend/FunctionbeerAPI/local.settings.json`:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "DatabaseConnectionString": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=USER;Password=PASS;Encrypt=True;",
    "KeyVaultUri": ""
  }
}
```

2. Replace with your actual database connection string
3. Leave `KeyVaultUri` empty to skip Key Vault

### Azure Deployment Configuration

#### Option 1: Environment Variables Only (Recommended)
Set in Azure Portal > Function App > Configuration > Application Settings:
- `DatabaseConnectionString`: Your SQL connection string
- `KeyVaultUri`: (leave empty or don't set)

#### Option 2: With Key Vault (Optional)
If you want to use Key Vault as a fallback:
1. Set `KeyVaultUri` to your Key Vault URL
2. Store `DatabaseConnectionString` secret in Key Vault
3. If environment variable exists, it will be used first
4. **Note**: Managed Identity or service principal required for Key Vault access

### How It Works
```csharp
// Connection string resolution:
1. Check environment variable "DatabaseConnectionString"
2. If found → Use it ✓
3. If not found AND KeyVaultUri is set → Try Key Vault
4. If both fail → Error with clear message
```

## Frontend Configuration

### API URL Configuration

The frontend uses a configurable API base URL with fallback.

#### Local Development
Edit `frontend/config.js` to set your API URL:
```javascript
const config = {
  apiBaseUrl: window.ENV?.API_BASE_URL || 'https://your-api-url.azurestaticapps.net'
};
```

#### Production
1. Set environment variable `API_BASE_URL` in Static Web App configuration
2. Or update the hardcoded fallback in `config.js`

### Files Modified
- `frontend/config.js` - Configuration file
- `frontend/script.js` - Updated to use `window.appConfig.getApiUrl()`
- `frontend/index.html` - Includes config.js

## Summary

✅ **Environment variables are primary and recommended**
✅ **Key Vault is optional** - only used if env var not found
✅ **No Key Vault permissions required** if using env variables only
✅ **Clear error messages** when configuration is missing
✅ **Works for local development and cloud deployment**
