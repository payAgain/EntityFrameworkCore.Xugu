# XuguDB EF Core Provider

**Xugu 原生方言**的 Entity Framework Core Provider for [XuguDB](https://www.xugudb.com/)（虚谷数据库）。

| | |
|---|---|
| **NuGet** | `Microsoft.EntityFrameworkCore.Xugu` |
| **Version** | **3.0.0** GA |
| **EF Core** | 9.0.x |
| **.NET** | 9.0 (`net9.0`) |
| **Driver** | [Xuguclient](https://www.nuget.org/packages/Xuguclient) (ADO.NET) |

> 本 Provider 以 **XuguDB 官方文档** 为 SQL 方言唯一权威。架构模式参考 [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) 9.0.0（**仅 C# 模块划分**，非 MySQL 迁移目标）。

---

## Install

```powershell
dotnet add package Microsoft.EntityFrameworkCore.Xugu --version 3.0.0
dotnet add package Xuguclient
```

## Quick start

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseXugu("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=***; PORT=5138; CHARSET=UTF8;"));
```

**5 分钟上手** → [docs/GETTING-STARTED.md](docs/GETTING-STARTED.md)  
**完整用户指南** → [docs/USER-GUIDE.md](docs/USER-GUIDE.md)（连接、模型、LINQ、迁移、FAQ）

---

## Build from source

需要 [.NET SDK 9.0+](https://dotnet.microsoft.com/download)（见 `global.json`）。

```powershell
dotnet restore Xugu.EFCore.Xugu.sln
dotnet build Xugu.EFCore.Xugu.sln -c Release
dotnet test Xugu.EFCore.Xugu.sln -c Release
```

发布包构建（依赖 NuGet 驱动，非本地子模块）：

```powershell
dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -o artifacts/ -p:UseLocalXuguDriver=false
```

---

## Repository layout

```
├── src/EFCore.Xugu/          # Provider 实现
├── test/EFCore.Xugu.Tests/   # 功能与集成测试
├── samples/EfDesignSample/   # 设计时工具示例
├── docs/                     # 用户文档
├── RELEASE.md                # 发布操作说明
└── Xugu.EFCore.Xugu.sln
```

---

## Documentation

| 文档 | 说明 |
|------|------|
| [GETTING-STARTED.md](docs/GETTING-STARTED.md) | 快速开始：安装、连接串、`UseXugu`、迁移 |
| [USER-GUIDE.md](docs/USER-GUIDE.md) | **用户指南**：模型约定、LINQ/JSON、批量 DML、重试、FAQ |
| [LIMITATIONS.md](docs/LIMITATIONS.md) | 已知限制与平台范围 |
| [XUGU-VS-MYSQL.md](docs/XUGU-VS-MYSQL.md) | 与 Pomelo/MySQL 对照参考（非迁移指南） |
| [CHANGELOG.md](docs/CHANGELOG.md) | 版本变更 |
| [RELEASE-SCOPE.md](docs/RELEASE-SCOPE.md) | 产品范围声明 |
| [xuguclient-dependency-strategy.md](docs/xuguclient-dependency-strategy.md) | 驱动依赖策略 |

---

## Platform support (3.0.0 GA)

- **Windows** — 生产 GA 平台
- **Linux x64** — RID / `libxugusql.so` 已登记 vendor ticket，signed-off blocked（见 LIMITATIONS）

---

## License

MIT — see [LICENSE](LICENSE).

## Contributing

欢迎通过 GitHub Issue / Pull Request 参与。开发环境需 .NET 9 SDK、Windows x64 与可访问的 XuguDB 实例。
