# Xsolla AccelByte SDK

A Unity package (`com.xsolla.accelbyte`) that bridges [Xsolla's commerce SDK](https://github.com/xsolla/store-unity-sdk) authentication into [AccelByte Gaming Services](https://github.com/AccelByte/accelbyte-unity-sdk).

This package keeps AccelByte as the game backend SDK and adds a focused Xsolla authentication bridge. It does not replace or mirror the wider AccelByte SDK API surface.

## Requirements

- Unity 2021.3 or later
- [`com.xsolla.commerce-sdk`](https://github.com/xsolla/store-unity-sdk) `3.0.1`
- `com.unity.nuget.newtonsoft-json` `3.0.1`

## Installation

This package embeds the AccelByte SDKs as git submodules, so clone or add as a submodule rather than downloading a zip:

```bash
git submodule update --init --recursive
```

Then import the package into your Unity project via the Package Manager (`Window > Package Manager > + > Add package from disk...`, pointing at this package's `package.json`), or reference it by git URL/local path in your project's `manifest.json`.

## Configuring The SDK

AccelByte configuration uses the official AccelByte SDK config files and editor windows. From the Unity Editor's menu bar, use the AccelByte settings entries:

- `AccelByte > Edit Client Settings` - client-side config (base URL, redirect URI, namespace, OAuth client, service URLs, etc.)
- `AccelByte > Edit Server Settings` - dedicated server config

Xsolla widget/login configuration is managed by the official Xsolla SDK through `Resources/XsollaSettings`.

## Usage

Use the AccelByte SDK normally for backend services. Xsolla auth is exposed as an extension on the AccelByte client API:

```csharp
using AccelByte.Core;
using Xsolla.AccelByte;

var xsollaAuth = AccelByteSDK.GetClientRegistry().GetApi().GetXsollaAuth();
```

The package also provides a high-level convenience facade:

```csharp
XsollaAccelByteSDK.Auth.LoginWithXsollaAccount(callback);
```

### Authenticating With Xsolla

```csharp
AccelByteSDK.GetClientRegistry().GetApi().GetXsollaAuth().LoginWithXsollaAccount(result =>
{
    if (result.IsError)
    {
        Debug.LogError(result.Error.error_description);
        return;
    }

    // result.Value is the AccelByte TokenData.
});
```

`XsollaAuth` also exposes `LoginWithXsollaSilentAuth`, `LoginWithXsollaAccessToken`, and `Logout`.
