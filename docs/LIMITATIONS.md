# Microsoft.EntityFrameworkCore.Xugu — 已知限制

> Phase 7 Wave 1 初稿；Phase 8 / gap analysis 补全。完整路线图见 `harness/tasks/ROADMAP.md`。

## 自动重试（Execution Strategy）

**状态：defer（Phase 7 Wave 1 确认）**

Provider 默认注册 `XuguExecutionStrategy`（`RetriesOnFailure => false`），**未**提供 `XuguRetryingExecutionStrategy`。

`EnableRetryOnFailure()` Fluent API 已对齐 Pomelo 入口，但调用时抛出 `NotSupportedException`（`XuguStrings.RetryingExecutionStrategyNotSupported`）。

### 阻塞原因

| 项 | 说明 |
|----|------|
| 驱动异常类型 | `external/csharp-driver` 抛出 `System.Exception`，非 `DbException` 子类 |
| 错误码 | XGCI 码嵌入 `Exception.Message`（如 `[E34501]:System.CommandExecuteException:sqlexecute err: …`），无 `ErrorCode` / `IsTransient` API |
| 瞬态码未映射 | 驱动源码中无 `E19886`（空闲断开）、`E32506`（连接超时断开）等常量 |
| 与 Pomelo 对比 | MySqlConnector 提供 `MySqlException.IsTransient`；Xugu 无等价能力 |

### 用户替代方案

在 `UseXugu(..., o => o.UseXuguExecutionStrategy(...))` 基础上，可注册自定义 `IExecutionStrategyFactory`，自行解析 `Exception.Message` 中的 XGCI 码决定是否重试。

### 后续前置条件

1. 驱动提供 `XuguException` + 结构化 `ErrorCode` / `IsTransient`；或
2. 与驱动团队确认 Message 中 XGCI 码格式长期稳定，并补充故障注入集成测试。

详见 `harness/references/retrying-execution-strategy.md`。

## DateOnly / TimeOnly SaveChanges

**状态：defer（驱动绑定）**

| 能力 | 查询（LINQ） | SaveChanges（INSERT/UPDATE） |
|------|-------------|------------------------------|
| `DateOnly` | **支持**（`DATE` 映射 + Translator） | **未验证 / 可能失败** — ADO.NET 驱动无 `DateOnly` 原生参数绑定 |
| `TimeOnly` | **支持**（`TIME` 映射 + Translator） | **未验证 / 可能失败** — 同上 |

**当前测试策略**：`BuiltInDataTypesTests` 等通过 fixture **raw SQL** 插入 DateOnly/TimeOnly 列，仅断言 LINQ 读取/过滤；不以 EF `SaveChanges` 写入这些类型作为门禁。

**后续前置条件**：`csharp-driver` 支持 `DateOnly`/`TimeOnly` 参数化，或 Provider 显式转换为驱动接受的 `DateTime`/`string` 绑定并补充往返测试。

## Collation（排序规则）

**状态：skip**

XuguDB 文档未提供与 MySQL 等价的列级/表级 Collation Fluent API。以下 Pomelo 能力 **不实现**：

| Pomelo API | 处置 |
|------------|------|
| `HasCollation(...)` | skip — `8.E4` |
| `XuguCollationAttribute` | skip — `8.DA2` |
| 表级 charset Fluent | skip — 使用连接串 `CHAR_SET`（见 `XuguModelBuilderExtensions` 文档注释） |

字符集在连接级通过 `CHAR_SET=UTF8`（或等价参数）配置，非 EF 模型注解。

## ExecuteDelete / ExecuteUpdate

**状态：done（核心路径）**

Provider 支持 EF Core `ExecuteDelete()` / `ExecuteUpdate()` **核心**路径：

- 单表谓词过滤
- MySQL 风格多表 JOIN（无 `LIMIT`/`ORDER BY`/`DISTINCT`/`GROUP BY` 的受限 `SelectExpression`）

SQL 生成见 `XuguQuerySqlGenerator`；验证见 `ExecuteDeleteTests` / `ExecuteUpdateTests`。

### 不支持或边缘场景

| 场景 | 处置 |
|------|------|
| TPC / TPT 继承层次批量 DML | 未实现 |
| Owned 类型上的 ExecuteUpdate/Delete | 未实现 |
| 关联子查询作为批量更新目标 | 未实现 |
| 多表 UPDATE 带 `ORDER BY` / `LIMIT` | `IsValidSelectExpressionForExecuteUpdate` 拒绝 |
| 带 `DISTINCT` / `GROUP BY` / `HAVING` 的源查询 | 拒绝翻译 |

需要上述能力时请使用常规 `SaveChanges` 或显式 SQL。

## Identity 主键类型变更（迁移）

**状态：NotSupported**

将 **IDENTITY 列**（`UseXuguIdentityColumn` / `INTEGER IDENTITY(1,1)`）作为主键的列 **变更 CLR/存储类型** 时，`XuguMigrationsSqlGenerator` 抛出 `NotSupportedException`（`XuguStrings.IdentityPrimaryKeyTypeChangeNotSupported`）。

**原因**：XuguDB IDENTITY 列重建策略与 Pomelo `AUTO_INCREMENT` 不同；自动迁移可能导致数据丢失。

**替代方案**：手工迁移（新列 + 数据拷贝 + 切换主键 + 删除旧列），或新建表迁移。

## 无符号整数

XuguDB 文档（`reference/sql/datatype/numerical.md`）仅定义有符号整数。Provider 映射：

| CLR | 存储类型 | 说明 |
|-----|---------|------|
| `uint` | `BIGINT` | 最大值 4_294_967_295 在 BIGINT 范围内 |
| `ulong` | `NUMERIC(20,0)` | 超出 BIGINT 上限时使用 NUMERIC |

## GUID 存储

CLR `Guid` 默认映射 XuguDB 原生 `GUID`（16 字节），非 MySQL 风格 `CHAR(36)`。见 `reference/sql/datatype/guid.md`。

## 乐观并发与 ROW_COUNT()

**状态：defer（驱动 / Update 批次）**

`XuguUpdateSqlGenerator` 在 batch 完成时使用 `SELECT 1` 作为 rows-affected 占位，而非 MySQL 风格 `ROW_COUNT()`。因此：

| 能力 | 状态 |
|------|------|
| 并发 token 列映射 / UPDATE 含 token 列 | **支持**（见 `OptimisticConcurrencyTests`） |
| `DbUpdateConcurrencyException` 检测 | **defer** — 需驱动返回真实 affected rows 或 Provider 切换为 `ROW_COUNT()` 且回归 CRUD 测试 |

**原因**：全量启用 `ROW_COUNT()` 曾破坏现有 CRUD/Update 测试；在驱动与方言契约确认前不默认开启。

**后续**：驱动暴露稳定 affected-rows API 或 XuguDB 文档确认 `ROW_COUNT()` 语义后，再解锁 `OptimisticConcurrencyTests.Stale_concurrency_token_throws_DbUpdateConcurrencyException`。

## 其他 defer / skip（摘要）

| 能力 | 处置 |
|------|------|
| `CREATE DATABASE` / `DROP DATABASE` | defer — 运维手工建库 |
| `CONVERT_TZ` / `ConvertTimeZone` DbFunction | skip — 无等价函数 |
| `FULLTEXT` 索引 / `Match` 查询 | skip |
| JSON 列 / NetTopologySuite | skip |
| Native Linux RID 打包 | defer — `8.N1–N3` |

详见 `harness/tasks/BACKLOG.md`。
