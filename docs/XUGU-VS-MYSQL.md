# XuguDB EF Core Provider vs MySQL（Pomelo）对比指南

> **版本**：Microsoft.EntityFrameworkCore.Xugu **2.0.0**  
> **对照基准**：Pomelo.EntityFrameworkCore.MySql **9.0.0**（EF Core 9）  
> **更新**：2026-07-07

本文面向从 MySQL/Pomelo 迁移或并列评估 XuguDB 的开发者，说明功能对等、SQL 方言差异、测试覆盖与选型建议。技术细节以 [LIMITATIONS.md](LIMITATIONS.md) 与 `harness/contracts/sql-dialect.contract.md` 为准。

---

## 执行摘要

| 维度 | Xugu 2.0.0 | Pomelo / MySQL |
|------|------------|----------------|
| 架构对齐 | 目录与服务注册对齐 Pomelo 9.0.0 | 参考实现 |
| 源码文件 | **120** .cs（~62% Pomelo 194） | 194 .cs |
| FunctionalTests | **676** 列测（~**64%** Pomelo ~1050） | ~1050 |
| 核心 CRUD / LINQ / 迁移 | **支持** | 支持 |
| ExecuteDelete / ExecuteUpdate | **核心路径支持** | 支持 |
| JSON / Spatial / FULLTEXT | **不实现** | 支持 |
| 自动重试（Retry） | **API 入口有，实现 defer** | `EnableRetryOnFailure` |
| 连接串 | Xugu 键值对（`IP=…; DB=…`） | MySQL 标准 URI/键值 |
| 自增主键 DDL | `IDENTITY(1,1)` | `AUTO_INCREMENT` |
| GUID 存储 | 原生 `GUID`（16 字节） | 常映射 `CHAR(36)` |

**结论**：Xugu Provider 已具备生产级 CRUD、查询翻译、迁移与 Scaffolding **主路径**；与 Pomelo 差距主要在 **扩展生态**（JSON/NTS/FULLTEXT）、**部分高级 Query/测试矩阵** 及 **驱动级能力**（Retry、DateOnly SaveChanges、ROW_COUNT 乐观并发）。适合 **以 XuguDB 为目标库** 的新项目或可控迁移；若强依赖 MySQL 专有扩展，需逐项核对下文限制。

---

## 功能对等表

| 能力 | Xugu 2.0.0 | 说明 |
|------|------------|------|
| `UseXugu` / DI 注册 | ✅ supported | 对齐 `UseMySql` 模式 |
| 基础 LINQ（Where/Select/Join/GroupBy/Include） | ✅ supported | Northwind + AdHoc 大矩阵已测 |
| ExecuteDelete / ExecuteUpdate | ✅ supported | 单表 + MySQL 风格多表 JOIN；TPC/TPT/Owned 批量 DML ❌ |
| 迁移（Create/Alter/Drop、索引、FK） | ✅ supported | 列重命名：`ADD+UPDATE+DROP` workaround |
| Scaffolding（`dotnet ef dbcontext scaffold`） | ⚠️ partial | DBA/ALL 视图；无 Collation/Charset 列级还原 |
| SequentialGuid | ✅ supported | 客户端 ticks+random；查询 `SYS_GUID()` |
| 乐观并发 token 列 | ✅ supported | `DbUpdateConcurrencyException` 检测 ⚠️ defer |
| DateOnly / TimeOnly 查询 | ✅ supported | SaveChanges 写入 ⚠️ defer（驱动） |
| `EnableRetryOnFailure` | ❌ defer | 抛 `NotSupportedException` |
| Collation / `HasCharSet` Fluent | ❌ skip | 连接串 `CHAR_SET` |
| JSON 列 / `Json*` API | ❌ skip | 无 Xugu JSON 列生态 |
| NetTopologySuite / Spatial | ❌ skip | — |
| FULLTEXT / `Match` | ❌ skip | — |
| `CONVERT_TZ` / ConvertTimeZone | ❌ skip | 无等价函数 |
| CREATE/DROP DATABASE | ❌ defer | 运维手工建库 |
| 参数内联（8.Q14） | ❌ defer | 性能优化 |
| FOR UPDATE / 窗口函数（8.Q12） | ❌ defer | — |
| 位运算返回类型修正（8.Q11） | ❌ defer | — |
| Linux x64 原生 RID 打包（8.N1–N3） | ❌ defer | 当前 Windows x64 |
| Pomelo IntegrationTests 宿主 | ❌ defer（9.IT2） | 低优先级 |
| Monster / Specification 全量测试 | ❌ defer | → Phase 10 按需 |

图例：**✅ supported** · **⚠️ partial** · **❌ skip/defer**

---

## SQL 方言差异

> 权威：XuguDB 官方文档 `E:\BaiduSyncdisk\docs\content\`；项目契约 `harness/contracts/sql-dialect.contract.md`

### 连接与兼容模式

| 项 | XuguDB | MySQL/Pomelo |
|----|--------|-------------|
| 连接串 | `IP=…; DB=…; USER=…; PWD=…; PORT=5138` | `Server=…;Database=…;` 等 |
| MySQL 兼容 | 连接后 `SET compatible_mode TO 'MYSQL'`（Provider 默认） | 原生 MySQL |
| 标识符（MYSQL 模式） | 反引号；**不做**大小写转换 | 同左 |
| 字符集 | 连接级 `CHAR_SET=UTF8` | 表/列级 `HasCharSet` |

文档：`reference/system-configuration-parameter/session-parameter/compatible_mode.md`

### 自增与 Identity

| 项 | XuguDB | MySQL |
|----|--------|-------|
| DDL | `INTEGER IDENTITY(1,1)` | `INT AUTO_INCREMENT` |
| NULL/0 插入行为 | 受 `identity_mode` 影响（0/1/2） | `sql_mode` 相关 |
| Identity PK **类型变更** | 迁移 **NotSupported** | Pomelo 可重建列 |

文档：`reference/object/table/create.md`、`def_identity_mode.md`

### 类型映射（节选）

| CLR | XuguDB | MySQL/Pomelo |
|-----|--------|-------------|
| `Guid` | `GUID`（二进制） | 常 `CHAR(36)` |
| `uint` | `BIGINT` | `INT UNSIGNED` |
| `ulong` | `NUMERIC(20,0)` | `BIGINT UNSIGNED` |
| `bool` | `BOOLEAN` | `TINYINT(1)` |
| `DateTimeOffset` | `DATETIME/TIMESTAMP WITH TIME ZONE` | `datetime(6)` 等 |
| MySQL `YEAR` 类型 | **无** | 有 |

### DML / 查询生成差异

| 场景 | XuguDB | MySQL/Pomelo |
|------|--------|-------------|
| 分页 | `LIMIT` + 可选 `TOP` | 主要 `LIMIT` |
| 单表 DELETE + LIMIT | `DELETE FROM t WHERE … LIMIT n` | 类似 |
| 多表 DELETE | `DELETE FROM t1 FROM t2 …` | `DELETE t1 FROM t1 JOIN …` |
| 多表 UPDATE + LIMIT | **不支持** LIMIT | 部分支持 |
| INSERT 回读 | 支持 **RETURNING**（Xugu 特有） | 有限支持 |
| 索引删除 | `DROP INDEX table.index` | `ALTER TABLE … DROP INDEX` |
| 列重命名 | 无 `RENAME COLUMN`；三语句 workaround | `RENAME COLUMN` / `CHANGE` |
| 表/列注释 | `COMMENT ON TABLE/COLUMN` | `COMMENT=` |

### 函数与表达式（LINQ → SQL）

| C# | XuguDB | MySQL/Pomelo |
|----|--------|-------------|
| `Guid.NewGuid()` | `SYS_GUID()` | `UUID()` |
| `DateTime.AddDays` | `TIMESTAMPADD(day, n, dt)` | `DATE_ADD(… INTERVAL …)` |
| `DateTime.Date` | `DATE(dt)` | `CONVERT(…, date)` |
| `Math.Log(x)` | `LN(x)` | `LOG(x)` |
| `Math.Log(x, base)` | `LOG(base, x)` **参数序相反** | `LOG(x, base)` |
| `Regex.IsMatch` | `REGEXP_LIKE(…)` | `REGEXP` 运算符 |
| `string.Equals(…, IgnoreCase)` | `LCASE` 双端 | `COLLATE …_bin` |
| `DateTimeOffset.Now` | `SYSTIMESTAMP()` | `UTC_TIMESTAMP()` 等 |
| `ConvertTimeZone` | **不实现** | `CONVERT_TZ` |

完整映射表见 `sql-dialect.contract.md` §函数映射表。

---

## 测试覆盖差距

| 指标 | Xugu 2.0.0 | Pomelo 9.0.0 |
|------|------------|--------------|
| FunctionalTests 列测 | **676** | ~**1050** |
| 可比覆盖率 | **~64%** | 100% |
| 剩余差距 | **~374** 方法 | — |
| 全量实库门禁 | **0 FAIL**（5 显式 Skip） | — |

### 已覆盖（Phase 9）

- Northwind Query 六套件（Where/Join/Group/Order/Select/Include）：+252
- CRUD、GraphUpdates、ManyToMany、TableSplitting、FieldMapping
- OptimisticConcurrency（除异常检测）、StoreGenerated、Seeding 子集
- DesignTime、ServiceCollection、ApiConsistency、ExistingConnection 子集

### 未覆盖 / Phase 10

| Pomelo 类别 | 处置 |
|-------------|------|
| MonsterFixup / Changed / Changing | Phase 10 按需子集 |
| Specification Tests | Phase 10 按需 |
| Spatial / Match / JSON | 永久 skip |
| Scaffolding Baselines 快照 | skip（维护成本） |
| IntegrationTests（Vegeta/ASP.NET） | 9.IT2 defer |

详见 `harness/references/test-parity-matrix.md`、`harness/handoffs/phase9-m3-test-parity-2026-07-07.md`。

---

## 连接与配置差异

### Xugu 连接串示例

```
IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8
```

环境变量：`XUGU_CONNECTION_STRING`（测试/CI 常用）。

### MySQL（Pomelo）对比

```csharp
// Pomelo
options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

// Xugu
options.UseXugu(connectionString);
// 默认 SET compatible_mode TO 'MYSQL'
```

### 关键会话参数

| 参数 | 用途 | 文档 |
|------|------|------|
| `compatible_mode=MYSQL` | MySQL 方言、标识符行为 | `compatible_mode.md` |
| `identity_mode` / `def_identity_mode` | IDENTITY 列 NULL/0 插入 | `def_identity_mode.md` |
| `CHAR_SET` | 字符集（替代 HasCharSet） | 连接串 |

### 执行策略

| API | Pomelo | Xugu |
|-----|--------|------|
| `EnableRetryOnFailure()` | 可用 | **NotSupportedException** |
| 自定义 `IExecutionStrategyFactory` | 可用 | **推荐**（自行解析 XGCI 码） |

---

## 迁移与 Scaffolding 差异

| 能力 | Xugu | Pomelo/MySQL |
|------|------|-------------|
| `dotnet ef migrations add` | ✅ | ✅ |
| `dotnet ef database update` | ✅（LOCK TABLE 迁移锁） | ✅ |
| Identity 列 | `IDENTITY(1,1)` | `AUTO_INCREMENT` |
| 过滤索引 `HasFilter` | Differ 剥离（DDL 不支持） | 支持 |
| 索引类型 | BTREE + **BITMAP**（Xugu 特有） | BTREE/FULLTEXT/SPATIAL |
| 列 Collation 迁移 | 忽略 | 生成 ALTER |
| Scaffolding 元数据 | `DBA_TABLES` / `ALL_COLUMNS` 等 | `INFORMATION_SCHEMA` |
| 视图反向工程 | `ALL_VIEWS` | `INFORMATION_SCHEMA.VIEWS` |

Scaffolding 不会还原 Pomelo 的 Collation/Charset 注解；请在连接串统一配置 `CHAR_SET`。

---

## 已知限制与变通

| 限制 | 变通 |
|------|------|
| 无 Retry | 自定义 ExecutionStrategy；或应用层 Polly |
| 无 JSON 列 | 用 `VARCHAR` + 应用序列化，或等待 Phase 10 JSON 调研 |
| 无 FULLTEXT | 应用层搜索引擎；或 LIKE（小数据） |
| DateOnly/TimeOnly SaveChanges 未验证 | 查询可用；写入用 `DateTime` 或 raw SQL |
| 乐观并发异常未检测 | 业务层版本检查；或等待 ROW_COUNT 路径 |
| Identity PK 类型变更 | 手工迁移（新列+拷贝+切 PK） |
| 无 CREATE DATABASE | DBA 预先建库 |
| 仅 Windows x64 原生包 | Linux 部署待 8.N1–N3 |

完整列表：[LIMITATIONS.md](LIMITATIONS.md)

---

## 何时选 Xugu vs Pomelo

### 选择 Xugu Provider

- 目标数据库为 **XuguDB**（虚谷）
- 需要 EF Core 9 + 标准 CRUD/LINQ/迁移主路径
- 可接受连接串与 MySQL 差异，并在连接级配置字符集/兼容模式
- 不依赖 JSON 列、Spatial、FULLTEXT、Collation Fluent

### 选择 Pomelo（MySQL）

- 目标库为 **MySQL / MariaDB**
- 需要 JSON、Spatial、FULLTEXT、Retry、完整 Pomelo 测试矩阵
- 依赖 MySQL 特有 DDL（`AUTO_INCREMENT`、列级 Collation、Scaffolding Baselines）
- 需要 Linux 多 RID 官方原生包与成熟生态

### 从 Pomelo 迁移到 Xugu（检查清单）

1. 改写连接串与 `UseXugu`
2. 确认 `COMPATIBLE_MODE=MYSQL` 与 `CHAR_SET`
3. 检查 `Guid` 存储（二进制 vs 字符串）
4. 迁移脚本中的 `AUTO_INCREMENT` → `IDENTITY`
5. 移除 `HasCollation` / JSON / Spatial 配置
6. 评估 Retry、DateOnly SaveChanges、乐观并发检测需求
7. 跑集成测试；参考 [TESTING.md](TESTING.md)

---

## 参考文档

| 文档 | 路径 |
|------|------|
| 已知限制 | [LIMITATIONS.md](LIMITATIONS.md) |
| 快速开始 | [GETTING-STARTED.md](GETTING-STARTED.md) |
| 测试说明 | [TESTING.md](TESTING.md) |
| SQL 方言契约 | `harness/contracts/sql-dialect.contract.md` |
| 测试对等矩阵 | `harness/references/test-parity-matrix.md` |
| Phase 9 Handoff | `harness/handoffs/phase9-m3-test-parity-2026-07-07.md` |
| XuguDB 官方文档 | `E:\BaiduSyncdisk\docs\content\` |
| Pomelo 参考源码 | `external/Pomelo.EntityFrameworkCore.MySql/` |
