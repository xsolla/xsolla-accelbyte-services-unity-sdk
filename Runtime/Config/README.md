# Xsolla Backend SDK -- Configuration

## XsollaConfig Asset

`XsollaConfig` is a Unity `ScriptableObject` that holds every setting the Xsolla
Backend SDK needs at runtime: environment, game namespace, OAuth credentials, base
URL, redirect URI, and optional TURN / relay parameters.

Create the asset from the Unity menu:

    Assets > Create > Xsolla > Backend SDK Config

or open the dedicated editor window:

    Xsolla > Backend SDK > Settings

The editor window will offer to create the asset automatically if none exists yet.

## Where to save the asset

The asset **must** be placed at `Assets/Resources/XsollaConfig.asset` (the editor
window does this by default). This ensures the runtime loader can locate it via:

```csharp
Resources.Load<XsollaConfig>("XsollaConfig");
```

Any `Resources` folder recognized by Unity will work, but
`Assets/Resources/XsollaConfig.asset` is the recommended, default location.

## Configure() -- programmatic override

If you need to supply settings from code (e.g. from a server response or a
different ScriptableObject instance), call:

```csharp
XsollaBackendSDK.Configure(myConfig);
```

**before** the first access to `XsollaBackendSDK.Instance.GetClientRegistry()`,
`GetServerRegistry()`, `GetClientConfig()`, or `GetServerConfig()`.

A config injected via `Configure()` takes priority over the `Resources`-based
asset. Pass `null` to clear a previously injected config and fall back to the
automatic `Resources.Load` behaviour.

## No AccelByte JSON or AccelByte settings window needed

The `XsollaConfig` asset replaces the AccelByte JSON configuration files and the
AccelByte settings editor windows entirely. At startup the SDK maps the
`XsollaConfig` values into the underlying engine via `XsollaConfigMapper.Apply()`,
so there is no need to maintain separate AccelByte JSON config files or open the
AccelByte editor windows. All configuration is done through the single
`XsollaConfig` asset and the **Xsolla > Backend SDK > Settings** editor window.
