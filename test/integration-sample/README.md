# Integration Sample — 真实场景冒烟（Phase 11）

> 最小 ASP.NET Core Web API，验证 **NuGet 消费方** 对 `Microsoft.EntityFrameworkCore.Xugu` 的典型用法。  
> **状态**：骨架（11.304）；实现随 Phase 11 推进。

## 前置

- .NET 9 SDK
- 本机或可连 XuguDB（默认 `127.0.0.1:5138`）
- 已构建 Provider 或已安装 NuGet 包

## 配置连接

```powershell
$env:XUGU_CONNECTION = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
```

或在 `MinimalApi/appsettings.Development.json` 中设置 `ConnectionStrings:Xugu`（勿提交生产密钥）。

## 开发态（ProjectReference）

```powershell
cd E:\Work\xuguefcore\test\integration-sample\MinimalApi
dotnet run
```

## 迁移

```powershell
cd E:\Work\xuguefcore\test\integration-sample\MinimalApi
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> 需已安装 `dotnet-ef`：`dotnet tool install --global dotnet-ef`

## 冒烟 API

```powershell
# 健康检查
curl http://localhost:5000/health

# CRUD
curl -X POST http://localhost:5000/api/items -H "Content-Type: application/json" -d "{\"name\":\"test\"}"
curl http://localhost:5000/api/items
```

## NuGet 消费态验证

发布前使用仓库脚本（不依赖 ProjectReference）：

```powershell
cd E:\Work\xuguefcore
harness/scripts/test-nuget-pack.ps1
harness/scripts/test-nuget-pack.ps1 -SmokeConnect  # 需 XUGU_CONNECTION
```

## 与 EfDesignSample 的区别

| 样本 | 焦点 |
|------|------|
| `samples/EfDesignSample` | `dotnet ef` CLI、Scaffolding |
| `test/integration-sample` | 运行时 Web API + HTTP CRUD + 发布门禁 |

## Phase 11 待办

- [ ] 11.109 后追加 JSON 实体与端点
- [x] `scripts/run-smoke.ps1` 自动化 curl 序列
- [x] `harness/scripts/run-integration-smoke.ps1` 端到端（build + run + CRUD）
- [ ] CI manual job 可选挂载
