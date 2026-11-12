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

### Authenticating With Xsolla ID

Xsolla ID login needs the game namespace to use Xsolla ID in the AGS Admin Portal (**Identity & Access > Xsolla Authentication**), with the same Client ID as below. While Xsolla ID is active, AGS rejects Xsolla Login tokens for that namespace.

If the game already has an Xsolla ID `id_token`, for example from the Xsolla ID web SDK in a WebGL build, log in with it on any platform:

```csharp
XsollaAccelByteSDK.Auth.LoginWithXsollaIdToken(idToken, callback);
```

To run the whole login, call `LoginWithXsollaId` on the main thread. It opens the system browser on the Xsolla ID login page, receives the authorization code on a loopback listener at `http://127.0.0.1:<port><path>`, exchanges it with PKCE, checks the `id_token`, and logs in to AGS. It works in Windows, Mac and Linux standalone builds and in the Editor.

```csharp
var options = new XsollaIdOptions
{
    ClientId = "<XSOLLA_ID_CLIENT_ID>",
    RedirectPort = 8081, // Required: the port of the registered redirect URI
    // Optional, defaults shown
    Issuer = "https://id.xsolla.com",
    Scope = "openid profile email",
    RedirectHost = "127.0.0.1",
    RedirectPath = "/callback",
    LoginTimeoutSeconds = 300
};

XsollaAccelByteSDK.Auth.LoginWithXsollaId(options, callback);
```

The wrapper cannot detect a closed browser tab, so call `CancelXsollaIdLogin()` to stop a pending login; the callback then receives the cancelled error.

The redirect URI `http://<host>:<port><path>` must be registered for the Xsolla ID client, for example `http://127.0.0.1:<port>/callback`. Xsolla ID matches it exactly, port included, so set `RedirectPort` to the registered port; the default `0` picks a random free port, which does not match. Keep `RedirectHost = "127.0.0.1"` as RFC 8252 recommends; set `"localhost"` only to match a redirect URI that is registered with `localhost`, for example during testing. The listener always binds `127.0.0.1`. `Scope` must contain `openid`; `profile` and `email` give AGS the player's name, picture and email.
