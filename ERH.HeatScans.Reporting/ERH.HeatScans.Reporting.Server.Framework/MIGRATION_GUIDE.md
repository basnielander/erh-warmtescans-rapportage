# Migration Guide: .NET 10 to .NET Framework 4.8.1

## Side-by-Side Comparison

### Project File

#### .NET 10 (Original)
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

#### .NET Framework 4.8.1 (New)
```xml
<Project ToolsVersion="15.0" DefaultTargets="Build">
  <PropertyGroup>
    <TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>
    <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};...</ProjectTypeGuids>
  </PropertyGroup>
</Project>
```

### Configuration

#### .NET 10: appsettings.json
```json
{
  "GoogleDrive": {
    "CredentialPath": "./google-credentials.json"
  },
  "Authentication": {
    "Google": {
      "ClientId": "..."
    }
  }
}
```

#### .NET Framework 4.8.1: Web.config
```xml
<appSettings>
  <add key="GoogleDrive:CredentialPath" value="./google-credentials.json" />
  <add key="Authentication:Google:ClientId" value="..." />
</appSettings>
```

### Application Startup

#### .NET 10: Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddCors(...);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)...
builder.Services.AddSingleton<GoogleDriveService>();
builder.Services.AddScoped<UserGoogleDriveService>();

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/googledrive/structure", async (...) => {...});
app.Run();
```

#### .NET Framework 4.8.1: Global.asax.cs
```csharp
public class WebApiApplication : HttpApplication
{
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(WebApiConfig.Register);
        var container = new UnityContainer();
        container.RegisterSingleton<GoogleDriveService>();
        container.RegisterType<UserGoogleDriveService>();
        GlobalConfiguration.Configuration.DependencyResolver = 
            new UnityDependencyResolver(container);
    }
}
```

### API Endpoints

#### .NET 10: Minimal API
```csharp
app.MapGet("/api/googledrive/structure", 
    async (GoogleDriveService driveService, string? folderId, 
           CancellationToken cancellationToken) =>
{
    try
    {
        var structure = await driveService.GetFolderStructureAsync(
            folderId, cancellationToken);
        return Results.Ok(structure);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error retrieving Google Drive structure");
    }
})
.WithName("GetGoogleDriveStructure")
.WithDescription("Get the hierarchical folder and file structure");
```

#### .NET Framework 4.8.1: Controller
```csharp
[RoutePrefix("api/googledrive")]
public class GoogleDriveController : ApiController
{
    private readonly GoogleDriveService _driveService;

    public GoogleDriveController(GoogleDriveService driveService)
    {
        _driveService = driveService;
    }

    [HttpGet]
    [Route("structure")]
    public async Task<IHttpActionResult> GetStructure(
        string folderId = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var structure = await _driveService.GetFolderStructureAsync(
                folderId, cancellationToken);
            return Ok(structure);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }
}
```

### Models

#### .NET 10: Record
```csharp
namespace ERH.HeatScans.Reporting.Server.Models;

public record GoogleDriveItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public bool IsFolder { get; init; }
    public DateTime? ModifiedTime { get; init; }
    public long? Size { get; init; }
    public List<GoogleDriveItem> Children { get; set; } = new();
}
```

#### .NET Framework 4.8.1: Class
```csharp
using System;
using System.Collections.Generic;

namespace ERH.HeatScans.Reporting.Server.Framework.Models
{
    public class GoogleDriveItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public bool IsFolder { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public long? Size { get; set; }
        public List<GoogleDriveItem> Children { get; set; }

        public GoogleDriveItem()
        {
            Id = string.Empty;
            Name = string.Empty;
            MimeType = string.Empty;
            Children = new List<GoogleDriveItem>();
        }
    }
}
```

### CORS Configuration

#### .NET 10
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:49806", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
app.UseCors();
```

#### .NET Framework 4.8.1
```csharp
// In WebApiConfig.cs
var cors = new EnableCorsAttribute(
    origins: "https://localhost:49806,https://localhost:5173",
    headers: "*",
    methods: "*")
{
    SupportsCredentials = true
};
config.EnableCors(cors);

// Also in Global.asax.cs for preflight handling
protected void Application_BeginRequest(object sender, EventArgs e)
{
    if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
    {
        HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", 
            GetAllowedOrigin());
        // ... other headers
        HttpContext.Current.Response.End();
    }
}
```

### Configuration Reading

#### .NET 10
```csharp
public GoogleDriveService(IConfiguration configuration, 
                          ILogger<GoogleDriveService> logger)
{
    _logger = logger;
    var credentialPath = configuration["GoogleDrive:CredentialPath"];
}
```

#### .NET Framework 4.8.1
```csharp
using System.Configuration;

public GoogleDriveService()
{
    var credentialPath = ConfigurationManager.AppSettings["GoogleDrive:CredentialPath"];
}
```

### Authentication Header Extraction

#### .NET 10
```csharp
var authHeader = context.Request.Headers.Authorization.ToString();
if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
{
    return Results.Unauthorized();
}
var accessToken = authHeader.Substring("Bearer ".Length).Trim();
```

#### .NET Framework 4.8.1
```csharp
private string GetAccessTokenFromHeader()
{
    var authHeader = Request.Headers.Authorization;
    if (authHeader == null || authHeader.Scheme != "Bearer")
    {
        return null;
    }
    return authHeader.Parameter;
}
```

## Key Differences Summary

| Feature | .NET 10 | .NET Framework 4.8.1 |
|---------|---------|---------------------|
| Project Format | SDK-style | Classic .csproj |
| Configuration | appsettings.json | Web.config |
| API Style | Minimal APIs | Controllers |
| Dependency Injection | Built-in | Unity Container |
| Startup | Program.cs | Global.asax |
| Models | Records | Classes |
| Nullable | Nullable reference types | No nullable reference types |
| Usings | Implicit usings | Explicit usings |
| Top-level statements | Yes | No |
| Pattern matching | Advanced | Limited |
| Performance | Higher | Lower |
| Cross-platform | Yes | Windows only |

## Testing the Migration

### Update Client Configuration

If using the Angular client, update the API base URL:

```typescript
// In google-drive.service.ts
private baseUrl = 'https://localhost:44300/api/user/googledrive';
```

### Test Endpoints

1. **Service Account Endpoint:**
   ```
   GET https://localhost:44300/api/googledrive/structure
   ```

2. **User-Authenticated Endpoint:**
   ```
   GET https://localhost:44300/api/user/googledrive/structure
   Headers: Authorization: Bearer {token}
   ```

## Common Issues and Solutions

### Issue: NuGet packages not restoring
**Solution:** Right-click solution ? Restore NuGet Packages

### Issue: Google credentials not found
**Solution:** Ensure `google-credentials.json` is in project root and "Copy to Output Directory" is set to "Copy always"

### Issue: CORS errors
**Solution:** 
1. Check allowed origins in `WebApiConfig.cs`
2. Verify client origin matches exactly
3. Check browser console for specific error

### Issue: Cannot find type or namespace
**Solution:** Add missing using statements (no implicit usings in Framework)

### Issue: 404 on API endpoints
**Solution:** Check routing configuration in `WebApiConfig.cs` and controller route attributes

## Rollback Plan

If you need to rollback to .NET 10:
1. The original project is untouched in `ERH.HeatScans.Reporting.Server/`
2. Simply point your client back to `https://localhost:7209/`
3. Remove or ignore the `.Framework` project

## Performance Considerations

.NET Framework 4.8.1 will be slower than .NET 10:
- Startup time: ~2-3x slower
- Request throughput: ~30-50% lower
- Memory usage: ~20-30% higher

Consider this for production planning.
