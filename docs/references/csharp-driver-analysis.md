# XuguDB C# 驱动分析

> 驱动仓库：`external/csharp-driver/`（自 http://gitlab2.xugu.com/RDB/csharp-driver.git clone）  
> 分析日期：2026-07-06

## 结论

**是的，这就是我们 EF Core Provider 需要的 ADO.NET 驱动。**

EF Core Relational Provider 的底层依赖是 `System.Data.Common` 体系（`DbConnection` / `DbCommand` / `DbDataReader` / `DbTransaction` / `DbProviderFactory`），本驱动完整实现了这些抽象，且与现有 XuguDB EF Core 文档中的连接方式一致。

---

## 驱动基本信息

| 项 | 值 |
|----|-----|
| 程序集名 | `XuguClient.dll` |
| NuGet 包 ID | `Xuguclient` |
| 版本 | 3.3.5（csproj）/ VERSION 文件 3.3.5 |
| 目标框架 | `netstandard2.0` |
| 命名空间 | `XuguClient` |
| 原生依赖 | `xugusql.dll`（C++ 底层驱动） |
| 文档 | `doc/虚谷数据库C#标准接口开发手册.md` |

---

## ADO.NET 实现清单

| EF Core 需要 | 驱动实现 | 状态 |
|-------------|---------|------|
| `DbConnection` | `XGConnection` | ✅ |
| `DbCommand` | `XGCommand` | ✅ |
| `DbDataReader` | `XGDataReader` | ✅ |
| `DbTransaction` | `XGTransaction` | ✅ |
| `DbParameter` | `XGParameters` | ✅ |
| `DbProviderFactory` | `XGProviderFactory` | ✅ |
| `DbConnectionStringBuilder` | `XGConnectionStringBuilder` | ✅ |
| `DbCommandBuilder` | `XGCommandBuilder` | ✅ |
| `DbDataAdapter` | `XGDataAdapter` | ✅ |
| `GetSchema()` | `XGConnection.GetSchema()` | ✅（反向工程可用） |
| 连接池 | `XGConnectionPool` | ✅ |
| BLOB/CLOB | `XGBlob` / `XGClob` | ✅ |
| 空间类型 | `XGSpatialType` | ✅ |

---

## 连接串格式

与 XuguDB EF Core 文档一致：

```
IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8
```

键名（大小写不敏感，测试代码中使用）：`IP`, `DB`, `USER`, `PWD`, `PORT`, `AUTO_COMMIT`, `CHAR_SET`

Provider 中 `UseXugu(connectionString)` 应将此连接串传给 `XGConnection`。

---

## 架构（两层）

```
应用 / EF Core Provider
        ↓
XuguClient.dll（C# 托管层，DbConnection 等）
        ↓ P/Invoke
xugusql.dll（C++ 原生驱动）
        ↓
XuguDB 服务端
```

**部署注意**：运行时必须将 `xugusql.dll` 与应用程序同目录（或 PATH 中），与 Microsoft.Data.Sqlite 依赖 native sqlite3 类似。

---

## 与 EF Core Provider 的集成方式

参考 Pomelo + MySqlConnector 模式：

```csharp
// XuguRelationalConnection 继承 RelationalConnection
public class XuguRelationalConnection : RelationalConnection
{
    public XuguRelationalConnection(RelationalConnectionDependencies dependencies)
        : base(dependencies) { }

    protected override DbConnection CreateDbConnection()
        => new XGConnection();  // 或 XGProviderFactory.Instance.CreateConnection()
}
```

OptionsExtension 中的连接串直接传给 `XGConnection.ConnectionString`。

---

## 优势

1. **官方驱动** — 虚谷自研，与 V12 服务端匹配（changelog: feature-776）
2. **标准 ADO.NET** — 文档明确对标 SqlClient / OraClient
3. **netstandard2.0** — 可引用到 .NET 6/8/9 EF Core 项目
4. **已有 EF Core 生态** — `E:\BaiduSyncdisk\docs\content\ecosystem\orm\dotnet\efcore.md` 中的 `UseXugu()` 即基于此驱动
5. **GetSchema** — 支持 Scaffolding 读元数据
6. **测试齐全** — XGCSClientTests 覆盖 Connection/Command/Transaction/Reader 等

---

## 风险与注意事项

| 风险 | 说明 | 应对 |
|------|------|------|
| **原生 DLL 部署** | 必须分发 `xugusql.dll` | CI/打包脚本复制 native 文件；文档说明 |
| **编码问题** | README 称 .NET Core 单元测试有中文乱码；v3.2.2 曾修复 | 集成测试验证 UTF8；优先 `CHAR_SET=UTF8` |
| **netstandard2.0** | 非 net8.0 专用 | 一般可引用；关注 nullable/AOT 警告 |
| **参数占位符** | 需确认 EF 生成的 SQL 参数格式与驱动 BindParamByPos 兼容 | Phase 1 做参数化 CRUD 验证 |
| **非 MySQL 连接串** | 不能用 MySQL 标准连接串 | Provider 文档明确 Xugu 格式 |
| **兼容模式** | SQL 方言靠 `SET compatible_mode TO 'MYSQL'`，非连接串参数 | 在 Connection 打开后执行（Open 拦截或初始化） |

---

## 不需要的东西

- ❌ 不需要 MySqlConnector / Pomelo 的 MySQL 驱动
- ❌ 不需要另找 ADO.NET 驱动
- ⚠️ Pomelo 源码仅作 **EF Core Provider 架构参考**，不是数据库连接层

---

## Harness 更新建议

1. `contracts/sql-dialect.contract.md` — 连接串格式已对齐
2. `references/xugudb-docs-map.md` — 增加驱动文档路径
3. Phase 1 Agent-Storage — `XuguRelationalConnection` 包装 `XGConnection`
4. `external/csharp-driver/` — 只读参考，Provider 项目引用 NuGet 或 ProjectReference

---

## 推荐引用方式（Provider 项目）

**Option A — NuGet（生产推荐）：**

```xml
<PackageReference Include="Xuguclient" Version="3.3.5" />
```

**Option B — 本地 ProjectReference（开发调试）：**

```xml
<ProjectReference Include="..\..\external\csharp-driver\XGCSClient\XGCSClient.csproj" />
```

同时确保 `xugusql.dll` 复制到输出目录。

---

## 相关路径

| 路径 | 说明 |
|------|------|
| `external/csharp-driver/XGCSClient/` | 驱动源码 |
| `external/csharp-driver/doc/` | 开发手册 |
| `external/csharp-driver/CORE_ConsoleApp/` | .NET Core 使用示例 |
| `external/csharp-driver/XGCSClientTests/` | 单元测试 |
