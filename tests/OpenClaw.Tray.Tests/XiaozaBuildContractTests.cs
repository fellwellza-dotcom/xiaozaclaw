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
}
