# Migration from packages.config to PackageReference

## Problem

The application fails to start with the error:
```
System.Configuration.ConfigurationErrorsException: Cannot load file or assembly 
'Flir.Cronos.Filter.Adapter.DLL' or one of its dependencies.
```

### Root Cause

The `Flir.Cronos.Filter.Adapter.dll` file exists in the bin directory but depends on **native (unmanaged) C++ DLLs** that are missing. FLIR SDKs typically require:
- Visual C++ Redistributable runtime libraries (vcruntime140.dll, msvcp140.dll)
- Native FLIR IRSDK libraries

When using `packages.config`, native dependencies from NuGet packages may not be automatically copied to the output directory. The `PackageReference` format has better support for handling native dependencies.

## Solution: Migrate to PackageReference

### Step 1: Close Visual Studio

**Important**: You must close Visual Studio completely before modifying the `.csproj` file.

### Step 2: Backup Your Project

Create a backup of:
- `ERH.HeatScans.Reporting.Server.Framework.csproj`
- `packages.config`

### Step 3: Replace the Project File Content

Replace the entire content of `ERH.HeatScans.Reporting.Server.Framework.csproj` with the following:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProductVersion>
    </ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{8B5E5F7D-9C4A-4E2B-8F3D-1A2B3C4D5E6F}</ProjectGuid>
    <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>ERH.HeatScans.Reporting.Server.Framework</RootNamespace>
    <AssemblyName>ERH.HeatScans.Reporting.Server.Framework</AssemblyName>
    <TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>
    <UseIISExpress>true</UseIISExpress>
    <Use64BitIISExpress />
    <IISExpressSSLPort>7209</IISExpressSSLPort>
    <IISExpressAnonymousAuthentication>enabled</IISExpressAnonymousAuthentication>
    <IISExpressWindowsAuthentication>disabled</IISExpressWindowsAuthentication>
    <IISExpressUseClassicPipelineMode>false</IISExpressUseClassicPipelineMode>
    <UseGlobalApplicationHostFile />
    <NuGetPackageImportStamp>
    </NuGetPackageImportStamp>
    <LangVersion>latest</LangVersion>
    <RestoreProjectStyle>PackageReference</RestoreProjectStyle>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CommonServiceLocator" Version="2.0.7" />
    <PackageReference Include="Flir.Atlas.Cronos" Version="7.6.0" />
    <PackageReference Include="Google.Api.CommonProtos" Version="2.17.0" />
    <PackageReference Include="Google.Api.Gax" Version="4.12.1" />
    <PackageReference Include="Google.Api.Gax.Grpc" Version="4.12.1" />
    <PackageReference Include="Google.Apis" Version="1.73.0" />
    <PackageReference Include="Google.Apis.Auth" Version="1.73.0" />
    <PackageReference Include="Google.Apis.Core" Version="1.73.0" />
    <PackageReference Include="Google.Apis.Drive.v3" Version="1.73.0.3996" />
    <PackageReference Include="Google.Geo.Type" Version="1.3.0" />
    <PackageReference Include="Google.Maps.Places.V1" Version="1.0.0-beta18" />
    <PackageReference Include="Google.Protobuf" Version="3.31.1" />
    <PackageReference Include="Grpc.Auth" Version="2.71.0" />
    <PackageReference Include="Grpc.Core" Version="2.46.6" />
    <PackageReference Include="Grpc.Core.Api" Version="2.71.0" />
    <PackageReference Include="Grpc.Net.Client" Version="2.71.0" />
    <PackageReference Include="Grpc.Net.Common" Version="2.71.0" />
    <PackageReference Include="Microsoft.AspNet.Cors" Version="5.3.0" />
    <PackageReference Include="Microsoft.AspNet.WebApi" Version="5.3.0" />
    <PackageReference Include="Microsoft.AspNet.WebApi.Client" Version="6.0.0" />
    <PackageReference Include="Microsoft.AspNet.WebApi.Core" Version="5.3.0" />
    <PackageReference Include="Microsoft.AspNet.WebApi.Cors" Version="5.3.0" />
    <PackageReference Include="Microsoft.AspNet.WebApi.WebHost" Version="5.3.0" />
    <PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="8.0.0" />
    <PackageReference Include="Microsoft.Bcl.TimeProvider" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="6.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="6.0.0" />
    <PackageReference Include="Microsoft.IdentityModel.Abstractions" Version="8.15.0" />
    <PackageReference Include="Microsoft.IdentityModel.JsonWebTokens" Version="8.15.0" />
    <PackageReference Include="Microsoft.IdentityModel.Logging" Version="8.15.0" />
    <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.15.0" />
    <PackageReference Include="Microsoft.VisualStudio.SlowCheetah" Version="4.0.50" />
    <PackageReference Include="Microsoft.Web.Infrastructure" Version="1.0.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
    <PackageReference Include="Newtonsoft.Json.Bson" Version="1.0.2" />
    <PackageReference Include="System.Buffers" Version="4.5.1" />
    <PackageReference Include="System.Diagnostics.DiagnosticSource" Version="6.0.2" />
    <PackageReference Include="System.Memory" Version="4.5.5" />
    <PackageReference Include="System.Net.Http.WinHttpHandler" Version="8.0.0" />
    <PackageReference Include="System.Numerics.Vectors" Version="4.5.0" />
    <PackageReference Include="System.Runtime.CompilerServices.Unsafe" Version="6.0.0" />
    <PackageReference Include="System.Text.Encodings.Web" Version="8.0.0" />
    <PackageReference Include="System.Text.Json" Version="8.0.5" />
    <PackageReference Include="System.Threading.Tasks.Extensions" Version="4.5.4" />
    <PackageReference Include="System.ValueTuple" Version="4.6.1" />
    <PackageReference Include="Unity" Version="5.11.10" />
    <PackageReference Include="Unity.Abstractions" Version="5.11.7" />
    <PackageReference Include="Unity.AspNet.WebApi" Version="5.11.2" />
    <PackageReference Include="Unity.Container" Version="5.11.11" />
    <PackageReference Include="WebActivatorEx" Version="2.2.0" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include="System">
      <Private>True</Private>
    </Reference>
    <Reference Include="System.Core" />
    <Reference Include="System.Drawing">
      <Private>True</Private>
    </Reference>
    <Reference Include="System.Management" />
    <Reference Include="System.Net.Http" />
    <Reference Include="System.Numerics" />
    <Reference Include="System.Web" />
    <Reference Include="System.Web.Extensions" />
    <Reference Include="System.Configuration" />
    <Reference Include="System.ComponentModel.DataAnnotations" />
    <Reference Include="Microsoft.CSharp" />
    <Reference Include="System.Data">
      <Private>True</Private>
    </Reference>
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="System.Xml">
      <Private>True</Private>
    </Reference>
    <Reference Include="System.Xml.Linq">
      <Private>True</Private>
    </Reference>
  </ItemGroup>
  <ItemGroup>
    <Reference Include="FLIR.ATS.Format.Utilities">
      <HintPath>..\lib\FLIR.ATS.Format.Utilities.dll</HintPath>
    </Reference>
    <Reference Include="Flir.Cronos.Common">
      <HintPath>..\lib\Flir.Cronos.Common.dll</HintPath>
    </Reference>
    <Reference Include="Flir.Cronos.Filter">
      <HintPath>..\lib\Flir.Cronos.Filter.dll</HintPath>
    </Reference>
    <Reference Include="Flir.Cronos.Filter.Adapter">
      <HintPath>..\lib\Flir.Cronos.Filter.Adapter.dll</HintPath>
    </Reference>
    <Reference Include="Flir.Cronos.Panorama">
      <HintPath>..\lib\Flir.Cronos.Panorama.dll</HintPath>
    </Reference>
    <Reference Include="Flir.FormatPlugin.IRSDK">
      <HintPath>..\lib\Flir.FormatPlugin.IRSDK.dll</HintPath>
    </Reference>
    <Reference Include="Flir.FormatPlugin.PTW">
      <HintPath>..\lib\Flir.FormatPlugin.PTW.dll</HintPath>
    </Reference>
    <Reference Include="FLIRCommunicationsAdapter">
      <HintPath>..\lib\FLIRCommunicationsAdapter.dll</HintPath>
    </Reference>
  </ItemGroup>
  <ItemGroup>
    <Content Include="Web.config" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="App_Start\UnityConfig.cs" />
    <Compile Include="App_Start\UnityWebApiActivator.cs" />
    <Compile Include="App_Start\WebApiConfig.cs" />
    <Compile Include="Controllers\MapsController.cs" />
    <Compile Include="Controllers\FoldersAndFilesController.cs" />
    <Compile Include="Global.asax.cs">
      <DependentUpon>Global.asax</DependentUpon>
    </Compile>
    <Compile Include="Models\FileDownloadResult.cs" />
    <Compile Include="Models\GoogleDriveItem.cs" />
    <Compile Include="Services\FLIRService.cs" />
    <Compile Include="Services\GoogleDriveService.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="Global.asax" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="CleanAndRestore.bat" />
    <Content Include="http-client.env.json" />
    <Content Include="Maps.http" />
    <Content Include="CAMEL_CASE_CHANGE.md" />
    <Content Include="CAMEL_CASE_QUICK_REF.md" />
    <Content Include="RestorePackages.ps1" />
    <Content Include="RestorePackages.bat" />
    <Content Include="Setup-IISExpressSSL.ps1" />
    <Content Include="UserGoogleDrive.http" />
    <Content Include="protos\google\type\timeofday.proto" />
    <Content Include="protos\google\type\quaternion.proto" />
    <Content Include="protos\google\type\postal_address.proto" />
    <Content Include="protos\google\type\phone_number.proto" />
    <Content Include="protos\google\type\month.proto" />
    <Content Include="protos\google\type\money.proto" />
    <Content Include="protos\google\type\localized_text.proto" />
    <Content Include="protos\google\type\latlng.proto" />
    <Content Include="protos\google\type\interval.proto" />
    <Content Include="protos\google\type\fraction.proto" />
    <Content Include="protos\google\type\expr.proto" />
    <Content Include="protos\google\type\decimal.proto" />
    <Content Include="protos\google\type\dayofweek.proto" />
    <Content Include="protos\google\type\datetime.proto" />
    <Content Include="protos\google\type\date.proto" />
    <Content Include="protos\google\type\color.proto" />
    <Content Include="protos\google\type\calendar_period.proto" />
    <Content Include="protos\google\rpc\status.proto" />
    <Content Include="protos\google\rpc\http.proto" />
    <Content Include="protos\google\rpc\error_details.proto" />
    <Content Include="protos\google\rpc\context\audit_context.proto" />
    <Content Include="protos\google\rpc\context\attribute_context.proto" />
    <Content Include="protos\google\rpc\code.proto" />
    <Content Include="protos\google\api\visibility.proto" />
    <Content Include="protos\google\api\usage.proto" />
    <Content Include="protos\google\api\system_parameter.proto" />
    <Content Include="protos\google\api\source_info.proto" />
    <Content Include="protos\google\api\service.proto" />
    <Content Include="protos\google\api\routing.proto" />
    <Content Include="protos\google\api\resource.proto" />
    <Content Include="protos\google\api\quota.proto" />
    <Content Include="protos\google\api\policy.proto" />
    <Content Include="protos\google\api\monitoring.proto" />
    <Content Include="protos\google\api\monitored_resource.proto" />
    <Content Include="protos\google\api\metric.proto" />
    <Content Include="protos\google\api\logging.proto" />
    <Content Include="protos\google\api\log.proto" />
    <Content Include="protos\google\api\launch_stage.proto" />
    <Content Include="protos\google\api\label.proto" />
    <Content Include="protos\google\api\httpbody.proto" />
    <Content Include="protos\google\api\http.proto" />
    <Content Include="protos\google\api\field_info.proto" />
    <Content Include="protos\google\api\field_behavior.proto" />
    <Content Include="protos\google\api\error_reason.proto" />
    <Content Include="protos\google\api\endpoint.proto" />
    <Content Include="protos\google\api\documentation.proto" />
    <Content Include="protos\google\api\distribution.proto" />
    <Content Include="protos\google\api\control.proto" />
    <Content Include="protos\google\api\context.proto" />
    <Content Include="protos\google\api\consumer.proto" />
    <Content Include="protos\google\api\config_change.proto" />
    <Content Include="protos\google\api\client.proto" />
    <Content Include="protos\google\api\billing.proto" />
    <Content Include="protos\google\api\backend.proto" />
    <Content Include="protos\google\api\auth.proto" />
    <Content Include="protos\google\api\annotations.proto" />
    <Content Include="SetEnvVars.ps1" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ERH.FLIR\ERH.FLIR.csproj">
      <Project>{1dc4cf1a-ac29-4acd-97ba-d228dc353bbe}</Project>
      <Name>ERH.FLIR</Name>
    </ProjectReference>
    <ProjectReference Include="..\ERH.HeatScans.Reporting\ERH.HeatScans.Reporting.csproj">
      <Project>{1149835a-85e5-4d0f-b3e5-4d1753a06aa3}</Project>
      <Name>ERH.HeatScans.Reporting</Name>
    </ProjectReference>
  </ItemGroup>
  <PropertyGroup>
    <VisualStudioVersion Condition="'$(VisualStudioVersion)' == ''">10.0</VisualStudioVersion>
    <VSToolsPath Condition="'$(VSToolsPath)' == ''">$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)</VSToolsPath>
  </PropertyGroup>
  <Import Project="$(MSBuildBinPath)\Microsoft.CSharp.targets" />
  <Import Project="$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets" Condition="'$(VSToolsPath)' != ''" />
  <Import Project="$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v10.0\WebApplications\Microsoft.WebApplication.targets" Condition="false" />
  <ProjectExtensions>
    <VisualStudio>
      <FlavorProperties GUID="{349c5851-65df-11da-9384-00065b846f21}">
        <WebProjectProperties>
          <UseIIS>True</UseIIS>
          <AutoAssignPort>False</AutoAssignPort>
          <DevelopmentServerPort>52154</DevelopmentServerPort>
          <DevelopmentServerVPath>/</DevelopmentServerVPath>
          <IISUrl>https://localhost:7209</IISUrl>
          <OverrideIISAppRootUrl>True</OverrideIISAppRootUrl>
          <IISAppRootUrl>https://localhost:7209/</IISAppRootUrl>
          <NTLMAuthentication>False</NTLMAuthentication>
          <UseCustomServer>False</UseCustomServer>
          <CustomServerUrl>
          </CustomServerUrl>
          <SaveServerSettingsInUserFile>False</SaveServerSettingsInUserFile>
        </WebProjectProperties>
      </FlavorProperties>
    </VisualStudio>
  </ProjectExtensions>
</Project>
```

### Step 4: Delete packages.config

Delete the file `ERH.HeatScans.Reporting.Server.Framework\packages.config`.

### Step 5: Reopen Visual Studio

1. Open the solution in Visual Studio
2. Right-click the solution ? **Restore NuGet Packages**
3. Clean the solution
4. Rebuild the solution

### Step 6: Verify the Fix

After rebuilding, check the `bin` directory for:
- All managed assemblies should still be present
- Native dependencies from the `Flir.Atlas.Cronos` package should now be in the bin directory or subdirectories

## If Native Dependencies Are Still Missing

If the error persists after migration, the `Flir.Atlas.Cronos` NuGet package may not include the required native DLLs. In that case:

### Option A: Install Visual C++ Redistributable

Download and install the **Microsoft Visual C++ Redistributable** (x64) from:
https://aka.ms/vs/17/release/vc_redist.x64.exe

This provides the required runtime DLLs system-wide.

### Option B: Manually Copy Native DLLs

If you have the FLIR SDK installed elsewhere:

1. Locate the native DLL files (typically in the FLIR SDK installation directory):
   - `vcruntime140.dll`
   - `msvcp140.dll`
   - `msvcp140_1.dll`
   - Any FLIR-specific native DLLs (e.g., `IRSDK_*.dll`)

2. Copy them to the `bin` directory: `ERH.HeatScans.Reporting.Server.Framework\bin\`

### Option C: Use the Web.config Workaround

The `Web.config` has already been updated to prevent automatic loading of the problematic assembly during startup:

```xml
<compilation debug="true" targetFramework="4.8.1">
  <assemblies>
    <remove assembly="Flir.Cronos.Filter.Adapter" />
  </assemblies>
</compilation>
```

This prevents ASP.NET from loading the assembly automatically. You'll need to load it explicitly in your code when needed, after ensuring native dependencies are available.

## Key Changes Made

1. **Added** `<RestoreProjectStyle>PackageReference</RestoreProjectStyle>` to enable the new format
2. **Converted** all `<Reference>` elements with HintPath from packages folder to `<PackageReference>` elements
3. **Removed** old NuGet package Import targets at the end of the file
4. **Kept** manual references to FLIR libraries from `..\lib\` folder (these are not from NuGet)
5. **Updated** `Web.config` to exclude the problematic assembly from automatic loading

## Benefits of PackageReference

- Better handling of native dependencies
- Transitive dependency resolution
- Cleaner project files
- Global package cache (no more solution-level `packages` folder)
- Better performance during restore operations
- Proper content file deployment
