using OpenClawTray.Services;

namespace OpenClawTray.Helpers;

/// <summary>
/// Applies the product build's language policy before WinUI resources are loaded.
/// The xiaozaclaw build is a Chinese product and always uses its dedicated zh-CN
/// resource map. Upstream builds keep the existing opt-in test override behavior.
/// </summary>
internal static class LocalizationBootstrap
{
    private static readonly HashSet<string> AllowedLocales = new(StringComparer.OrdinalIgnoreCase)
    {
        "en-us",
        "fr-fr",
        "nl-nl",
        "zh-cn",
        "zh-tw",
    };

    public static void Configure(bool isXiaozaclaw)
    {
        var requested = XiaozaLanguagePolicy.ResolveLanguage(
            isXiaozaclaw,
            Environment.GetEnvironmentVariable("OPENCLAW_LANGUAGE"));
        if (requested is null)
            return;

        if (!AllowedLocales.Contains(requested))
        {
            Logger.Warn($"[App] Ignoring invalid OPENCLAW_LANGUAGE value: {requested}");
            return;
        }

        // WinUI's resource manager is initialized after Application startup. Applying
        // ApplicationLanguages.PrimaryLanguageOverride here can fault Microsoft.UI.Xaml
        // before the logger or a window exists. The dedicated xiaozaclaw PRI resources
        // and the existing helper override provide the intended language without that
        // early runtime mutation.
        LocalizationHelper.SetLanguageOverride(requested);
    }
}
