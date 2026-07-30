namespace OpenClawTray.Helpers;

internal static class XiaozaLanguagePolicy
{
    internal static string? ResolveLanguage(bool isXiaozaclaw, string? environmentOverride)
    {
        if (isXiaozaclaw)
            return "zh-cn";

        return string.IsNullOrWhiteSpace(environmentOverride)
            ? null
            : environmentOverride.Trim().ToLowerInvariant();
    }
}
