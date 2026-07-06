# XuguDB EF Core Provider — 快速开始

> 当前版本：**`1.0.0`**  
> 包名：`Microsoft.EntityFrameworkCore.Xugu`  
> 目标框架：.NET 9.0

本文介绍如何在应用中使用 XuguDB EF Core Provider：连接串、`UseXugu`、迁移与常见排错。已知限制见 [LIMITATIONS.md](LIMITATIONS.md)；版本历史见 [CHANGELOG.md](CHANGELOG.md)。

---

## 前置条件

| 项 | 要求 |
|----|------|
| .NET SDK | 9.0+（见仓库根目录 `global.json`） |
| XuguDB 服务端 | 可网络访问的实例（默认端口 `5138`） |
| ADO.NET 驱动 | `Xuguclient`（NuGet）或开发时本地 `external/csharp-driver` |
| 原生库 | `xugusql.dll`（Windows x64；须与应用程序同目录或在 PATH 中） |

驱动与依赖策略详见 [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md)。

---

## 安装

```shell
dotnet add package Microsoft.EntityFrameworkCore.Xugu
dotnet tool install --global dotnet-ef
```

设计时迁移还需引用（通常仅开发机）：

```shell
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

## 连接串

XuguDB 使用 **键值对** 连接串（非 MySQL 标准格式）：

```
IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8
```

| 键 | 说明 |
|----|------|
| `IP` | 服务端地址 |
| `DB` | 数据库名 |
| `USER` / `PWD` | 认证 |
| `PORT` | 端口（默认 `5138`） |
| `AUTO_COMMIT` | 建议 `on` |
| `CHAR_SET` | 建议 `UTF8` |

可通过环境变量 `XUGU_CONNECTION_STRING` 覆盖（测试与 CI 常用）。

---

## 配置 DbContext

```csharp
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet<...>
}

// Program.cs 或 Startup
services.AddDbContext<AppDbContext>(options =>
    options.UseXugu(
        "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"));
```

### 兼容模式（MySQL 方言）

Provider 在连接打开后默认执行：

```sql
SET compatible_mode TO 'MYSQL'
```

以启用 MySQL 兼容 SQL 方言（自增、`LIMIT` 等）。若需关闭：

```csharp
options.UseXugu(connectionString, xugu => xugu.DisableCompatibleModeOnOpen());
```

依据：XuguDB 文档 `reference/system-configuration-parameter/session-parameter/compatible_mode.md`。

### 设计时工厂（`dotnet ef`）

```csharp
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("XUGU_CONNECTION")
            ?? "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXugu(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
```

端到端样本见 `samples/EfDesignSample/`。

---

## 迁移（Migrations）

```powershell
$env:XUGU_CONNECTION = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"

dotnet ef migrations add InitialCreate --project YourApp --startup-project YourApp
dotnet ef database update --project YourApp --startup-project YourApp
```

### 迁移注意事项

| 能力 | 状态 |
|------|------|
| `migrations add` / `database update` | ✅ 支持 |
| `dbcontext scaffold` | ✅ 基础支持（`DBA_TABLES` / `DBA_COLUMNS`） |
| `EnsureCreated()` / `EnsureDeleted()` | ❌ 不支持（`Database.Create`/`Delete` 抛 `NotSupportedException`） |
| 幂等迁移脚本 | ❌ 不支持 |
| 过滤索引（`HasFilter`） | ❌ 不支持 |

建库/删库请由 DBA 或运维在 XuguDB 侧手工完成，再执行 `database update`。

---

## 反向工程（Scaffolding）

```powershell
dotnet ef dbcontext scaffold $env:XUGU_CONNECTION Microsoft.EntityFrameworkCore.Xugu `
  --project YourApp --startup-project YourApp --context ScaffoldedDbContext --table YOUR_TABLE
```

生成的 `OnConfiguring` 会调用 `UseXugu(...)`。

---

## 常见错误

| 现象 | 可能原因 | 处理 |
|------|----------|------|
| `指定的连接无效或者尚未打开` `[E34301]` | 连接串错误或服务未启动 | 检查 `IP`/`PORT`/凭据；确认 XuguDB 进程 |
| `sqlexecute err:` `[E34501]` | SQL 方言或语法不兼容 | 确认 `compatible_mode` 已启用；对照 XuguDB 官方文档 |
| 找不到 `xugusql.dll` | 原生库未部署 | 将 `runtimes/win-x64/native/xugusql.dll` 复制到输出目录 |
| `DatabaseCreateNotSupported` | 调用了 `EnsureCreated()` | 手工建库后使用迁移 |
| `IdempotentMigrationScriptsNotSupported` | 使用了幂等脚本 API | 使用标准 `database update` |
| 中文乱码 | 字符集不一致 | 连接串使用 `CHAR_SET=UTF8` |

更多限制与 defer 项见 [LIMITATIONS.md](LIMITATIONS.md)。

---

## 相关文档

| 文档 | 说明 |
|------|------|
| [LIMITATIONS.md](LIMITATIONS.md) | 已知限制、skip/defer 清单 |
| [CHANGELOG.md](CHANGELOG.md) | 版本变更摘要 |
| [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md) | 驱动依赖、CI 与 NuGet 发布 |
| `harness/references/csharp-driver-analysis.md` | ADO.NET 驱动分析 |
| `E:\BaiduSyncdisk\docs\content\ecosystem\orm\dotnet\efcore.md` | XuguDB 官方 EF Core 生态文档 |
