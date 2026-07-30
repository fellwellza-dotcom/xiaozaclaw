namespace OpenClaw.Tray.Tests;

public sealed class EnterpriseWorkbenchPageContractTests
{
    [Fact]
    public void Workbench_IsRoutedAndExposesTheAuditableMeetingMinutesFlow()
    {
        var root = TestRepositoryPaths.GetRepositoryRoot();
        var hub = File.ReadAllText(Path.Combine(root, "src", "OpenClaw.Tray.WinUI", "Windows", "HubWindow.xaml.cs"));
        var page = File.ReadAllText(Path.Combine(root, "src", "OpenClaw.Tray.WinUI", "Pages", "EnterpriseWorkbenchPage.xaml.cs"));
        var xaml = File.ReadAllText(Path.Combine(root, "src", "OpenClaw.Tray.WinUI", "Pages", "EnterpriseWorkbenchPage.xaml"));

        Assert.Contains("\"workbench\" => typeof(EnterpriseWorkbenchPage)", hub);
        Assert.Contains("EnterpriseWorkbenchPageMarker", xaml);
        Assert.Contains("SendChatMessageAsync(EnterpriseTemplates.BuildMeetingMinutesPrompt(task))", page);
        Assert.Contains("SubmitForApproval", page);
        Assert.Contains("ApproveDelivery", page);
        Assert.Contains("RequestChanges", page);
        Assert.Contains("OnCreateRevisionClick", page);
    }
}
