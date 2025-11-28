# Azure Architecture Diagram

## Current Architecture with Azure Key Vault

```
┌─────────────────────────────────────────────────────────────────┐
│                         Frontend                                 │
│                    (Static Web App/CDN)                          │
│                                                                   │
│  ┌──────────────────────────────────────────────────────┐       │
│  │ Vue.js Application                                   │       │
│  │ - Event Registration (POST /api/savenomikai)         │       │
│  │ - Event Search (GET /api/nomikai/search)             │       │
│  │ - Update Payments (POST /api/updatepaymentflags)     │       │
│  │                                                       │       │
│  │ API URL: import.meta.env.VITE_API_URL                │       │
│  └──────────────────────────────────────────────────────┘       │
│                          │                                       │
│                          │ HTTPS                                 │
│                          ▼                                       │
└─────────────────────────────────────────────────────────────────┘
                           │
                           │
┌──────────────────────────────────────────────────────────────────────┐
│              Azure Function App (m3h-beerkn-functionapp)              │
│                                                                        │
│  ┌────────────────────────────────────────────────────────────┐     │
│  │ HTTP Triggered Functions:                                   │     │
│  │ • Calculate (POST /api/calculate)                           │     │
│  │ • GetHistory (GET /api/history)                             │     │
│  │ • SaveHistory (POST /api/history)                           │     │
│  │ • SaveNomikaiEvent (POST /api/savenomikai)                 │     │
│  │ • SearchNomikaiEvent (GET /api/nomikai/search)             │     │
│  │ • UpdatePaymentFlags (POST /api/updatepaymentflags)        │     │
│  └────────────────────────────────────────────────────────────┘     │
│                          │                                           │
│                          │ Calls GetConnectionStringFromKeyVault()  │
│                          ▼                                           │
│  ┌────────────────────────────────────────────────────────────┐     │
│  │ Key Vault Integration (Managed Identity)                   │     │
│  │ • DefaultAzureCredential()                                  │     │
│  │ • Retrieves: DatabaseConnectionString                       │     │
│  └────────────────────────────────────────────────────────────┘     │
│                          │                                           │
│                          │ Secure Request (Managed Identity)        │
│                          ▼                                           │
└──────────────────────────────────────────────────────────────────────┘
                           │
                           │
┌──────────────────────────────────────────────────────────────────────┐
│                    Azure Key Vault (m3h-keyvault)                    │
│                                                                        │
│  ┌────────────────────────────────────────────────────────────┐     │
│  │ Secrets:                                                    │     │
│  │ • DatabaseConnectionString (encrypted)                      │     │
│  │   Value: Server=tcp:m3hkanamofunctiondb...                 │     │
│  │                                                             │     │
│  │ Access Policy: Function App Managed Identity               │     │
│  │ Permissions: get, list                                      │     │
│  └────────────────────────────────────────────────────────────┘     │
│                          │                                           │
│                          │ Returns decrypted secret                 │
│                          ▼                                           │
└──────────────────────────────────────────────────────────────────────┘
                           │
                           │
┌──────────────────────────────────────────────────────────────────────┐
│            Azure SQL Database (m3h-kanamo-functionDB)                │
│                                                                        │
│  ┌────────────────────────────────────────────────────────────┐     │
│  │ Tables:                                                     │     │
│  │ • Nomikai                                                   │     │
│  │ • Payments                                                  │     │
│  │ • Events                                                    │     │
│  │                                                             │     │
│  │ Connection: TLS Encrypted                                   │     │
│  │ Firewall: Allow Azure Services                              │     │
│  └────────────────────────────────────────────────────────────┘     │
│                                                                        │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Data Flow: Event Registration Example

```
User Fills Form
      │
      ▼
[POST] /api/savenomikai
      │
      ▼
SaveNomikaiEvent() Function
      │
      ├─ Parse Request
      │
      ├─ Validate Data
      │
      ├─ Call GetConnectionStringFromKeyVault()
      │       │
      │       ▼
      │    [SecretClient] with DefaultAzureCredential
      │       │
      │       ▼
      │    Azure Key Vault (Managed Identity Auth)
      │       │
      │       ▼
      │    Return: "Server=tcp:..."
      │
      ├─ Create SqlConnection with connection string
      │
      ├─ Split participants
      │
      ├─ Calculate amount per participant
      │
      ├─ INSERT INTO Nomikai table (for each participant)
      │
      ▼
Return Success Response
      │
      ▼
Frontend Displays: "Event saved successfully"
```

---

## Security Layers

```
┌─────────────────────────────────────────────────────────────┐
│ Layer 1: Client Security                                    │
│ • HTTPS only (encrypted in transit)                         │
│ • Frontend hosted on CDN/Static Web App                      │
│ • CORS enabled for trusted domain only                       │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Layer 2: API Security                                       │
│ • Azure Function App (serverless)                           │
│ • Anonymous auth (public endpoints)                         │
│ • Can be upgraded to API Key or OAuth2                      │
│ • Application Insights logging                              │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Layer 3: Secret Management                                  │
│ • Azure Key Vault (encrypted at rest)                       │
│ • Managed Identity (no credentials in code)                 │
│ • Access control via IAM policies                           │
│ • Audit logs for all access                                 │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│ Layer 4: Database Security                                  │
│ • Azure SQL Database (encrypted at rest)                    │
│ • TLS 1.2+ for connections                                  │
│ • Firewall rules (allow only Azure services)                │
│ • SQL authentication (username/password in Key Vault)       │
│ • Row-level security (can be implemented)                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Deployment Pipeline

```
Source Code (GitHub)
      │
      ├─ backend/
      │    └─ FunctionbeerAPI/
      │         ├─ Function1_Updated.cs (with Key Vault)
      │         ├─ FunctionbeerAPI.csproj (updated packages)
      │         └─ host.json
      │
      └─ frontend/
           ├─ script.js (environment variables)
           ├─ .env (local config)
           ├─ .env.production (prod config)
           └─ vite.config.js

                    │
                    ▼ Build & Test

      ┌─────────────────────────────────┐
      │ Backend Build                   │
      │ • dotnet build                  │
      │ • dotnet publish --configuration Release
      │ • Create deployment package     │
      └─────────────────────────────────┘
                    │
                    ▼

      ┌─────────────────────────────────┐
      │ Frontend Build                  │
      │ • npm install                   │
      │ • npm run build:azure           │
      │ • Output: dist/                 │
      └─────────────────────────────────┘
                    │
                    ▼ Deploy

      ┌─────────────────────────────────┐
      │ Azure Function App              │
      │ • Zip deploy or VS publish      │
      │ • Contains Key Vault reference  │
      └─────────────────────────────────┘
                    │
                    ▼

      ┌─────────────────────────────────┐
      │ Azure Static Web App / CDN      │
      │ • Serves frontend files         │
      │ • Points to Function App API    │
      └─────────────────────────────────┘
                    │
                    ▼

               Live in Azure
```

---

## Configuration Files Summary

| File | Location | Purpose |
|------|----------|---------|
| `AZURE_KEY_VAULT_SETUP.md` | Root | Setup guide for Key Vault |
| `KEY_VAULT_SECRETS.md` | Root | What secrets to store |
| `DEPLOYMENT_CHECKLIST.md` | Root | Step-by-step deployment |
| `FRONTEND_ENV_CONFIG.md` | Root | Frontend environment setup |
| `Function1_Updated.cs` | backend/FunctionbeerAPI/ | Backend with Key Vault integration |
| `FunctionbeerAPI.csproj` | backend/FunctionbeerAPI/ | Updated NuGet packages |
| `.env` | frontend/ | Local dev environment (create) |
| `.env.production` | frontend/ | Production environment (create) |
| `vite.config.js` | frontend/ | Vite build config (create) |

---

## Environment Variables

### Backend (Azure Function App)
```
KeyVaultUri = https://m3h-keyvault.vault.azure.net/
```

### Frontend (.env files)
```
VITE_API_URL = https://m3h-beerkn-functionapp.azurewebsites.net
```

---

## Managed Identity Flow

```
Function App (Managed Identity Enabled)
      │
      │ Uses DefaultAzureCredential()
      │
      ▼
Azure AD Token Service
      │
      │ Issues token without stored credentials
      │
      ▼
Azure Key Vault
      │
      │ Validates token
      │ Checks access policy
      │
      ▼
Returns Decrypted Secret
      │
      ▼
Function uses connection string to access Database
```

