namespace OpenClaw.Tray.Tests;

public sealed class LocalizationBootstrapTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("en-us")]
    [InlineData("fr-fr")]
    public void XiaozaclawBuild_AlwaysUsesSimplifiedChinese(string? environmentOverride)
    {
        Assert.Equal(
            "zh-cn",
            OpenClawTray.Helpers.XiaozaLanguagePolicy.ResolveLanguage(
                isXiaozaclaw: true,
                environmentOverride));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ZH-CN ", "zh-cn")]
    [InlineData("fr-FR", "fr-fr")]
    public void UpstreamBuild_PreservesOptionalEnvironmentOverride(string? input, string? expected)
    {
        Assert.Equal(
            expected,
            OpenClawTray.Helpers.XiaozaLanguagePolicy.ResolveLanguage(
                isXiaozaclaw: false,
                input));
    }
}
