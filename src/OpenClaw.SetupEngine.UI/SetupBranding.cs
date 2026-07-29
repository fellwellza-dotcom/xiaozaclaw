namespace OpenClaw.SetupEngine.UI;

/// <summary>
/// Product-specific text projected into the shared setup engine.
/// The upstream OpenClaw build keeps its existing English copy, while
/// independently branded builds can provide their own first-run experience.
/// </summary>
public sealed class SetupBranding
{
    public static SetupBranding OpenClaw { get; } = new("OpenClaw", useSimplifiedChinese: false);
    public static SetupBranding XiaozaclawSimplifiedChinese { get; } = new("xiaozaclaw", useSimplifiedChinese: true);

    public SetupBranding(string productName, bool useSimplifiedChinese)
    {
        ProductName = string.IsNullOrWhiteSpace(productName) ? "OpenClaw" : productName.Trim();
        UseSimplifiedChinese = useSimplifiedChinese;
    }

    public string ProductName { get; }
    public bool UseSimplifiedChinese { get; }

    public string SetupTitle => UseSimplifiedChinese ? $"{ProductName} 安装向导" : $"{ProductName} Setup";
    public string SecurityTitle => UseSimplifiedChinese ? $"欢迎使用 {ProductName}" : $"Welcome to {ProductName}";
    public string SecurityDescription => UseSimplifiedChinese
        ? $"{ProductName} 通过这台电脑上的私有网关运行你的个人 AI 助手，也可以连接 WhatsApp、Telegram 等聊天应用。"
        : $"{ProductName} runs your personal AI agent through a private gateway on this PC: reachable from chat apps like WhatsApp or Telegram.";
    public string SecurityNoticeTitle => UseSimplifiedChinese ? "安全提示" : "Security notice";
    public string SecurityNoticeMessage => UseSimplifiedChinese
        ? "你连接的 AI 助手可以在这台电脑上执行重要操作，包括运行命令、读取和写入文件、捕获屏幕。实际可执行的操作取决于你授予的权限。\n\n请只在你信任的电脑上进行设置，并确认你理解相关风险、信任所使用的提示词和集成。后续步骤中可以精确选择允许的能力。"
        : "The AI agent you connect (for example, Claude) can take powerful actions on this PC, including running commands, reading and writing files, and capturing your screen. Those actions depend on what you allow.\n\nOnly set this up on a computer you trust, and only continue if you understand the risks and trust the prompts and integrations you use. You choose exactly what to allow in the next steps.";
    public string Continue => UseSimplifiedChinese ? "继续" : "Continue";

    public string WelcomeTitle => UseSimplifiedChinese ? $"设置 {ProductName}" : $"Set up {ProductName}";
    public string WelcomeDescription => UseSimplifiedChinese
        ? $"{ProductName} 通过网关运行你的 AI 助手。请选择这台电脑的设置方式，之后仍可修改。"
        : $"{ProductName} runs your agent through a gateway. Choose how to set it up on this PC. You can change this later.";
    public string InstallLocalGateway => UseSimplifiedChinese ? "安装本地网关 (WSL)" : "Install a local gateway (WSL)";
    public string CheckingExistingSetup => UseSimplifiedChinese ? "正在检查现有设置..." : "Checking existing setup...";
    public string Recommended => UseSimplifiedChinese ? "推荐" : "Recommended";
    public string InstallLocalGatewayDescription => UseSimplifiedChinese
        ? $"在独立的 Ubuntu WSL 环境中运行私有、自托管的 {ProductName} 网关。执行前会明确显示安装内容。"
        : $"A private, self-hosted {ProductName} gateway in an isolated Ubuntu WSL instance. We'll show exactly what gets installed before anything runs.";
    public string ConnectExistingGateway => UseSimplifiedChinese ? "连接已有网关" : "Connect to an existing gateway";
    public string ConnectExistingGatewayDescription => UseSimplifiedChinese
        ? "如果已经在本机、其他电脑或远程服务器运行兼容网关，可以直接配对，无需安装。"
        : $"Already run a {ProductName} gateway: here, on another machine, or remote? Pair without installing anything.";
    public string Back => UseSimplifiedChinese ? "返回" : "Back";
    public string Next => UseSimplifiedChinese ? "下一步" : "Next";
    public string ReplaceGatewayTitle => UseSimplifiedChinese ? "替换现有 WSL 网关？" : "Replace existing WSL gateway?";
    public string InstallGatewayTitle => UseSimplifiedChinese ? "安装新的 WSL 网关？" : "Install a new WSL gateway?";
    public string Cancel => UseSimplifiedChinese ? "取消" : "Cancel";

    public string BuildReplacementSummary(
        bool hasLocalGateway,
        bool hasDistro,
        string? distroName,
        bool hasIdentityFiles,
        IReadOnlyList<string> preservedGatewayNames,
        string englishFallback)
    {
        if (!UseSimplifiedChinese)
            return englishFallback;

        if (!hasLocalGateway && !hasDistro)
            return "将创建新的本地 WSL 网关。现有配置不会受到影响。";

        var lines = new List<string>();
        if (hasDistro)
            lines.Add($"• WSL 发行版“{distroName}”将被删除并重新创建");
        if (hasLocalGateway)
            lines.Add("• 本地网关记录将被替换");
        if (hasIdentityFiles)
            lines.Add("• 本地网关的设备身份文件将重新生成");

        if (preservedGatewayNames.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("以下网关不会受到影响：");
            foreach (var name in preservedGatewayNames)
                lines.Add($"  • {name}");
        }

        return string.Join("\n", lines);
    }
}
