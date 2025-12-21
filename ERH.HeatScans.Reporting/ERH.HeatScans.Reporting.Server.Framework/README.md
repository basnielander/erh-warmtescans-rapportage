# ERH Heat Scans Reporting Server - .NET Framework 4.8.1

## Migration from .NET 10 to .NET Framework 4.8.1

This project is a **complete rewrite** of the ERH.HeatScans.Reporting.Server from .NET 10 to .NET Framework 4.8.1.

### Major Changes

#### Architecture
- **From**: ASP.NET Core 10 with Minimal APIs
- **To**: ASP.NET Framework Web API with Controllers

#### Project Structure
- **SDK-Style Project** ? **Classic .csproj format**
- **appsettings.json** ? **Web.config**
- **Program.cs with WebApplication** ? **Global.asax with HttpApplication**
- **Minimal API endpoints** ? **Web API Controllers**
- **Built-in DI** ? **Unity DI Container**

#### Code Changes

1. **Models**
   - Converted from `record` types to `class` types
   - Changed `init` properties to `set` properties
   - Added explicit constructors

2. **Dependency Injection**
   - Using Unity Container instead of built-in DI
   - Services registered in `Global.asax.cs`

3. **Configuration**
   - Settings moved from `appsettings.json` to `Web.config` AppSettings
   - Access via `ConfigurationManager.AppSettings`

4. **Routing**
   - Minimal API routes converted to Controller actions
   - Attribute routing with `[RoutePrefix]` and `[Route]`

5. **CORS**
   - Configured in `WebApiConfig.cs` using `EnableCorsAttribute`
   - Additional CORS headers in `Global.asax` event handlers
   - Headers also in `Web.config` for IIS

6. **Error Handling**
   - Changed from `Results.Problem()` to `InternalServerError()`
   - Removed custom error descriptions

### API Endpoints

All endpoints remain functionally identical:

#### Service Account Endpoints
- `GET /api/googledrive/structure?folderId={id}` - Get folder structure
- `GET /api/googledrive/files?folderId={id}` - Get flat file list

#### User-Authenticated Endpoints
- `GET /api/user/googledrive/structure?folderId={id}` - Get user's folder structure
- `GET /api/user/googledrive/files?folderId={id}` - Get user's file list

### Prerequisites

1. **.NET Framework 4.8.1 Developer Pack**
   - Download from: https://dotnet.microsoft.com/download/dotnet-framework/net481

2. **NuGet Package Restore**
   - Required packages are listed in `packages.config`
   - Visual Studio will restore automatically

3. **Google Credentials**
   - Place `google-credentials.json` in the project root
   - Update path in `Web.config` if needed

### Configuration

Edit `Web.config` to configure:

```xml
<appSettings>
  <add key="GoogleDrive:CredentialPath" value="./google-credentials.json" />
  <add key="Authentication:Google:ClientId" value="YOUR_CLIENT_ID" />
</appSettings>
```

### Running the Application

1. **IIS Express** (Default)
   - Press F5 in Visual Studio
   - Application runs on https://localhost:44300/

2. **Local IIS**
   - Publish to IIS
   - Configure application pool for .NET Framework 4.8
   - Ensure Windows Authentication is enabled if needed

### Limitations Compared to .NET 10

1. **No Nullable Reference Types** - No compile-time null safety
2. **No Top-Level Statements** - More boilerplate code required
3. **No Records** - Using classes instead
4. **No Pattern Matching** - Less concise code
5. **Older C# Language Features** - No C# 10+ features
6. **No Minimal APIs** - More verbose controller code
7. **Performance** - Generally slower than .NET Core/5+
8. **No OpenAPI/Swagger** - Would require Swashbuckle.AspNet (not included)
9. **No Built-in SPA Integration** - SPA proxy features removed

### NuGet Packages Used

- **Microsoft.AspNet.WebApi** - Web API framework
- **Microsoft.AspNet.WebApi.Cors** - CORS support
- **Google.Apis.Drive.v3** - Google Drive API
- **Unity** - Dependency Injection container
- **Newtonsoft.Json** - JSON serialization
- **System.IdentityModel.Tokens.Jwt** - JWT token handling

### Development Notes

- The original .NET 10 project remains unchanged in `ERH.HeatScans.Reporting.Server/`
- This .NET Framework project is in `ERH.HeatScans.Reporting.Server.Framework/`
- Frontend client project remains unchanged
- Update client `baseUrl` to point to new server if needed: `https://localhost:44300/`

### Deployment

For IIS deployment:
1. Publish the project using Visual Studio
2. Copy published files to IIS web directory
3. Create an IIS application pointing to the published folder
4. Ensure .NET Framework 4.8.1 is installed on the server
5. Copy `google-credentials.json` to the deployment directory
6. Update `Web.config` with production settings

### Troubleshooting

**CORS Issues:**
- Check allowed origins in `WebApiConfig.cs`
- Verify client origin matches allowed list
- Check browser console for CORS errors

**Google Authentication:**
- Verify `google-credentials.json` path in Web.config
- Check service account permissions
- Ensure OAuth client ID is correct

**500 Errors:**
- Check Windows Event Log
- Enable detailed errors: `<customErrors mode="Off"/>` in Web.config
- Check IIS application pool identity permissions

### Migration Checklist

- [x] Convert project to .NET Framework 4.8.1
- [x] Convert SDK-style csproj to classic format
- [x] Create Web.config
- [x] Convert Program.cs to Global.asax
- [x] Convert minimal APIs to controllers
- [x] Convert records to classes
- [x] Set up dependency injection with Unity
- [x] Configure CORS
- [x] Preserve all API functionality
- [x] Document changes

### Recommended: Stay on Modern .NET

If possible, consider staying on .NET 8 (LTS) or .NET 10+ for:
- Better performance
- Modern language features
- Better tooling
- Longer support lifecycle
- Cross-platform capabilities
- Better security updates
