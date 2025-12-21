# Quick Setup: Change Port to 7209

## ? Quick Steps

### The project file needs to be updated. Follow these steps:

1. **Close Visual Studio** completely (important!)

2. **Run this PowerShell script:**
   ```powershell
   cd ERH.HeatScans.Reporting.Server.Framework
   .\ConfigurePort7209.ps1
   ```

   **OR manually edit:**
   
   Open `ERH.HeatScans.Reporting.Server.Framework.csproj` in Notepad and change:
   ```xml
   FROM: <IISExpressSSLPort>44300</IISExpressSSLPort>
   TO:   <IISExpressSSLPort>7209</IISExpressSSLPort>
   ```

3. **(Optional but recommended)** Delete the `.vs` folder:
   ```powershell
   Remove-Item -Recurse -Force .\.vs
   ```

4. **Open Visual Studio** and load the solution

5. **Press F5** to start debugging

6. **Verify** it's running on `https://localhost:7209/`

## ? Test Endpoints

Once running, test in browser or Postman:
```
https://localhost:7209/api/googledrive/structure
```

## ?? If Port Change Doesn't Work

1. Right-click project ? **Properties**
2. Go to **Web** tab  
3. Under **Servers**, check the **Project Url**
4. If it doesn't show `https://localhost:7209/`, click **Create Virtual Directory**

## ?? Update Angular Client

After port change, update:

**File:** `erh.heatscans.reporting.client/src/app/services/google-drive.service.ts`

```typescript
private baseUrl = 'https://localhost:7209/api/user/googledrive';
```

---

**See PORT_7209_SETUP.md for detailed troubleshooting**
