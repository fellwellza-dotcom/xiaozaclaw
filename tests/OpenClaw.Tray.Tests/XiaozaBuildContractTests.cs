namespace OpenClaw.Tray.Tests;

public sealed class XiaozaBuildContractTests
{
    private static string RepoRoot => TestRepositoryPaths.GetRepositoryRoot();

    private static string Read(params string[] pathParts)
        => File.ReadAllText(Path.Combine(new[] { RepoRoot }.Concat(pathParts).ToArray()));

    [Fact]
    public void AppIdentity_DefinesIndependentXiaozaProfile()
    {
        var source = Read("src", "OpenClaw.Tray.WinUI", "AppIdentity.cs");

        Assert.Contains("#if XIAOZACLAW_BUILD", source);
        Assert.Contains("DisplayName = \"xiaozaclaw\"", source);
        Assert.Contains("PackageIdentityName = \"xiaozaclaw.Desktop\"", source);
        Assert.Contains("DataDirectoryName = \"xiaozaclaw\"", source);
        Assert.Contains("ProtocolScheme = \"xiaozaclaw\"", source);
        Assert.Contains("SetupDistroName = \"xiaozaclawGateway\"", source);
        Assert.Contains("SetupGatewayPort = 18889", source);
        Assert.Contains("IsXiaozaClaw => true", source);
    }

    [Fact]
    public void Project_EmitsXiaozaBuildIdentityMarkerAndMetadata()
    {
        var project = Read("src", "OpenClaw.Tray.WinUI", "OpenClaw.Tray.WinUI.csproj");

        Assert.Contains("Condition=\"'$(XiaozaBuild)' == 'true'\"", project);
        Assert.Contains("XIAOZACLAW_BUILD", project);
        Assert.Contains("<AppIdentityMarker>xiaozaclaw</AppIdentityMarker>", project);
        Assert.Contains("<AssemblyTitle>xiaozaclaw</AssemblyTitle>", project);
        Assert.Contains("xiaozaclaw.app.manifest", project);
        Assert.Contains("<_AlternateIdentityName Condition=\"'$(XiaozaBuild)' == 'true'\">xiaozaclaw.Desktop</_AlternateIdentityName>", project);
        Assert.Contains("<_AlternateProtocolName Condition=\"'$(XiaozaBuild)' == 'true'\">xiaozaclaw</_AlternateProtocolName>", project);
    }

    [Fact]
    public void BuildAndLaunchScripts_ExposeExplicitXiaozaSwitch()
    {
        var build = Read("build.ps1");
        var run = Read("run-app-local.ps1");
        var installerBuild = Read("scripts", "build-inno-local.ps1");

        Assert.Contains("[switch]$XiaozaBuild", build);
        Assert.Contains("-p:XiaozaBuild=true", build);
        Assert.Contains("[switch]$Xiaoza", run);
        Assert.Contains("$buildArgs[\"XiaozaBuild\"] = $true", run);
        Assert.Contains("if ($Xiaoza) { \"xiaozaclaw\" }", run);
        Assert.Contains("[switch]$Xiaoza", installerBuild);
        Assert.Contains("\"-p:XiaozaBuild=$($Xiaoza.IsPresent.ToString().ToLowerInvariant())\"", installerBuild);
        Assert.Contains("\"/DXiaozaBuild=1\"", installerBuild);
    }

    [Fact]
    public void UnpackagedManifest_UsesXiaozaIdentity()
    {
        var manifest = Read("src", "OpenClaw.Tray.WinUI", "xiaozaclaw.app.manifest");

        Assert.Contains("name=\"xiaozaclaw.Desktop\"", manifest);
        Assert.DoesNotContain("name=\"OpenClaw.Companion\"", manifest);
    }

    [Fact]
    public void UpdateCoordinator_DoesNotInstallOpenClawReleasesOverXiaozaclaw()
    {
        var source = Read("src", "OpenClaw.Tray.WinUI", "Services", "UpdateCoordinator.cs");

        Assert.Contains("AppIdentity.IsDev || AppIdentity.IsXiaozaClaw", source);
        Assert.Contains("Skipping OpenClaw release-channel update check", source);
    }

    [Fact]
    public void SetupWindow_ReceivesXiaozaclawChineseBranding()
    {
        var app = Read("src", "OpenClaw.Tray.WinUI", "App.xaml.cs");
        var setupWindow = Read("src", "OpenClaw.SetupEngine.UI", "SetupWindow.xaml.cs");

        Assert.Contains("SetupBranding.XiaozaclawSimplifiedChinese", app);
        Assert.Contains("Branding = branding ?? SetupBranding.OpenClaw", setupWindow);
        Assert.Contains("SetupTitleText.Text = Branding.SetupTitle", setupWindow);
    }

    [Fact]
    public void XiaozaclawResources_CoverEveryUpstreamChineseKey()
    {
        var upstream = ReadResources(
            "src", "OpenClaw.Tray.WinUI", "Strings", "zh-cn", "Resources.resw");
        var xiaozaclaw = ReadResources(
            "src", "OpenClaw.Tray.WinUI", "XiaozaclawStrings", "Resources.resw");

        Assert.Equal(upstream.Keys.OrderBy(static key => key), xiaozaclaw.Keys.OrderBy(static key => key));
        Assert.Equal("xiaozaclaw Windows 桌面助手", xiaozaclaw["TitleText.Text"]);
        Assert.Equal("设置", xiaozaclaw["SettingsPage_Settings.Text"]);
        Assert.Equal("关于", xiaozaclaw["SettingsPage_About.Text"]);
        Assert.DoesNotContain(
            xiaozaclaw.Values,
            static value => value.Contains("OpenClaw", StringComparison.Ordinal));
    }

    [Fact]
    public void XiaozaclawBuild_UsesOnlyDedicatedChinesePriResources()
    {
        var project = Read("src", "OpenClaw.Tray.WinUI", "OpenClaw.Tray.WinUI.csproj");
        var props = Read("Directory.Build.props");
        var bootstrap = Read("src", "OpenClaw.Tray.WinUI", "App.xaml.cs");

        Assert.Contains("XiaozaclawStrings\\Resources.resw", project);
        Assert.Contains("<DefaultLanguage>zh-CN</DefaultLanguage>", project);
        Assert.DoesNotContain("<Link>Strings\\zh-cn\\Resources.resw</Link>", project);
        Assert.Contains("Strings\\**\\*.resw;XiaozaclawStrings\\**\\*.resw", props);
        Assert.Contains("LocalizationBootstrap.Configure(AppIdentity.IsXiaozaClaw)", bootstrap);
    }

    [Fact]
    public void SetupWizard_AppliesChineseProductLocalization()
    {
        var window = Read("src", "OpenClaw.SetupEngine.UI", "SetupWindow.xaml.cs");
        var localizer = Read("src", "OpenClaw.SetupEngine.UI", "SetupTextLocalizer.cs");

        Assert.Contains("RootFrame.Navigated += ApplyProductLocalization", window);
        Assert.Contains("SetupTextLocalizer.Attach(element, Branding, _config)", window);
        Assert.Contains("[\"Set up the WSL gateway\"] = \"设置 WSL 网关\"", localizer);
        Assert.Contains("[\"OpenClaw onboard\"] = \"xiaozaclaw 配置\"", localizer);
        Assert.Contains(".Replace(\"OpenClaw\", productName", localizer);
        Assert.Contains("config?.GatewayPort is > 0", localizer);
        Assert.Contains("root.Loaded += loaded", localizer);
        Assert.DoesNotContain("root.LayoutUpdated +=", localizer);
    }

    [Fact]
    public void Installer_UsesChineseLanguageAndShortcuts()
    {
        var installer = Read("installer.iss");

        Assert.Contains("MessagesFile: \"installer\\third-party\\ChineseSimplified.isl\"", installer);
        Assert.Contains("#define MyAppName \"xiaozaclaw\"", installer);
        Assert.Contains("MIT License", Read(
            "installer", "third-party", "LICENSE.Inno-Setup-Chinese-Simplified-Translation.txt"));
        Assert.Contains("#define MyGatewayShortcut \"xiaozaclaw 网关设置\"", installer);
        Assert.Contains("#define MySettingsShortcut \"xiaozaclaw 设置\"", installer);
        Assert.Contains("#define MyChatShortcut \"xiaozaclaw 对话\"", installer);
        Assert.Contains("#define MyCheckUpdatesShortcut \"检查更新\"", installer);
        Assert.Contains("#define MyStartupTaskDescription \"Windows 启动时运行 xiaozaclaw\"", installer);
    }

    [Fact]
    public void MainWindow_DoesNotFlashEnglishStatusOrAboutName()
    {
        var hub = Read("src", "OpenClaw.Tray.WinUI", "Windows", "HubWindow.xaml");
        var settings = Read("src", "OpenClaw.Tray.WinUI", "Pages", "SettingsPage.xaml");

        Assert.DoesNotContain("StatusPillText\" Text=\"Disconnected", hub);
        Assert.Contains("x:Uid=\"TitleText\"", settings);
    }

    private static IReadOnlyDictionary<string, string> ReadResources(params string[] pathParts)
    {
        var document = System.Xml.Linq.XDocument.Parse(Read(pathParts));
        return document.Root!
            .Elements("data")
            .ToDictionary(
                static element => element.Attribute("name")!.Value,
                static element => element.Element("value")?.Value ?? string.Empty,
                StringComparer.Ordinal);
    }
}
