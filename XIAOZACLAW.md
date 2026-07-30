# xiaozaclaw 企业 AI 工作台

xiaozaclaw 是面向中小企业的 Windows 桌面 AI 工作台，基于 [new-api](https://github.com/QuantumNous/new-api) 提供统一模型接入、员工账号、权限、额度、成本和审计管理。

## 给普通用户的产品形态

普通用户只需要下载并安装 `xiaozaclaw-new-api-<版本>-setup.exe`。安装包内置桌面界面、Go 服务、网页资源和 SQLite 数据库。安装后直接打开应用，不要求安装 Docker、WSL、Go、Bun、PostgreSQL 或 Redis，也不需要在命令行配置环境。

首次启动时，用户在界面中完成三件事：创建管理员账号、添加已购买或已授权的模型服务密钥、按需要邀请员工和分配权限。模型服务密钥无法也不应该随安装包预置。

数据仅保存在当前 Windows 用户的应用数据目录。桌面版本地服务使用随机端口并仅监听 `127.0.0.1`，不会因为安装而向局域网暴露管理后台。

## 构建与交付

维护者在 GitHub 创建 `xiaozaclaw-v1.0.0` 这类标签，或手动运行 `Build xiaozaclaw Windows installer` 工作流。云端工作流会构建并上传安装程序和 SHA-256 校验文件，构建机而不是最终用户电脑安装 Go、Bun 和 Node。

如需避免 Windows SmartScreen 对未签名程序的提示，请在 GitHub 仓库中配置 `WINDOWS_CERTIFICATE` 和 `WINDOWS_CERTIFICATE_PASSWORD` secrets。它们只用于云端签名，不能提交到仓库。

## 服务器版，仅限集中部署

`xiaozaclaw/setup.ps1` 和 `xiaozaclaw/compose.yaml` 是给有运维人员的集中式服务器部署准备的。它需要 Docker Desktop 或服务器 Docker，不是普通员工电脑的安装方式。

## 许可与署名

本部署套件基于 new-api。new-api、QuantumNous 的署名、[上游链接](https://github.com/QuantumNous/new-api)、`LICENSE`、`NOTICE` 和 `THIRD-PARTY-LICENSES.md` 均保留在本项目中。

new-api 采用 AGPLv3 及其附加条款。对外提供网络服务或分发修改版本前，请审阅并履行相应的源码提供、署名和链接义务。
