using System.Text.Json;

namespace OpenClawTray.Services;

/// <summary>
/// Local, auditable foundation for the business workbench. The gateway remains
/// responsible for AI execution. This store owns only team task metadata and
/// deliberately keeps audit events free of meeting content and model output.
/// </summary>
public sealed class EnterpriseWorkspaceStore
{
    private const int CurrentSchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly object _sync = new();

    public EnterpriseWorkspaceStore(string dataDirectory)
    {
        if (string.IsNullOrWhiteSpace(dataDirectory))
            throw new ArgumentException("A data directory is required.", nameof(dataDirectory));

        _filePath = Path.Combine(dataDirectory, "enterprise-workspace.json");
    }

    public EnterpriseWorkspaceSnapshot EnsureInitialized()
    {
        lock (_sync)
        {
            if (File.Exists(_filePath))
                return ReadUnsafe();

            var snapshot = EnterpriseWorkspaceSnapshot.CreateDefault(CurrentSchemaVersion);
            WriteUnsafe(snapshot);
            return snapshot;
        }
    }

    public EnterpriseWorkspaceSnapshot Load()
    {
        lock (_sync)
        {
            return File.Exists(_filePath)
                ? ReadUnsafe()
                : EnterpriseWorkspaceSnapshot.CreateDefault(CurrentSchemaVersion);
        }
    }

    public EnterpriseWorkTask CreateMeetingMinutesTask(string title, string sourceText, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
            throw new ArgumentException("Meeting notes cannot be empty.", nameof(sourceText));

        lock (_sync)
        {
            var snapshot = File.Exists(_filePath)
                ? ReadUnsafe()
                : EnterpriseWorkspaceSnapshot.CreateDefault(CurrentSchemaVersion);
            var now = DateTimeOffset.UtcNow;
            var task = new EnterpriseWorkTask(
                Id: Guid.NewGuid().ToString("N"),
                TemplateId: EnterpriseTemplates.MeetingMinutesId,
                Title: string.IsNullOrWhiteSpace(title) ? "未命名会议纪要" : title.Trim(),
                SourceText: sourceText.Trim(),
                Status: EnterpriseTaskStatus.Draft,
                CreatedBy: string.IsNullOrWhiteSpace(createdBy) ? "本机管理员" : createdBy.Trim(),
                CreatedAtUtc: now,
                UpdatedAtUtc: now,
                Reviewer: null,
                ReviewNote: null);

            snapshot.Tasks.Insert(0, task);
            AddAuditUnsafe(snapshot, task.Id, "task.created", task.CreatedBy, now);
            WriteUnsafe(snapshot);
            return task;
        }
    }

    public EnterpriseWorkTask MarkProcessing(string taskId, string actor) =>
        Transition(taskId, EnterpriseTaskStatus.InProgress, actor, null);

    public EnterpriseWorkTask SubmitForApproval(string taskId, string actor) =>
        Transition(taskId, EnterpriseTaskStatus.AwaitingApproval, actor, null);

    public EnterpriseWorkTask ApproveDelivery(string taskId, string actor, EnterpriseRole role) =>
        Transition(taskId, EnterpriseTaskStatus.Delivered, actor, null, role);

    public EnterpriseWorkTask RequestChanges(string taskId, string actor, EnterpriseRole role, string? note) =>
        Transition(taskId, EnterpriseTaskStatus.NeedsChanges, actor, note, role);

    private EnterpriseWorkTask Transition(
        string taskId,
        EnterpriseTaskStatus destination,
        string actor,
        string? reviewNote,
        EnterpriseRole? role = null)
    {
        if (string.IsNullOrWhiteSpace(taskId))
            throw new ArgumentException("A task id is required.", nameof(taskId));

        lock (_sync)
        {
            var snapshot = File.Exists(_filePath)
                ? ReadUnsafe()
                : throw new InvalidOperationException("The enterprise workspace has not been initialized.");
            var index = snapshot.Tasks.FindIndex(task => string.Equals(task.Id, taskId, StringComparison.Ordinal));
            if (index < 0)
                throw new InvalidOperationException("The requested task no longer exists.");

            var current = snapshot.Tasks[index];
            if (!EnterpriseTaskPolicy.CanTransition(current.Status, destination, role))
                throw new InvalidOperationException($"Cannot move a {current.Status} task to {destination}.");

            var now = DateTimeOffset.UtcNow;
            var updated = current with
            {
                Status = destination,
                UpdatedAtUtc = now,
                Reviewer = destination is EnterpriseTaskStatus.Delivered or EnterpriseTaskStatus.NeedsChanges
                    ? NormalizeActor(actor)
                    : current.Reviewer,
                ReviewNote = destination == EnterpriseTaskStatus.NeedsChanges
                    ? NormalizeOptionalText(reviewNote)
                    : current.ReviewNote
            };
            snapshot.Tasks[index] = updated;
            AddAuditUnsafe(snapshot, current.Id, $"task.{destination.ToString().ToLowerInvariant()}", actor, now);
            WriteUnsafe(snapshot);
            return updated;
        }
    }

    private EnterpriseWorkspaceSnapshot ReadUnsafe()
    {
        try
        {
            var json = File.ReadAllText(_filePath);
            var snapshot = JsonSerializer.Deserialize<EnterpriseWorkspaceSnapshot>(json, JsonOptions);
            if (snapshot is null || snapshot.SchemaVersion != CurrentSchemaVersion)
                throw new InvalidDataException("The enterprise workspace data is not supported by this version.");

            snapshot.Templates ??= new();
            snapshot.Tasks ??= new();
            snapshot.AuditEvents ??= new();
            if (snapshot.Templates.Count == 0)
                snapshot.Templates = EnterpriseTemplates.CreateDefault();
            return snapshot;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The enterprise workspace data could not be read.", ex);
        }
    }

    private void WriteUnsafe(EnterpriseWorkspaceSnapshot snapshot)
    {
        var directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(_filePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(snapshot, JsonOptions));
            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static void AddAuditUnsafe(
        EnterpriseWorkspaceSnapshot snapshot,
        string taskId,
        string action,
        string actor,
        DateTimeOffset occurredAtUtc)
    {
        snapshot.AuditEvents.Insert(0, new EnterpriseAuditEvent(
            Id: Guid.NewGuid().ToString("N"),
            TaskId: taskId,
            Action: action,
            Actor: NormalizeActor(actor),
            OccurredAtUtc: occurredAtUtc));
    }

    private static string NormalizeActor(string actor) =>
        string.IsNullOrWhiteSpace(actor) ? "本机管理员" : actor.Trim();

    private static string? NormalizeOptionalText(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}

public enum EnterpriseRole
{
    Owner,
    Administrator,
    Member,
    ReadOnly,
}

public enum EnterpriseTaskStatus
{
    Draft,
    InProgress,
    AwaitingApproval,
    NeedsChanges,
    Delivered,
}

public static class EnterpriseTaskPolicy
{
    public static bool CanTransition(
        EnterpriseTaskStatus source,
        EnterpriseTaskStatus destination,
        EnterpriseRole? actorRole = null) =>
        (source, destination) switch
        {
            (EnterpriseTaskStatus.Draft, EnterpriseTaskStatus.InProgress) => true,
            (EnterpriseTaskStatus.NeedsChanges, EnterpriseTaskStatus.InProgress) => true,
            (EnterpriseTaskStatus.InProgress, EnterpriseTaskStatus.AwaitingApproval) => true,
            (EnterpriseTaskStatus.AwaitingApproval, EnterpriseTaskStatus.Delivered) => CanReview(actorRole),
            (EnterpriseTaskStatus.AwaitingApproval, EnterpriseTaskStatus.NeedsChanges) => CanReview(actorRole),
            _ => false
        };

    public static bool CanReview(EnterpriseRole? role) =>
        role is EnterpriseRole.Owner or EnterpriseRole.Administrator;
}

public sealed class EnterpriseWorkspaceSnapshot
{
    public int SchemaVersion { get; init; }
    public string OrganizationName { get; init; } = "我的团队";
    public string CurrentMemberName { get; init; } = "本机管理员";
    public EnterpriseRole CurrentMemberRole { get; init; } = EnterpriseRole.Administrator;
    public List<EnterpriseWorkTemplate> Templates { get; set; } = new();
    public List<EnterpriseWorkTask> Tasks { get; set; } = new();
    public List<EnterpriseAuditEvent> AuditEvents { get; set; } = new();

    public static EnterpriseWorkspaceSnapshot CreateDefault(int schemaVersion) => new()
    {
        SchemaVersion = schemaVersion,
        Templates = EnterpriseTemplates.CreateDefault(),
    };
}

public sealed record EnterpriseWorkTemplate(
    string Id,
    string Name,
    string Description,
    bool IsAvailable,
    bool RequiresApproval);

public sealed record EnterpriseWorkTask(
    string Id,
    string TemplateId,
    string Title,
    string SourceText,
    EnterpriseTaskStatus Status,
    string CreatedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? Reviewer,
    string? ReviewNote);

public sealed record EnterpriseAuditEvent(
    string Id,
    string TaskId,
    string Action,
    string Actor,
    DateTimeOffset OccurredAtUtc);

public static class EnterpriseTemplates
{
    public const string MeetingMinutesId = "meeting-minutes";

    public static List<EnterpriseWorkTemplate> CreateDefault() =>
    [
        new(MeetingMinutesId, "会议纪要", "将会议记录整理为结论、待办和负责人。", true, true),
        new("weekly-report", "日报周报", "按部门格式汇总工作进展和风险。", false, true),
        new("customer-follow-up", "客户跟进", "把沟通记录整理为下一步跟进事项。", false, true),
        new("policy-qa", "制度问答", "基于已授权的企业资料回答制度问题。", false, false),
        new("spreadsheet-cleanup", "表格整理", "识别表格中的字段并输出清理建议。", false, true),
    ];

    public static string BuildMeetingMinutesPrompt(EnterpriseWorkTask task) =>
        "你正在处理企业会议纪要。请仅根据以下会议记录，输出：1. 会议结论。2. 待办事项。3. 每项负责人和截止时间。4. 待确认的问题。不要编造缺失信息。\n\n" + task.SourceText;
}
