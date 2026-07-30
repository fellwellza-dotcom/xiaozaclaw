using OpenClawTray.Services;
using OpenClaw.TestSupport;

namespace OpenClaw.Tray.Tests;

public sealed class EnterpriseWorkspaceStoreTests
{
    [Fact]
    public void EnsureInitialized_CreatesFiveTemplatesAndPersistsWorkspace()
    {
        using var temp = new TempDirectory();
        var store = new EnterpriseWorkspaceStore(temp.Path);

        var snapshot = store.EnsureInitialized();
        var reloaded = new EnterpriseWorkspaceStore(temp.Path).Load();

        Assert.Equal(5, snapshot.Templates.Count);
        Assert.Single(snapshot.Templates, template => template.IsAvailable);
        Assert.Equal(snapshot.Templates.Select(template => template.Id), reloaded.Templates.Select(template => template.Id));
    }

    [Fact]
    public void MeetingMinutesLifecycle_RequiresAdminApprovalAndWritesContentFreeAudit()
    {
        using var temp = new TempDirectory();
        var store = new EnterpriseWorkspaceStore(temp.Path);
        store.EnsureInitialized();
        const string notes = "客户项目将在周五前确定负责人。";

        var task = store.CreateMeetingMinutesTask("销售例会", notes, "王管理员");
        Assert.Equal(EnterpriseTaskStatus.Draft, task.Status);
        Assert.Throws<InvalidOperationException>(() => store.ApproveDelivery(task.Id, "王管理员", EnterpriseRole.Administrator));

        store.MarkProcessing(task.Id, "王管理员");
        store.SubmitForApproval(task.Id, "王管理员");
        Assert.Throws<InvalidOperationException>(() => store.ApproveDelivery(task.Id, "普通成员", EnterpriseRole.Member));
        var delivered = store.ApproveDelivery(task.Id, "王管理员", EnterpriseRole.Administrator);
        var reloaded = store.Load();

        Assert.Equal(EnterpriseTaskStatus.Delivered, delivered.Status);
        Assert.Equal(EnterpriseTaskStatus.Delivered, Assert.Single(reloaded.Tasks).Status);
        Assert.All(reloaded.AuditEvents, audit => Assert.DoesNotContain(notes, audit.Action + audit.Actor + audit.TaskId));
        Assert.All(reloaded.AuditEvents, audit => Assert.StartsWith("task.", audit.Action, StringComparison.Ordinal));
    }

    [Fact]
    public void TaskPolicy_OnlyAllowsReviewedDeliveryFromApprovalQueue()
    {
        Assert.True(EnterpriseTaskPolicy.CanTransition(EnterpriseTaskStatus.Draft, EnterpriseTaskStatus.InProgress));
        Assert.True(EnterpriseTaskPolicy.CanTransition(EnterpriseTaskStatus.InProgress, EnterpriseTaskStatus.AwaitingApproval));
        Assert.False(EnterpriseTaskPolicy.CanTransition(EnterpriseTaskStatus.AwaitingApproval, EnterpriseTaskStatus.Delivered, EnterpriseRole.Member));
        Assert.True(EnterpriseTaskPolicy.CanTransition(EnterpriseTaskStatus.AwaitingApproval, EnterpriseTaskStatus.Delivered, EnterpriseRole.Owner));
        Assert.True(EnterpriseTaskPolicy.CanTransition(EnterpriseTaskStatus.AwaitingApproval, EnterpriseTaskStatus.NeedsChanges, EnterpriseRole.Administrator));
        Assert.True(EnterpriseTaskPolicy.CanTransition(EnterpriseTaskStatus.NeedsChanges, EnterpriseTaskStatus.InProgress));
        Assert.False(EnterpriseTaskPolicy.CanTransition(EnterpriseTaskStatus.Delivered, EnterpriseTaskStatus.InProgress));
    }
}
