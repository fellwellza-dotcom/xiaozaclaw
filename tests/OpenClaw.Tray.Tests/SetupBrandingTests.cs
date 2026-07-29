using OpenClaw.SetupEngine.UI;

namespace OpenClaw.Tray.Tests;

public sealed class SetupBrandingTests
{
    [Fact]
    public void Xiaozaclaw_UsesIndependentChineseFirstRunCopy()
    {
        var branding = SetupBranding.XiaozaclawSimplifiedChinese;

        Assert.Equal("xiaozaclaw 安装向导", branding.SetupTitle);
        Assert.Equal("设置 xiaozaclaw", branding.WelcomeTitle);
        Assert.Equal("安装新的 WSL 网关？", branding.InstallGatewayTitle);
        Assert.Equal("继续", branding.Continue);
        Assert.Equal("取消", branding.Cancel);
        Assert.DoesNotContain("OpenClaw", branding.WelcomeDescription);
    }

    [Fact]
    public void Xiaozaclaw_NewGatewaySummary_PromisesIsolation()
    {
        var summary = SetupBranding.XiaozaclawSimplifiedChinese.BuildReplacementSummary(
            hasLocalGateway: false,
            hasDistro: false,
            distroName: null,
            hasIdentityFiles: false,
            preservedGatewayNames: [],
            englishFallback: "fallback");

        Assert.Equal("将创建新的本地 WSL 网关。现有配置不会受到影响。", summary);
    }

    [Fact]
    public void OpenClaw_DefaultCopyRemainsEnglish()
    {
        var branding = SetupBranding.OpenClaw;

        Assert.Equal("OpenClaw Setup", branding.SetupTitle);
        Assert.Equal("Set up OpenClaw", branding.WelcomeTitle);
        Assert.Equal("Continue", branding.Continue);
    }
}
