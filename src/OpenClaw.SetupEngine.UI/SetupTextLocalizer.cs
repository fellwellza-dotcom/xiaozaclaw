using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace OpenClaw.SetupEngine.UI;

/// <summary>
/// Applies the independently branded setup copy without changing the upstream
/// setup engine's English defaults.
/// </summary>
internal static class SetupTextLocalizer
{
    private static readonly IReadOnlyDictionary<string, string> Chinese =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Connect to an existing gateway"] = "连接已有网关",
            ["We'll open Companion Settings next. Nothing gets installed. Here's what you'll do there:"] = "下一步将打开 xiaozaclaw 设置，不会安装任何内容。请按以下步骤操作：",
            ["Add a gateway"] = "添加网关",
            ["In Connection, choose Add a gateway, then pick how to connect: Setup code or Direct (URL + token)."] = "在“连接”中选择“添加网关”，再选择连接方式：设置码或直接连接（网址和令牌）。",
            ["Point it at your gateway"] = "指定网关地址",
            ["Connect to a gateway running here, on another PC, or remote. For example, use ws://host.local:18790."] = "连接本机、其他电脑或远程服务器上的网关，例如 ws://host.local:18790。",
            ["Pair this device"] = "配对此设备",
            ["Paste a setup code from the gateway host, or your shared token, so the gateway trusts this PC."] = "粘贴网关主机生成的设置码或共享令牌，让网关信任这台电脑。",
            ["Optional: SSH tunnel"] = "可选：SSH 隧道",
            ["If your gateway isn't directly reachable, any method can tunnel over SSH."] = "如果无法直接访问网关，任意连接方式都可以通过 SSH 建立隧道。",
            ["Open Companion Settings"] = "打开 xiaozaclaw 设置",
            ["Set up the WSL gateway"] = "设置 WSL 网关",
            ["OpenClaw runs your agent in a private, self-hosted gateway on this PC. Pick what it's allowed to do. You can change this later in Companion Settings."] = "xiaozaclaw 在这台电脑的私有自托管网关中运行 AI 助手。请选择允许的能力，之后可在设置中修改。",
            ["Read-only"] = "只读",
            ["See your screen, render on canvas, read device info. No commands."] = "可查看屏幕、在画布中呈现内容并读取设备信息，但不能运行命令。",
            ["Standard"] = "标准",
            ["Recommended"] = "推荐",
            ["Read-only plus run commands, edit files, and use text-to-speech and speech-to-text."] = "包含只读能力，并可运行命令、编辑文件、使用语音合成和语音识别。",
            ["Full access"] = "完整访问",
            ["Everything, plus camera, location, and browser control."] = "允许全部能力，包括摄像头、位置和浏览器控制。",
            ["Fine-tune individual capabilities (optional)"] = "精细调整各项能力（可选）",
            ["Basic device info and status stay available while Node Mode is on."] = "启用节点模式时，基本设备信息和状态会保持可用。",
            ["Grant the Windows permissions your selected capabilities need: now, or later from Settings. The agent only uses what you allow."] = "授予所选能力需要的 Windows 权限。可以现在授予，也可以稍后在设置中处理。AI 助手只能使用你允许的权限。",
            ["Install an isolated Ubuntu 24.04 instance"] = "安装独立的 Ubuntu 24.04 实例",
            ["WSL distro \"OpenClawGateway\" in your AppData. Separate from any Ubuntu you already have."] = "在 AppData 中创建 WSL 发行版“xiaozaclawGateway”，与现有 Ubuntu 相互独立。",
            ["Tailnet access with Tailscale Serve"] = "通过 Tailscale Serve 访问 Tailnet",
            ["Publish this private gateway as HTTPS/WSS to your Tailscale tailnet. Windows Tailscale must already be signed in; setup root owns Serve."] = "将私有网关以 HTTPS/WSS 方式发布到 Tailscale 网络。Windows 版 Tailscale 必须已经登录，Serve 由安装环境管理。",
            ["Check Windows Tailscale before continuing."] = "继续前请检查 Windows 版 Tailscale。",
            ["Checking Windows Tailscale…"] = "正在检查 Windows 版 Tailscale…",
            ["Windows Tailscale must be installed and signed in before setup can continue."] = "必须先安装并登录 Windows 版 Tailscale，才能继续设置。",
            ["Tailscale auth key (not saved)"] = "Tailscale 授权密钥（不会保存）",
            ["Trust Tailscale identities for gateway authentication"] = "信任 Tailscale 身份并用于网关认证",
            ["Enabled"] = "已启用",
            ["Disabled"] = "已禁用",
            ["Off by default. Enable only when your Tailscale ACLs should be part of the gateway access-control boundary; OpenClaw receives only a restricted identity lookup, not Tailscale operator control."] = "默认关闭。仅当 Tailscale 访问控制规则应作为网关权限边界时启用。xiaozaclaw 只能查询受限的身份信息，不能控制 Tailscale。",
            ["Download & install the OpenClaw CLI"] = "下载并安装 xiaozaclaw CLI",
            ["Fetched over HTTPS; runs as a non-root openclaw user inside the instance."] = "通过 HTTPS 获取，并以实例中的非 root 用户 openclaw 运行。",
            ["Run a local gateway service"] = "运行本地网关服务",
            ["Loopback only. It is not reachable from your network or the internet."] = "仅监听本机回环地址，局域网和互联网无法直接访问。",
            ["May ask for admin once"] = "可能需要一次管理员权限",
            ["Only if the Windows WSL platform isn't installed yet. One-time per machine."] = "仅在尚未安装 Windows WSL 平台时需要，每台电脑只执行一次。",
            ["Show the exact commands & files"] = "显示将执行的命令和写入的文件",
            ["Install & set up"] = "安装并设置",
            ["All set!"] = "设置完成！",
            ["OpenClaw is ready to go"] = "xiaozaclaw 已可使用",
            ["Setup failed"] = "设置失败",
            ["Local gateway running"] = "本地网关正在运行",
            ["Device paired"] = "设备已配对",
            ["This PC is connected and ready to use"] = "这台电脑已连接并可使用",
            ["Capability profile applied"] = "能力配置已应用",
            ["Editable any time in Companion Settings"] = "随时可以在 xiaozaclaw 设置中修改",
            ["View full log →"] = "查看完整日志 →",
            ["Node mode enabled"] = "节点模式已启用",
            ["Launch OpenClaw at startup"] = "开机时启动 xiaozaclaw",
            ["On"] = "开",
            ["Off"] = "关",
            ["Finish"] = "完成",
            ["Close"] = "关闭",
            ["Setting up WSL gateway"] = "正在设置 WSL 网关",
            ["Authorize Tailscale"] = "授权 Tailscale",
            ["Open Tailscale authorization"] = "打开 Tailscale 授权页面",
            ["Live activity"] = "实时进度",
            ["Open log file"] = "打开日志文件",
            ["Gateway installed"] = "网关已安装",
            ["Your private OpenClaw gateway is up and running on this PC."] = "你的 xiaozaclaw 私有网关已在这台电脑上运行。",
            ["Up next: OpenClaw onboard"] = "下一步：配置 xiaozaclaw",
            ["A few quick questions. Choose your AI provider, model, and key to connect your agent."] = "回答几个简单问题，选择 AI 服务商、模型和密钥以连接助手。",
            ["Start OpenClaw onboard"] = "开始配置 xiaozaclaw",
            ["OpenClaw onboard"] = "xiaozaclaw 配置",
            ["Connecting to gateway..."] = "正在连接网关...",
            ["Starting wizard..."] = "正在启动配置向导...",
            ["Live gateway output"] = "网关实时输出",
            ["Enter value"] = "请输入内容",
            ["Open terminal"] = "打开终端",
            ["Restart gateway"] = "重启网关",
            ["Back"] = "返回",
            ["Next"] = "下一步",
            ["More options"] = "更多选项",
            ["Restart onboard"] = "重新开始配置",
            ["Skip & exit"] = "跳过并退出",
            ["Skip"] = "跳过",
            ["Continue"] = "继续",
            ["Yes"] = "是",
            ["No"] = "否",
            ["Working…"] = "处理中…",
            ["Setting things up…"] = "正在进行设置…",
            ["More ▾"] = "更多 ▾",
            ["Loading..."] = "正在加载...",
            ["Starting over..."] = "正在重新开始...",
            ["Skipping..."] = "正在跳过...",
            ["Submitting..."] = "正在提交...",
            ["Copy"] = "复制",
            ["Wizard needs attention"] = "配置向导需要处理",
            ["Start wizard again"] = "重新启动配置向导",
            ["Windows integration needs attention"] = "Windows 集成需要处理",
            ["Retry Windows integration"] = "重试 Windows 集成",
            ["Skipping wizard..."] = "正在跳过配置向导...",
            ["Finishing Windows integration..."] = "正在完成 Windows 集成...",
            ["A few quick questions to connect your agent"] = "回答几个简单问题以连接 AI 助手",
            ["Default AI model"] = "默认 AI 模型",
            ["Choose at least one valid option."] = "请至少选择一个有效选项。",
            ["Enter a value to continue."] = "请输入内容后继续。",
            ["Choose a valid option."] = "请选择有效选项。",
            ["Confirm"] = "确认",
            ["Choose an option"] = "选择一个选项",
            ["Choose options"] = "选择多个选项",
            ["Setup"] = "设置",
            ["Couldn't read Windows permission status"] = "无法读取 Windows 权限状态",
            ["Another setup task is still active. Wait for it to finish, then start OpenClaw onboard."] = "另一个设置任务仍在运行。请等待其完成，然后开始配置 xiaozaclaw。",
            ["The gateway restarted before the current wizard step finished. Your setup is still installed; choose Start wizard again, or use More options to restart onboard or skip and exit."] = "当前步骤完成前网关已重启。已安装的内容仍然保留，请选择“重新启动配置向导”，或从“更多选项”中重新开始或跳过并退出。",
            ["Gateway connection was lost while the wizard was running."] = "配置向导运行时网关连接已断开。",
            ["Gateway wizard returned an invalid response."] = "网关配置向导返回了无效响应。",
            ["Gateway wizard step is missing an id."] = "网关配置步骤缺少标识。",
            ["Gateway wizard returned a choice step without any selectable options."] = "网关配置步骤没有可选择的选项。",
        };

    public static void Attach(FrameworkElement root, SetupBranding branding, SetupConfig? config)
    {
        if (!branding.UseSimplifiedChinese)
            return;

        var applied = false;
        RoutedEventHandler? loaded = null;
        void ApplyOnce()
        {
            if (applied)
                return;
            applied = true;
            try
            {
                ApplyToTree(root, branding.ProductName, config);
            }
            finally
            {
                if (loaded is not null)
                    root.Loaded -= loaded;
            }
        }

        // Changing text from LayoutUpdated invalidates layout again and can
        // create a WinUI LayoutCycleException on first-run setup. Apply only
        // after the page has materialized, once per navigation.
        loaded = (_, _) => ApplyOnce();
        if (root.IsLoaded)
            ApplyOnce();
        else
            root.Loaded += loaded;
        root.Unloaded += (_, _) =>
        {
            if (loaded is not null)
                root.Loaded -= loaded;
        };
    }

    internal static string Translate(string source, string productName, SetupConfig? config)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        var translated = Chinese.TryGetValue(source, out var exact) ? exact : source;
        if (translated.StartsWith("Creating OpenClawGateway WSL instance:", StringComparison.Ordinal))
            translated = translated.Replace("Creating OpenClawGateway WSL instance:", "正在创建 xiaozaclawGateway WSL 实例：", StringComparison.Ordinal);
        else if (translated.StartsWith("Restarting ", StringComparison.Ordinal))
            translated = "正在重启 " + translated["Restarting ".Length..];
        else if (translated.StartsWith("Opened a terminal in ", StringComparison.Ordinal))
            translated = translated.Replace("Opened a terminal in ", "已在 ", StringComparison.Ordinal)
                .Replace(". Install the tool, then choose Restart gateway.", " 中打开终端。安装工具后请选择“重启网关”。", StringComparison.Ordinal);
        else if (translated.StartsWith("Couldn't open a terminal:", StringComparison.Ordinal))
            translated = translated.Replace("Couldn't open a terminal:", "无法打开终端：", StringComparison.Ordinal);
        else if (translated.StartsWith("Gateway wizard failed:", StringComparison.Ordinal))
            translated = translated.Replace("Gateway wizard failed:", "网关配置向导失败：", StringComparison.Ordinal);
        else if (translated.StartsWith("Restarting the gateway failed:", StringComparison.Ordinal))
            translated = translated.Replace("Restarting the gateway failed:", "重启网关失败：", StringComparison.Ordinal);
        else if (translated.StartsWith("Windows could not complete the WSL platform installation", StringComparison.Ordinal))
            translated = translated
                .Replace("Windows could not complete the WSL platform installation", "Windows 无法完成 WSL 平台安装", StringComparison.Ordinal)
                .Replace("No xiaozaclaw WSL distribution was created, and existing WSL distributions were not removed.", "未创建 xiaozaclaw 的 WSL 发行版，也未删除现有 WSL 发行版。", StringComparison.Ordinal)
                .Replace("Open Windows PowerShell as Administrator and run:", "请以管理员身份打开 Windows PowerShell，并依次运行：", StringComparison.Ordinal)
                .Replace("Restart Windows, then run setup again.", "然后重启 Windows，再重新运行安装向导。", StringComparison.Ordinal);

        var distro = string.IsNullOrWhiteSpace(config?.DistroName) ? "xiaozaclawGateway" : config.DistroName;
        var gatewayPort = config?.GatewayPort is > 0 ? config.GatewayPort : 18889;
        return translated
            .Replace("OpenClawGateway", distro, StringComparison.Ordinal)
            .Replace("OpenClaw", productName, StringComparison.Ordinal)
            .Replace("18789", gatewayPort.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    private static void ApplyToTree(DependencyObject root, string productName, SetupConfig? config)
    {
        if (root is TextBlock textBlock)
            textBlock.Text = Translate(textBlock.Text, productName, config);

        if (root is ContentControl contentControl && contentControl.Content is string content)
            contentControl.Content = Translate(content, productName, config);

        if (root is TextBox textBox)
            textBox.PlaceholderText = Translate(textBox.PlaceholderText, productName, config);
        if (root is PasswordBox passwordBox)
            passwordBox.PlaceholderText = Translate(passwordBox.PlaceholderText, productName, config);

        if (root is ToggleSwitch toggle)
        {
            if (toggle.Header is string header)
                toggle.Header = Translate(header, productName, config);
            if (toggle.OnContent is string onContent)
                toggle.OnContent = Translate(onContent, productName, config);
            if (toggle.OffContent is string offContent)
                toggle.OffContent = Translate(offContent, productName, config);
        }

        if (root is Expander expander && expander.Header is string expanderHeader)
            expander.Header = Translate(expanderHeader, productName, config);

        if (root is InfoBar infoBar)
        {
            infoBar.Title = Translate(infoBar.Title, productName, config);
            infoBar.Message = Translate(infoBar.Message, productName, config);
        }

        if (root is DropDownButton { Flyout: MenuFlyout menu })
        {
            foreach (var item in menu.Items.OfType<MenuFlyoutItem>())
                item.Text = Translate(item.Text, productName, config);
        }

        if (root is UIElement element)
        {
            var automationName = AutomationProperties.GetName(element);
            if (!string.IsNullOrWhiteSpace(automationName))
                AutomationProperties.SetName(element, TranslateStepName(automationName));
        }

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
            ApplyToTree(VisualTreeHelper.GetChild(root, index), productName, config);
    }

    private static string TranslateStepName(string source)
    {
        if (!source.StartsWith("Step ", StringComparison.Ordinal))
            return source;
        return "步骤 " + source["Step ".Length..].Replace(" of ", " / ", StringComparison.Ordinal);
    }
}
