# CORS Configuration Fix

## Problem
The Angular app (running on port 49806) was unable to call the API (running on port 7209) due to CORS issues.

## Changes Made

### 1. Web.config - Removed Conflicting CORS Headers
**File:** `ERH.HeatScans.Reporting.Server.Framework\Web.config`

- **Removed** the conflicting wildcard CORS headers from `<httpProtocol><customHeaders>` section
- These headers were conflicting with the proper CORS configuration in WebApiConfig.cs
- Web API CORS should be handled through the `System.Web.Http.Cors` package, not through IIS headers

### 2. WebApiConfig.cs - Updated Allowed Origins
**File:** `ERH.HeatScans.Reporting.Server.Framework\App_Start\WebApiConfig.cs`

- **Added** `http://localhost:49806` to the allowed origins (Angular dev server might use HTTP)
- **Kept** `https://localhost:49806` for HTTPS support
- **Enabled** `SupportsCredentials = true` for cookie/auth header support

Current allowed origins:
```
http://localhost:49806   (Angular dev - HTTP)
https://localhost:49806  (Angular dev - HTTPS)
https://localhost:5173   (Vite fallback)
https://localhost:7209   (API itself)
```

### 3. Angular Proxy Configuration
**File:** `erh.heatscans.reporting.client\src\proxy.conf.js`

- **Changed** context from `/weatherforecast` to `/api` to proxy all API calls
- **Added** `changeOrigin: true` to properly handle the origin header
- **Added** `logLevel: "debug"` for troubleshooting
- **Set** `secure: false` to allow self-signed certificates in development

### 4. Updated .http Test File
**File:** `ERH.HeatScans.Reporting.Server.Framework\UserGoogleDrive.http`

- **Corrected** base URL to `https://localhost:7209` (the actual API port)
- **Added** example for proxied calls from Angular app

## How It Works

### Option 1: Direct API Calls (from .http file or external clients)
```
Client ? https://localhost:7209/api/user/googledrive
```
- CORS headers are added by WebApiConfig.cs
- Client must be on allowed origins list

### Option 2: Proxied Calls (from Angular app)
```
Angular App (http://localhost:49806) 
  ? Proxy intercepts /api/* requests
    ? Forwards to https://localhost:7209/api/*
      ? API returns response
        ? Proxy returns to Angular
```
- No CORS issues because proxy makes server-side request
- Angular app sees same-origin request (port 49806)

## Testing

1. **Start the API**: Run the `ERH.HeatScans.Reporting.Server.Framework` project
   - Should be available at `https://localhost:7209`

2. **Start Angular**: Run `npm start` or `ng serve` in the Angular project
   - Should be available at `http://localhost:49806`
   - Proxy will automatically forward `/api/*` requests to the API

3. **Test the endpoint**: 
   ```typescript
   // In Angular, use relative path (will be proxied)
   this.http.post('/api/user/googledrive?addressFolderId=YOUR_FOLDER_ID', {}, {
     headers: { Authorization: `Bearer ${accessToken}` }
   })
   ```

## Troubleshooting

### If you still see CORS errors:

1. **Clear browser cache** and restart both Angular and API servers
2. **Check browser console** for the actual CORS error message
3. **Verify the proxy is working**: Check Angular dev server console for proxy logs
4. **Check API is running**: Navigate to `https://localhost:7209/api/user/googledrive/structure` directly
5. **Verify SSL certificate**: Accept the self-signed certificate in your browser

### Common Issues:

- **Preflight OPTIONS request failing**: Make sure API accepts OPTIONS verb (already configured)
- **Credentials not included**: Ensure `SupportsCredentials = true` and Angular uses `withCredentials: true`
- **Wrong port**: Verify Angular is on 49806 and API is on 7209
