# Microsoft.EntityFrameworkCore.Xugu — 已知限制

> **面向用户**：本文列出 3.x 的功能边界与平台范围。使用前请先阅读 [GETTING-STARTED.md](GETTING-STARTED.md) 与 [USER-GUIDE.md](USER-GUIDE.md)。  
> **版本**：9.0.0（对齐 EF Core 9.0.x；含 3.0.0 GA + 3.0.1 runtime-gap + Phase 13）

若你遇到文档未列出的行为，请在 GitHub 仓库提交 Issue，并附上 XGCI 错误码与复现步骤。

## 测试门禁（L1 / L2 / L3）

| 层 | CI 触发 | 说明 |
|----|---------|------|
| **L1 Unit** | 每次 PR / push | 无 DB；SQL 金标 + NotSupported |
| **L2 Integration** | `main` / nightly / `v*` tag | **native 全量 + compat 全量**（均 `XUGU_REQUIRE_DATABASE=true`） |
| **L3 Experiential** | nightly / `v*` tag | NuGet 消费 + `dotnet ef` + MinimalApi 冒烟 |

详见 [TESTING.md](TESTING.md)。PR **不**跑 L2；主干与发布强制双方言完整集成。

## 方言模式（3.x）

| 模式 | 默认 | Identity 回读 | 说明 |
|------|------|---------------|------|
| **Xugu 原生** | **是** | `INSERT` + `SELECT … WHERE id = LAST_INSERT_ID()` | 产品默认；不执行 `SET compatible_mode` |
| **会话兼容模式** | 否（opt-in） | 同上 | `EnableCompatibleModeOnOpen(XuguCompatibleMode)` → `SET compatible_mode TO 'MYSQL'\|'ORACLE'\|'POSTGRESQL'` |

**标识符折叠**（仅会话行为，**不是** SQL 方言切换）见官方 `compatible_mode.md`：NONE/ORACLE→大写；MYSQL→不转换；POSTGRESQL→小写。

**明确不承诺**：ORACLE/POSTGRESQL 模式 **不** 提供 Oracle/PostgreSQL SQL 方言翻译；禁止据此宣称「可零改动迁移」。

**ADO 说明**：XuguDB 支持 `INSERT … RETURNING`，但当前 **XuguClient** 不通过 `DbDataReader` 暴露 RETURNING 行（`FieldCount=0`）。Provider 使用 `LAST_INSERT_ID()`；见 `harness/scripts/probe-returning.ps1` 与 `ado-driver-contract.md`。

`COMPATIBLE_MODE=MYSQL` **不是**产品目标或迁移承诺。Pomelo 移植测试在 CI 中通过 `XUGU_DIALECT_MODE=compat` 运行。

## 应用能力矩阵（Phase 13）

审核关键路径常驻门禁：`Category=AppCapabilityMatrix` + `harness/scripts/run-app-matrix-gate.ps1`（native+compat，`XUGU_REQUIRE_DATABASE=true`）。样例：`samples/AppCapabilityMatrix`。驱动读写契约见 `harness/contracts/ado-driver-contract.md`。

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

**状态：done（Phase 12 W3 — 12.304；Post-GA 物化闭环 2026-07-13）**

| 能力 | 查询（LINQ） | SaveChanges（INSERT/UPDATE） | 实体物化 / Include |
|------|-------------|------------------------------|-------------------|
| `DateOnly` | **支持** | **支持**（`XuguDateOnlyTypeMapping`） | **支持**（string converter 适配官方驱动） |
| `TimeOnly` | **支持** | **支持**（默认 store `TIME(3)`） | **支持**（同上） |

Phase 12 早期仅覆盖 LINQ 过滤/投影；**完整 SaveChanges 往返 + Include 含 `DATE` 列物化** 由 Post-GA `RuntimeGap` / `BuiltInDataTypesTests` 实库闭合（native/compat）。**未**修改 `external/csharp-driver`。

## DateTimeOffset

**状态：done（Post-GA Provider 适配，2026-07-13）**

| 能力 | 说明 |
|------|------|
| Store 类型 | `DATETIME WITH TIME ZONE` / `TIMESTAMP WITH TIME ZONE`（**不**生成未经文档支持的 `DATETIME WITH TIME ZONE(n)`） |
| 参数 / 字面量 | Provider 以 invariant 字符串携带偏移（如 `… +08:00`）写入 |
| 读回 | 兼容驱动常见 `…+8`（单数字小时）与 `…+08:00` |
| 驱动边界 | 官方驱动仍无完整原生 `DateTimeOffset` 绑定；闭环在 Provider `XuguTemporalValueConverters` |

验证：`RuntimeGapBaselineTests` DateTimeOffset SaveChanges 往返 PASS。

## COUNT / DateDiff 标量（驱动 E34412 适配）

**状态：done（Post-GA Provider CAST，2026-07-13）**

官方驱动对高精度聚合/`TIMESTAMPDIFF`（官方返回 `BIGINT`）走 `GetInt32` 时可能抛 `E34412`。Provider 侧：

| 场景 | SQL / 类型处理 |
|------|----------------|
| `Count` / `LongCount`（含 GroupBy、导航） | `CAST(COUNT(…) AS INTEGER\|BIGINT)` |
| `EF.Functions.DateDiff*` | `TIMESTAMPDIFF` 按 `BIGINT` 生成，公共 `int` API 末端再转 `INTEGER` |

验证：`RuntimeGapBaselineTests`（Category=`RuntimeGap`）native/compat **9/9** PASS。驱动上游仍可改进 `GetInt32`；当前 Provider 兜底已足够应用路径。

## Collation（排序规则）

**状态：excluded-with-evidence（Phase 12 W4 — OOS-03）**

XuguDB 文档未提供与 MySQL 等价的列级/表级 Collation Fluent API。以下 Pomelo 能力 **永久不实现**：

| Pomelo API | 处置 | 文档依据 |
|------------|------|----------|
| `HasCollation(...)` | **excluded** — `8.E4` | `reference/system-configuration-parameter/session-parameter/char_set.md` |
| `XuguCollationAttribute` | **excluded** — `8.DA2` | 同上；JDBC 不支持列级排序规则 |
| 表级 charset Fluent | **excluded** | 连接级 `CHAR_SET=UTF8`（`XuguModelBuilderExtensions` 文档注释） |

字符集在连接级通过 `CHAR_SET=UTF8`（或等价参数）配置，非 EF 模型注解。

## NetTopologySuite / Spatial

**状态：excluded-with-evidence（Phase 12 W4 — OOS-01）**

XuguDB 服务端有 `spatial-database/` SQL 能力，但 **无** EF Core NetTopologySuite 集成包或 ORM 映射文档。Pomelo 对等能力在独立 `EFCore.MySql.NTS` 包中。

| 能力 | 处置 |
|------|------|
| `geometry` / `geography` CLR 映射 | **不实现** |
| `SpatialMySqlTest` / `SpatialQuery*` | **不 port**（OUT OF SCOPE） |
| 空间索引 RTREE 迁移 | `NotSupportedException`（与 FULLTEXT 同类） |

## FULLTEXT 索引 / Match 查询

**状态：excluded-with-evidence（Phase 12 W4 — OOS-02）**

| 能力 | 处置 | 文档依据 |
|------|------|----------|
| `CREATE … FULLTEXT INDEX` | **NotSupported** | `reference/object/indexes.md` — 「目前不提供全文索引相关的内容」 |
| `MATCH … AGAINST` / `IsMatch` | **不实现** | 同上 |
| 文本搜索替代 | **`REGEXP_LIKE`** | `XuguRegexIsMatchTranslator`（8.Q15 done） |

## CONVERT_TZ / ConvertTimeZone

**状态：excluded-with-evidence（Phase 12 W4 — OOS-04）**

| 能力 | 处置 |
|------|------|
| `ConvertTimeZone` DbFunction | **不翻译** |
| `DateTimeOffset.LocalDateTime` LINQ | 客户端求值 |
| 时区配置 | 库级 `def_timezone` / 会话参数 |

## Scaffolding Baselines

**状态：excluded-with-evidence（Phase 12 W4 — OOS-05）**

Pomelo `Scaffolding/Baselines/**` 全量快照（86 文件）**不 port**。主路径由 `ScaffoldingIntegrationTests` + `ScaffoldingExtendedTests` 覆盖。

## ExecuteDelete / ExecuteUpdate

**状态：done（核心路径）**

Provider 支持 EF Core `ExecuteDelete()` / `ExecuteUpdate()` **核心**路径：

- 单表谓词过滤
- MySQL 风格多表 JOIN（无 `LIMIT`/`ORDER BY`/`DISTINCT`/`GROUP BY` 的受限 `SelectExpression`）

SQL 生成见 `XuguQuerySqlGenerator`；验证见 `ExecuteDeleteTests` / `ExecuteUpdateTests`。

### 不支持或边缘场景（13.205 清单）

| 场景 | 处置 | 测试 / 文档 |
|------|------|-------------|
| TPC / TPT 继承层次批量 DML | **NotSupported**（未实现） | LIMITATIONS；用 SaveChanges |
| TPH 单表 ExecuteUpdate/Delete | **支持**（核心路径） | ExecuteUpdateTests |
| Owned 类型上的 ExecuteUpdate/Delete | **NotSupported** | 使用 SaveChanges |
| 关联子查询作为批量更新目标 | **NotSupported** | — |
| 多表 UPDATE 带 `ORDER BY` / `LIMIT` | 拒绝翻译 | `IsValidSelectExpressionForExecuteUpdate` |
| 带 `DISTINCT` / `GROUP BY` / `HAVING` 的源查询 | 拒绝翻译 | — |

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

**状态：产品声明不支持自动并发异常（Phase 13.201 决策 C；vendor 仍跟踪 V-01）**

**决策（2026-07-19）**：在 `ROW_COUNT()` / batch affected-count 未由驱动或服务端提供前，采用 **(C)** — **不**承诺自动抛出 `DbUpdateConcurrencyException`。并发 token **列映射与 UPDATE WHERE** 仍可用；应用需自行比对版本或使用显式 SQL。

| 能力 | 状态 |
|------|------|
| 并发 token 列映射 / UPDATE 含 token 列 | **支持**（见 `OptimisticConcurrencyTests`） |
| `DbUpdateConcurrencyException` 自动检测 | **不支持（documented）** — VT-XUGU-ROWCOUNT-001；测试保持 Skip |

`XuguUpdateSqlGenerator` 使用 `SELECT 1` 作为 rows-affected 占位。实库 `ROW_COUNT()` → **E10049**（MYSQL compat 下亦然）。

**RecordsAffected fallback**：`XGDataReader.RecordsAffected` 存在，但 EF `UpdateAndSelectSqlGenerator` 从 batch 末 `SELECT` 标量读 affected rows，ADO fallback **不可行**（曾评估选项 B，否决）。

**解锁**：XuguDB 提供 `ROW_COUNT()` 或驱动暴露可被 EF 消费的 batch affected count → 重新评估选项 B 并取消 Skip。

## 平台支持（Windows / Linux RID）

**状态：signed-off platform exclusion（Phase 12 W5 — 12.509/PLAT-02）**

| 平台 | RID | 状态 |
|------|-----|------|
| Windows x64 | `win-x64` | **支持** — `xugusql.dll` 随 NuGet / 构建输出 |
| Linux x64 | `linux-x64` | **signed-off blocked** — 驱动无 `libxugusql.so`；VT-XUGU-LINUXRID-001 |

**预备**：`NativeAssets.props` + `EFCore.Xugu.csproj` 条件 `runtimes/linux-x64/native/` 打包（`.so` 存在时自动启用）。

**CI**：GitHub Actions 实库 job 为 **Windows-only**（12.508 signed-off）；Linux agent 待驱动发布 `.so` 后添加。

详见 `harness/references/platform-limitations-signed-off-12.509.md`。

## JSON 列（EF Core 映射）

**状态：done（11.109 Wave 2）— TypeMapping + DDL + LINQ/函数翻译 + 实库冒烟**

XuguDB **服务端**支持原生 `JSON` 列类型（LOB，最大 2GB）、`->` / `->>` 路径运算符及 28+ JSON 函数（`reference/sql/datatype/json.md`、`reference/function/json-functions/`）。

| 能力 | XuguDB | Provider 2.1.0 |
|------|--------|------------------|
| `CREATE TABLE … col JSON` | **支持** | **`XuguJsonTypeMapping` + DDL `JSON`（11.109a）** |
| `EF.Functions.JsonValue/JsonExtract/JsonContains*` | SQL 层支持 | **`XuguJsonDbFunctionsExtensions` + Translator（11.109b）** |
| `JsonScalarExpression` / `->` / `->>` | SQL 层支持 | **`XuguQuerySqlGenerator`（11.109b）** |
| Fluent `HasXuguJsonColumn()` | — | **done**（11.109c） |
| 实库 INSERT + JSON 函数查询 | — | **`JsonIntegrationTests`（11.109d；SkippableFact）** |
| 整列 JSON LOB 实体物化（`SELECT payload`） | 驱动 `String`/LOB | **边界（13.206）** — 小文档可通；大 LOB/`GetString` 可能失败；**推荐** `JsonValue`/`JsonExtract` 投影 |
| Pomelo `MySqlJson*` / `Json*MySqlTest` 全矩阵 | — | **skip** |
| EF `ToJson()` owned JSON 列 | — | **不承诺** |

**变通**：查询优先 `EF.Functions.JsonValue` / `JsonExtract`；整列读取仅适合小 JSON；见 `ado-driver-contract.md` G-06。

## 其他 excluded / blocked（摘要）

| 能力 | 处置 |
|------|------|
| `CREATE DATABASE` / `DROP DATABASE` | **excluded** — 运维手工建库 |
| NetTopologySuite / Spatial | **excluded** — OOS-01 |
| `FULLTEXT` 索引 / `Match` 查询 | **excluded** — OOS-02 |
| `CONVERT_TZ` / `ConvertTimeZone` | **excluded** — OOS-04 |
| Scaffolding Baselines 全量快照 | **excluded** — OOS-05 |
| Native Linux RID 打包 | **signed-off blocked** — PLAT-02 / VT-XUGU-LINUXRID-001 |
| ROW_COUNT 乐观并发 | **signed-off blocked** — PLAT-01 / VT-XUGU-ROWCOUNT-001 |
| 参数内联（OFFSET） | **done** — `10.201` |

详见 `harness/references/out-of-scope-approved-12.409.md` 与 `harness/tasks/BACKLOG.md`。
