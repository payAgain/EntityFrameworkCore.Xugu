# Microsoft.EntityFrameworkCore.Xugu — 已知限制

> Phase 7 Wave 1 初稿；Phase 8 / gap analysis 补全。完整路线图见 `harness/tasks/ROADMAP.md`。

## 自动重试（Execution Strategy）

**状态：done（Phase 10 Wave 4 — 10.106）**

Provider 默认注册 `XuguExecutionStrategy`（`RetriesOnFailure => false`）。调用 `EnableRetryOnFailure()` 时注册 `XuguRetryingExecutionStrategy`，通过解析 `Exception.Message` 中的 XGCI 码判定瞬态错误。

### 实现说明

| 项 | 说明 |
|----|------|
| 瞬态检测 | `XuguTransientExceptionDetector` 解析 `[E19886]`、`[E32506]`、`[E34304]`、`[E34305]` 等 |
| 驱动限制 | 驱动仍抛出 `System.Exception`，无 `DbException.ErrorCode` / `IsTransient` |
| `errorNumbersToAdd` | Pomelo 兼容参数 **忽略**（Xugu 使用字符串 XGCI 码，非数字 error number） |
| 故障注入 | 尚无集成测试模拟 idle disconnect；依赖单元测试 + 实库偶发瞬态 |

### 残余风险

若驱动团队变更 Message 中 XGCI 码格式，瞬态判定可能失效。长期建议驱动提供 `XuguException` + 结构化 `IsTransient`。

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

**状态：blocked（Phase 10 Wave 4 实库验证 — 10.105）**

`XuguUpdateSqlGenerator` 在 batch 完成时使用 `SELECT 1` 作为 rows-affected 占位。实库启用 `ROW_COUNT()` 后 XuguDB 返回 **E10049**（函数不存在），即使在 `COMPATIBLE_MODE=MYSQL` 下亦不可用。

| 能力 | 状态 |
|------|------|
| 并发 token 列映射 / UPDATE 含 token 列 | **支持**（见 `OptimisticConcurrencyTests`） |
| `DbUpdateConcurrencyException` 检测 | **blocked** — 需 XuguDB 提供等价 affected-rows API 或驱动返回真实 `RecordsAffected` |

**实库证据（2026-07-08）**：`[E10049 L3 C9] 字段变量或函数"ROW_COUNT"()不存在`

**后续**：驱动 `RecordsAffected` 可靠返回，或 XuguDB 文档确认 MySQL 兼容函数后再解锁 `Stale_concurrency_token_throws_DbUpdateConcurrencyException`。

## JSON 列（EF Core 映射）

**状态：部分实现 — 11.109a TypeMapping + DDL done；LINQ/路径翻译待 11.109b**

XuguDB **服务端**支持原生 `JSON` 列类型（LOB，最大 2GB）、MySQL 风格 `->` / `->>` 路径运算符及 28+ JSON 函数（`reference/sql/datatype/json.md`、`reference/function/json-functions/`）。

| 能力 | XuguDB | Provider 2.0.x |
|------|--------|----------------|
| `CREATE TABLE … col JSON` | **支持** | **`XuguJsonTypeMapping` + DDL `JSON`（11.109a done）** |
| LINQ JSON 列查询（`EF.Property` / owned JSON） | SQL 层支持 | **未实现**（11.109b 待办） |
| Pomelo `MySqlJson*` / `Json*MySqlTest` | — | **skip**（无 Fluent / TypeMapping） |

**变通**：应用层 `VARCHAR`/`CLOB` + 序列化；或 raw SQL 使用 `JSON_EXTRACT` / `->` 运算符。

**解锁 10.109 前置**：`XuguJsonTypeMapping`、`JsonScalarExpression` 翻译、`XuguQuerySqlGenerator` 路径遍历；可选 Microsoft/Newtonsoft 扩展包（对标 Pomelo `EFCore.MySql.Json.*`）。

## 其他 defer / skip（摘要）

| 能力 | 处置 |
|------|------|
| `CREATE DATABASE` / `DROP DATABASE` | defer — 运维手工建库 |
| `CONVERT_TZ` / `ConvertTimeZone` DbFunction | skip — 无等价函数 |
| `FULLTEXT` 索引 / `Match` 查询 | skip |
| NetTopologySuite / Spatial | skip |
| Native Linux RID 打包 | **blocked** — `10.205`（无 `libxugusql.so`） |
| 参数内联（OFFSET） | **done** — `10.201` |

详见 `harness/tasks/BACKLOG.md`。
