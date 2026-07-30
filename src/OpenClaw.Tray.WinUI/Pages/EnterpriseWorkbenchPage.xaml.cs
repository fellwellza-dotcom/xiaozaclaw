using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using OpenClaw.Shared;
using OpenClawTray.Services;

namespace OpenClawTray.Pages;

public sealed partial class EnterpriseWorkbenchPage : Page
{
    private static App CurrentApp => (App)Application.Current!;
    private EnterpriseWorkspaceStore? _store;
    private EnterpriseWorkspaceSnapshot? _snapshot;

    public EnterpriseWorkbenchPage()
    {
        InitializeComponent();
        Loaded += (_, _) => Initialize();
    }

    public void Initialize()
    {
        var settingsDirectory = CurrentApp.Settings?.SettingsDirectory;
        if (string.IsNullOrWhiteSpace(settingsDirectory))
        {
            ShowInfo("工作台暂时不可用。应用设置尚未准备完成。", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            _store ??= new EnterpriseWorkspaceStore(settingsDirectory);
            _snapshot = _store.EnsureInitialized();
            RefreshView();
        }
        catch (Exception ex)
        {
            Services.Logger.Warn($"[EnterpriseWorkbench] Failed to load local workspace: {ex.Message}");
            ShowInfo("工作台记录无法读取。请在管理中心检查本机数据目录。", InfoBarSeverity.Error);
        }
    }

    private void OnNewMeetingMinutesClick(object sender, RoutedEventArgs e)
    {
        MeetingFormCard.Visibility = Visibility.Visible;
        MeetingTitleBox.Focus(FocusState.Programmatic);
    }

    private void OnCancelMeetingMinutesClick(object sender, RoutedEventArgs e)
    {
        MeetingFormCard.Visibility = Visibility.Collapsed;
        MeetingTitleBox.Text = string.Empty;
        MeetingNotesBox.Text = string.Empty;
    }

    private void OnStartMeetingTaskClick(object sender, RoutedEventArgs e) =>
        AsyncEventHandlerGuard.Run(
            StartMeetingTaskAsync,
            new OpenClawTray.AppLogger(),
            nameof(OnStartMeetingTaskClick));

    private async Task StartMeetingTaskAsync()
    {
        if (_store is null || _snapshot is null)
            return;

        if (string.IsNullOrWhiteSpace(MeetingNotesBox.Text))
        {
            ShowInfo("请先填写会议记录。", InfoBarSeverity.Warning);
            return;
        }

        StartMeetingTaskButton.IsEnabled = false;
        try
        {
            var task = _store.CreateMeetingMinutesTask(
                MeetingTitleBox.Text,
                MeetingNotesBox.Text,
                _snapshot.CurrentMemberName);
            var client = CurrentApp.GatewayClient;
            if (client is null || !client.IsConnectedToGateway)
            {
                ShowInfo("草稿已保存。连接 Gateway 后可在此任务中启动处理。", InfoBarSeverity.Warning);
            }
            else
            {
                await client.SendChatMessageAsync(EnterpriseTemplates.BuildMeetingMinutesPrompt(task));
                _store.MarkProcessing(task.Id, _snapshot.CurrentMemberName);
                ShowInfo("任务已发送到当前对话。确认 AI 结果后，请在本页提交审批。", InfoBarSeverity.Success);
            }

            MeetingFormCard.Visibility = Visibility.Collapsed;
            MeetingTitleBox.Text = string.Empty;
            MeetingNotesBox.Text = string.Empty;
            ReloadAndRefresh();
        }
        catch (Exception ex)
        {
            Services.Logger.Warn($"[EnterpriseWorkbench] Failed to start meeting task: {ex.Message}");
            ShowInfo("任务已保存，但发送到 Gateway 失败。请检查连接后重试。", InfoBarSeverity.Error);
            ReloadAndRefresh();
        }
        finally
        {
            StartMeetingTaskButton.IsEnabled = true;
        }
    }

    private void OnStartSavedTaskClick(object sender, RoutedEventArgs e) =>
        AsyncEventHandlerGuard.Run(
            () => StartSavedTaskAsync(sender),
            new OpenClawTray.AppLogger(),
            nameof(OnStartSavedTaskClick));

    private async Task StartSavedTaskAsync(object sender)
    {
        if (_store is null || _snapshot is null || sender is not Button { Tag: string taskId })
            return;

        var task = _snapshot.Tasks.FirstOrDefault(item => item.Id == taskId);
        var client = CurrentApp.GatewayClient;
        if (task is null || client is null || !client.IsConnectedToGateway)
        {
            ShowInfo("请先连接 Gateway，再启动此任务。", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            await client.SendChatMessageAsync(EnterpriseTemplates.BuildMeetingMinutesPrompt(task));
            _store.MarkProcessing(taskId, _snapshot.CurrentMemberName);
            ShowInfo("任务已发送到当前对话。", InfoBarSeverity.Success);
            ReloadAndRefresh();
        }
        catch (Exception ex)
        {
            Services.Logger.Warn($"[EnterpriseWorkbench] Failed to start saved task: {ex.Message}");
            ShowInfo("发送失败。请检查 Gateway 连接后重试。", InfoBarSeverity.Error);
        }
    }

    private void OnSubmitForApprovalClick(object sender, RoutedEventArgs e)
    {
        if (_store is null || _snapshot is null || sender is not Button { Tag: string taskId }) return;
        try
        {
            _store.SubmitForApproval(taskId, _snapshot.CurrentMemberName);
            ShowInfo("任务已提交审批。", InfoBarSeverity.Success);
            ReloadAndRefresh();
        }
        catch (Exception ex)
        {
            Services.Logger.Warn($"[EnterpriseWorkbench] Failed to submit task: {ex.Message}");
            ShowInfo("无法提交审批，请刷新后重试。", InfoBarSeverity.Error);
        }
    }

    private void OnApproveDeliveryClick(object sender, RoutedEventArgs e)
    {
        if (_store is null || _snapshot is null || sender is not Button { Tag: string taskId }) return;
        try
        {
            _store.ApproveDelivery(taskId, _snapshot.CurrentMemberName, _snapshot.CurrentMemberRole);
            ShowInfo("任务已批准交付，记录已写入本机审计日志。", InfoBarSeverity.Success);
            ReloadAndRefresh();
        }
        catch (Exception ex)
        {
            Services.Logger.Warn($"[EnterpriseWorkbench] Failed to approve task: {ex.Message}");
            ShowInfo("当前角色不能批准交付，或任务状态已发生变化。", InfoBarSeverity.Warning);
        }
    }

    private void OnRequestChangesClick(object sender, RoutedEventArgs e)
    {
        if (_store is null || _snapshot is null || sender is not Button { Tag: string taskId }) return;
        try
        {
            _store.RequestChanges(taskId, _snapshot.CurrentMemberName, _snapshot.CurrentMemberRole, "请补充或修正后重新提交。");
            ShowInfo("任务已退回补充。", InfoBarSeverity.Informational);
            ReloadAndRefresh();
        }
        catch (Exception ex)
        {
            Services.Logger.Warn($"[EnterpriseWorkbench] Failed to request changes: {ex.Message}");
            ShowInfo("当前角色不能退回任务，或任务状态已发生变化。", InfoBarSeverity.Warning);
        }
    }

    private void OnCreateRevisionClick(object sender, RoutedEventArgs e)
    {
        if (_snapshot is null || sender is not Button { Tag: string taskId }) return;
        var task = _snapshot.Tasks.FirstOrDefault(item => item.Id == taskId);
        if (task is null) return;

        MeetingTitleBox.Text = $"修订：{task.Title}";
        MeetingNotesBox.Text = string.IsNullOrWhiteSpace(task.ReviewNote)
            ? task.SourceText
            : $"{task.SourceText}\n\n审批意见：{task.ReviewNote}";
        MeetingFormCard.Visibility = Visibility.Visible;
        MeetingNotesBox.Focus(FocusState.Programmatic);
        ShowInfo("已载入原记录和审批意见。修改后启动新的修订任务，原任务保留完整审计记录。", InfoBarSeverity.Informational);
    }

    private void ReloadAndRefresh()
    {
        if (_store is null) return;
        _snapshot = _store.Load();
        RefreshView();
    }

    private void RefreshView()
    {
        if (_snapshot is null) return;
        TemplatesRepeater.ItemsSource = _snapshot.Templates.Select(template => new TemplateCard(
            template.Name,
            template.Description,
            template.IsAvailable ? "已可用" : "即将开放"));
        TemplateCountText.Text = _snapshot.Templates.Count(template => template.IsAvailable).ToString();
        ApprovalCountText.Text = _snapshot.Tasks.Count(task => task.Status == EnterpriseTaskStatus.AwaitingApproval).ToString();
        DeliveredCountText.Text = _snapshot.Tasks.Count(task => task.Status == EnterpriseTaskStatus.Delivered).ToString();
        RoleText.Text = $"当前身份：{RoleTextFor(_snapshot.CurrentMemberRole)}。此版本在本机保存任务与审计记录，多账户身份接入将在企业连接器阶段提供。";

        TasksPanel.Children.Clear();
        if (_snapshot.Tasks.Count == 0)
        {
            TasksPanel.Children.Add(new TextBlock
            {
                Text = "还没有任务。选择“新建会议纪要”开始第一项工作。",
                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            return;
        }

        foreach (var task in _snapshot.Tasks.Take(12))
            TasksPanel.Children.Add(BuildTaskCard(task));
    }

    private UIElement BuildTaskCard(EnterpriseWorkTask task)
    {
        var root = new StackPanel { Spacing = 8 };
        root.Children.Add(new TextBlock
        {
            Text = task.Title,
            Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"],
            TextWrapping = TextWrapping.Wrap
        });
        root.Children.Add(new TextBlock
        {
            Text = $"会议纪要 · {StatusText(task.Status)} · 创建于 {task.CreatedAtUtc.ToLocalTime():g}",
            Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            TextWrapping = TextWrapping.Wrap
        });
        if (!string.IsNullOrWhiteSpace(task.ReviewNote))
        {
            root.Children.Add(new TextBlock
            {
                Text = $"审批意见：{task.ReviewNote}",
                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                TextWrapping = TextWrapping.Wrap
            });
        }

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        switch (task.Status)
        {
            case EnterpriseTaskStatus.Draft:
                actions.Children.Add(CreateActionButton("启动处理", task.Id, OnStartSavedTaskClick));
                break;
            case EnterpriseTaskStatus.NeedsChanges:
                actions.Children.Add(CreateActionButton("按意见新建修订", task.Id, OnCreateRevisionClick));
                break;
            case EnterpriseTaskStatus.InProgress:
                actions.Children.Add(CreateActionButton("提交审批", task.Id, OnSubmitForApprovalClick));
                break;
            case EnterpriseTaskStatus.AwaitingApproval:
                actions.Children.Add(CreateActionButton("批准交付", task.Id, OnApproveDeliveryClick));
                actions.Children.Add(CreateActionButton("退回补充", task.Id, OnRequestChangesClick));
                break;
        }
        if (actions.Children.Count > 0)
            root.Children.Add(actions);

        return new Border
        {
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            Background = (Brush)Application.Current.Resources["CardBackgroundFillColorSecondaryBrush"],
            Child = root
        };
    }

    private static Button CreateActionButton(string content, string taskId, RoutedEventHandler handler)
    {
        var button = new Button
        {
            Content = content,
            Tag = taskId,
            Padding = new Thickness(10, 4, 10, 4)
        };
        AutomationProperties.SetName(button, content);
        button.Click += handler;
        return button;
    }

    private void ShowInfo(string message, InfoBarSeverity severity)
    {
        WorkspaceInfoBar.Title = severity switch
        {
            InfoBarSeverity.Error => "需要处理",
            InfoBarSeverity.Warning => "请注意",
            InfoBarSeverity.Success => "已完成",
            _ => "提示"
        };
        WorkspaceInfoBar.Message = message;
        WorkspaceInfoBar.Severity = severity;
        WorkspaceInfoBar.IsOpen = true;
    }

    private static string StatusText(EnterpriseTaskStatus status) => status switch
    {
        EnterpriseTaskStatus.Draft => "草稿",
        EnterpriseTaskStatus.InProgress => "处理中",
        EnterpriseTaskStatus.AwaitingApproval => "待审批",
        EnterpriseTaskStatus.NeedsChanges => "待补充",
        EnterpriseTaskStatus.Delivered => "已交付",
        _ => "未知"
    };

    private static string RoleTextFor(EnterpriseRole role) => role switch
    {
        EnterpriseRole.Owner => "企业所有者",
        EnterpriseRole.Administrator => "管理员",
        EnterpriseRole.Member => "成员",
        _ => "只读成员"
    };

    private sealed record TemplateCard(string Name, string Description, string AvailabilityText);
}
