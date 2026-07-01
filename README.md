# Xsolla Backend SDK

A Unity package (`com.xsolla.xsollabackendsdk`) that bridges [Xsolla's commerce SDK](https://github.com/xsolla/store-unity-sdk) with [AccelByte's gaming services SDK](https://github.com/AccelByte/accelbyte-unity-sdk). It lets a game built on Xsolla's storefront authenticate players into AccelByte's backend services (lobby, matchmaking, sessions, stats, and more) using a Xsolla access token.

## Requirements

- Unity 2021.3 or later
- [`com.xsolla.commerce-sdk`](https://github.com/xsolla/store-unity-sdk) `3.0.1`
- `com.unity.nuget.newtonsoft-json` `3.0.1`

## Installation

This package embeds the AccelByte SDKs as git submodules, so clone (or add as a submodule) rather than downloading a zip:

```bash
git submodule update --init --recursive
```

Then import the package into your Unity project via the Package Manager (`Window > Package Manager > + > Add package from disk...`, pointing at this package's `package.json`), or reference it by git URL/local path in your project's `manifest.json`.

## Configuring the SDK

Configuration is done entirely through the Xsolla-branded editor windows, which write directly into the underlying AccelByte SDK config files — there's no separate config asset to manage. From the Unity Editor's menu bar, go to:

- `Xsolla > Backend SDK > Edit Client Settings` — client-side config (base URL, redirect URI, namespace, OAuth client, service URLs, etc.)
- `Xsolla > Backend SDK > Edit Server Settings` — dedicated server config

AccelByte's own settings menu items are hidden to avoid confusion; use the Xsolla windows above instead.

## Usage

All access goes through the `XsollaBackendSDK` singleton:

```csharp
// Client-side (player)
var clientApi = XsollaBackendSDK.Instance.GetClientRegistry().GetApi();

// Server-side (dedicated server)
var serverApi = XsollaBackendSDK.Instance.GetServerRegistry().GetApi();
```

### Authenticating with a Xsolla token

`ApiClient.GetXsollaAuth()` is the SDK's one Xsolla-specific feature: it exchanges a Xsolla access token for an AccelByte session.

```csharp
clientApi.GetXsollaAuth().AuthWithXsollaWidget((result) =>
{
    if (result.IsError)
    {
        Debug.LogError(result.Error.error_description);
        return;
    }

    // result.Value.XsollaAccessToken   -> the Xsolla token
    // result.Value.GamingServiceToken  -> the AccelByte TokenData
});
```

`XsollaAuth` also exposes `AuthBySavedToken`, `AuthViaXsollaLauncher`, `AuthViaSocialNetwork`, `SilentAuth`, `AuthWithXsollaAccessToken`, and `LogOut`.

Every other AccelByte service (lobby, session, stats, leaderboards, etc.) is available unchanged through `ApiClient` / `ApiServer`, or via `.Get()` on either for the raw AccelByte type.

