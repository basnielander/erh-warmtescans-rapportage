using ERH.HeatScans.Reporting.Server.Services;
using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure CORS
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

// Configure Google OAuth authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.Audience = builder.Configuration["Authentication:Google:ClientId"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://accounts.google.com",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:Google:ClientId"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Register Google Drive services
builder.Services.AddSingleton<GoogleDriveService>();
builder.Services.AddScoped<UserGoogleDriveService>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Google Drive API endpoints (Service Account - Admin only)
app.MapGet("/api/googledrive/structure", async (GoogleDriveService driveService, string? folderId, CancellationToken cancellationToken) =>
{
    try
    {
        var structure = await driveService.GetFolderStructureAsync(folderId, cancellationToken);
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
.WithDescription("Get the hierarchical folder and file structure from Google Drive")
.AddOpenApiOperationTransformer((operation, context, ct) =>
{
    operation.Summary = "Gets a folder and files list.";
    operation.Description = "TBD.";
    return Task.CompletedTask;
});

app.MapGet("/api/googledrive/files", async (GoogleDriveService driveService, string? folderId, CancellationToken cancellationToken) =>
{
    try
    {
        var files = await driveService.GetFlatFileListAsync(folderId, cancellationToken);
        return Results.Ok(files);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error retrieving Google Drive files");
    }
})
.WithName("GetGoogleDriveFiles")
.WithDescription("Get a flat list of all files from a Google Drive folder")
.AddOpenApiOperationTransformer((operation, context, ct) =>
{
    operation.Summary = "Gets a folder and files list.";
    operation.Description = "TBD.";
    return Task.CompletedTask;
});

// User-authenticated Google Drive API endpoints
app.MapGet("/api/user/googledrive/structure", async (
    UserGoogleDriveService driveService, 
    HttpContext context,
    string? folderId, 
    CancellationToken cancellationToken) =>
{
    try
    {
        // Extract access token from Authorization header
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Results.Unauthorized();
        }

        var accessToken = authHeader.Substring("Bearer ".Length).Trim();
        var structure = await driveService.GetFolderStructureAsync(accessToken, folderId, cancellationToken);
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
.WithName("GetUserGoogleDriveStructure")
.WithDescription("Get the hierarchical folder and file structure from user's Google Drive")
.AddOpenApiOperationTransformer((operation, context, ct) =>
{
    operation.Summary = "Gets a folder and files list from user's Google Drive.";
    operation.Description = "Requires user authentication via Google OAuth.";
    return Task.CompletedTask;
});

app.MapGet("/api/user/googledrive/files", async (
    UserGoogleDriveService driveService,
    HttpContext context,
    string? folderId, 
    CancellationToken cancellationToken) =>
{
    try
    {
        // Extract access token from Authorization header
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Results.Unauthorized();
        }

        var accessToken = authHeader.Substring("Bearer ".Length).Trim();
        var files = await driveService.GetFlatFileListAsync(accessToken, folderId, cancellationToken);
        return Results.Ok(files);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error retrieving Google Drive files");
    }
})
.WithName("GetUserGoogleDriveFiles")
.WithDescription("Get a flat list of all files from user's Google Drive folder")
.AddOpenApiOperationTransformer((operation, context, ct) =>
{
    operation.Summary = "Gets a flat list of files from user's Google Drive.";
    operation.Description = "Requires user authentication via Google OAuth.";
    return Task.CompletedTask;
});

app.MapFallbackToFile("/index.html");

app.Run();
