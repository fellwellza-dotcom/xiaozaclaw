# xiaozaclaw 企业 AI 平台

xiaozaclaw 是面向中小企业的部署套件，基于 [new-api](https://github.com/QuantumNous/new-api) 提供统一模型接入、员工账号、权限、额度、成本和审计管理。

它不再要求每一位员工安装 WSL、配置 Docker 或分别保存模型密钥。管理员部署一个平台实例，员工通过浏览器登录使用。

## 一次部署

1. 安装并启动 Docker Desktop。
2. 在项目根目录运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\xiaozaclaw\setup.ps1
```

3. 打开 `http://localhost:3000`，按初始化页面创建管理员。
4. 在管理后台依次配置模型渠道、员工账号、Token 分组、模型权限和额度策略。

脚本第一次运行时自动生成 PostgreSQL、Redis 和会话密钥，写入未被 Git 跟踪的 `xiaozaclaw/.env`。不要把该文件发送给其他人或提交到版本库。

## 生产部署

生产环境请通过 HTTPS 反向代理暴露服务，并将 `SESSION_COOKIE_SECURE=true`，同时设置 `SESSION_COOKIE_TRUSTED_URL=https://你的域名`。数据库、Redis 和容器端口默认不暴露到局域网。

停止本机实例：

```powershell
powershell -ExecutionPolicy Bypass -File .\xiaozaclaw\setup.ps1 -Stop
```

## 许可与署名

本部署套件基于 new-api。new-api、QuantumNous 的署名、[上游链接](https://github.com/QuantumNous/new-api)、`LICENSE`、`NOTICE` 和 `THIRD-PARTY-LICENSES.md` 均保留在本项目中。

new-api 采用 AGPLv3 及其附加条款。对外提供网络服务或分发修改版本前，请审阅并履行相应的源码提供、署名和链接义务。
