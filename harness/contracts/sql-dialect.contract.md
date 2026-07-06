# XuguDB SQL 方言契约（Living Document）

> **权威来源**：`E:\BaiduSyncdisk\docs\content\`  
> 所有 Agent 实现 SQL 相关代码前必须阅读本文档 + 对应官方文档。  
> 发现新差异时更新本文档并注明文档路径。

## 数据库信息

| 项 | 值 |
|----|-----|
| 数据库 | XuguDB（虚谷数据库） |
| EF Core 包名 | `Microsoft.EntityFrameworkCore.Xugu` |
| 连接 API | `UseXugu(connectionString)` |
| 连接串示例 | `IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8` |
| ADO.NET 驱动 | `XuguClient.dll` + 原生 `xugusql.dll`（见 `external/csharp-driver/`） |
| 兼容模式 | `SET compatible_mode TO 'MYSQL';`（开发对照 MySQL 时使用） |
| 文档根目录 | `E:\BaiduSyncdisk\docs\content\` |

## 兼容模式行为

> 文档：`reference/system-configuration-parameter/session-parameter/compatible_mode.md`

| COMPATIBLE_MODE | 标识符处理 |
|-----------------|-----------|
| NONE / ORACLE | 词法阶段转大写 |
| **MYSQL** | **不做大小写转换** |
| POSTGRESQL | 词法阶段转小写 |

Provider 默认应在连接建立后确保 MySQL 兼容模式（若目标场景为 MySQL 方言开发）。

## 标识符

> 文档：`reference/sql/identifier.md`

| 项 | XuguDB 规则 |
|----|------------|
| 引号 | 双引号 `"` 或反引号 `` ` `` |
| 非引号标识符 | 不区分大小写（MYSQL 模式下不做转换） |
| 加引号标识符 | 区分大小写 |
| 最大长度 | 1–127 字节 |
| Schema | 支持，`schema.object` 形式 |

**Provider 实现建议（MYSQL 模式）**：

- `SqlGenerationHelper.DelimitIdentifier()` 使用反引号 `` ` ``（对齐 MySQL/Pomelo）
- 表名/列名映射遵循 MYSQL 模式不做自动大小写转换

## 分页

> 文档：`reference/sql/select/resultset-restricted.md`

XuguDB 同时支持 **LIMIT** 和 **TOP**：

```sql
-- 形式 1（MySQL 风格）
SELECT * FROM t LIMIT {count};
SELECT * FROM t LIMIT {offset}, {count};
SELECT * FROM t LIMIT {count} OFFSET {offset};

-- 形式 2（SQL Server 风格）
SELECT TOP {n} * FROM t ORDER BY ...;
```

**EF Core Provider 映射**：

| LINQ | 生成 SQL | 文档依据 |
|------|---------|---------|
| `.Take(n)` | `LIMIT n` | resultset-restricted.md |
| `.Skip(o).Take(n)` | `LIMIT o, n` 或 `LIMIT n OFFSET o` | resultset-restricted.md |
| `.Take(n)` + OrderBy (SQL Server 模式) | 可选 `TOP n` | resultset-restricted.md §TOP |

## 自增主键（IDENTITY）

> 文档：`reference/system-configuration-parameter/xugu.ini/compatible/def_identity_mode.md`

XuguDB 使用 `IDENTITY(seed, increment)` 而非 MySQL `AUTO_INCREMENT`：

```sql
CREATE TABLE t1(c1 INTEGER IDENTITY(1, 1));
```

| identity_mode | INSERT NULL 行为 |
|---------------|-----------------|
| 0 (DEFAULT) | 报错 E16005 |
| 1 (NULL_AS_AUTO_INCREMENT) | NULL 替换为自增值 |
| 2 (ZERO_AS_AUTO_INCREMENT) | NULL 或 0 替换为自增值 |

**Provider 实现要点**：

- 模型约定：映射为 `IDENTITY(1,1)`，不是 `AUTO_INCREMENT`
- SaveChanges 回读：需查 XuguDB 文档确认等效于 `LAST_INSERT_ID()` 的方式
- 与 Pomelo 差异：**必须在 MigrationsSqlGenerator 和 Convention 中单独实现**

## 数据类型映射（CLR → XuguDB）

> 文档：`reference/sql/datatype/`

| CLR 类型 | XuguDB 类型 | 文档 |
|----------|------------|------|
| `bool` | `BOOLEAN` | `datatype/bool.md` |
| `byte` | `TINYINT` | `datatype/numerical.md` |
| `short` | `SMALLINT` | `datatype/numerical.md` |
| `int` | `INTEGER` | `datatype/numerical.md` |
| `long` | `BIGINT` | `datatype/numerical.md` |
| `decimal` | `NUMERIC/DECIMAL/NUMBER` | `datatype/numerical.md` |
| `float` | `FLOAT` | `datatype/numerical.md` |
| `double` | `DOUBLE` | `datatype/numerical.md` |
| `string` | `VARCHAR(n)` / `CHAR(n)` | `datatype/character.md` |
| `DateTime` | `DATETIME` | `datatype/datetime.md` |
| `DateOnly` | `DATE` | `datatype/datetime.md` |
| `TimeOnly` | `TIME` | `datatype/datetime.md` |
| `DateTimeOffset` | `DATETIME WITH TIME ZONE` / `TIMESTAMP WITH TIME ZONE` | `datatype/datetime.md` |
| `Guid` | 见 GUID 文档 | `datatype/guid.md` |
| `byte[]` | 见二进制文档 | `datatype/binary.md` |

## INSERT 语句

> 文档：`reference/sql/dml/insert.md`

支持：

- 标准 INSERT（单行/多行）
- REPLACE INTO
- INSERT IGNORE INTO
- RETURNING 子句（XuguDB 特有，见 insert.md）

**与 Pomelo/MySQL 差异**：XuguDB 有 RETURNING，Provider 可利用此特性做 INSERT 回读（需查文档确认语法）。

## 函数映射表

> 文档：`reference/sql/expression/function.md` 及 `reference/function/` 目录  
> **实现每个 Translator 前必须打开对应函数文档**

| C# 表达式 | SQL（XuguDB） | Pomelo 参考 | 负责 Agent | 状态 |
|-----------|--------------|------------|-----------|------|
| `string.Contains(s)` | `LIKE CONCAT('%', s, '%')` | LIKE / LOCATE | QueryTranslators | done |
| `string.StartsWith(s)` | `LIKE CONCAT(s, '%')` | LIKE | QueryTranslators | done |
| `string.EndsWith(s)` | `LIKE CONCAT('%', s)` | LIKE | QueryTranslators | done |
| `string.Length` | `LENGTH()` | CHAR_LENGTH | QueryTranslators | done |
| `DateTime.Year` | `YEAR()` | `EXTRACT(year FROM …)` / `YEAR()` | QueryTranslators | done |
| `DateTime.Month/Day/Hour/Minute/Second` | `MONTH()` 等 | `EXTRACT(… FROM …)` | QueryTranslators | done |
| `DateTime.Millisecond` | `MICROSECOND()/1000` | `EXTRACT(microsecond FROM …)/1000` | QueryTranslators | done |
| `DateTime.Date` | `DATE()` | `CONVERT(…, date)` | QueryTranslators | done |
| `DateTime.Now/UtcNow/Today` | `CURRENT_TIMESTAMP()` / `UTC_TIMESTAMP()` / `CURDATE()` | 同左（MySQL 风格） | QueryTranslators | done |
| `DateTime.AddDays(n)` 等 | `TIMESTAMPADD(unit, n, dt)` | `DATE_ADD(dt, INTERVAL n unit)` | QueryTranslators | done |
| `DateTime.DayOfWeek` | `DAYOFWEEK()-1` | 同左（ODBC 索引 1=Sunday） | QueryTranslators | done |
| `Math.Abs(x)` | `ABS()` | ABS() | QueryTranslators | done |
| `Guid.NewGuid()` | `SYS_GUID()` | `UUID()` | QueryTranslators | done |
| `Convert.To*(x)` | `CAST(x AS type)` | `CONVERT(x, type)` / `CAST` | QueryTranslators | done |
| `DateTimeOffset.Now` | `SYSTIMESTAMP()` | `UTC_TIMESTAMP()` | QueryTranslators | done |
| `DateTimeOffset.UtcNow` | `UTC_TIMESTAMP()` | `UTC_TIMESTAMP()` | QueryTranslators | done |
| `DateTimeOffset.ToUnixTime*` | `TIMESTAMPDIFF(...)` | 同左 | QueryTranslators | done |
| `TimeOnly.FromDateTime(dt)` | `TIME(dt)` | `TIME(dt)` | QueryTranslators | done |
| `TimeOnly.AddHours/Minutes` | `ADDTIME(CAST(t AS TIME), INTERVAL n unit)` | `DATE_ADD` / `ADDTIME` | QueryTranslators | done |
| `EF.Functions.Degrees/Radians` | `DEGREES()` / `RADIANS()` | 同左 | QueryTranslators | done |
| `double.RadiansToDegrees/DegreesToRadians` | `DEGREES()` / `RADIANS()` | 同左 | QueryTranslators | done |
| `DateOnly.ToDateTime(time)` | `MAKE_TIMESTAMP(...)` | `ADDTIME(CAST(...), time)` | QueryTranslators | done |
| `DateOnly.DayNumber` | `TO_DAYS(d) - 366` | 同左 | QueryTranslators | done |
| `XuguDbFunctionsExtensions.DateDiff*` | `TIMESTAMPDIFF(unit, start, end)` | 同左 | QueryTranslators | done |
| `byte[].Contains(b)` | `LOCATE(b, arr) > 0` | 同左 | QueryTranslators | done |
| `Enumerable.First(byte[])` | `ASCII(arr)` | 同左 | QueryTranslators | done |
| `XuguDbFunctionsExtensions.Like` | `LIKE` | 同左 | QueryTranslators | done |
| `XuguDbFunctionsExtensions.Hex` | `HEX(expr)` | 同左 | QueryTranslators | done |
| `XuguDbFunctionsExtensions.Unhex` | `UNHEX(expr)` | 同左 | QueryTranslators | done |
| `object.ToString()` | `CAST(expr AS VARCHAR)` | 同左 | QueryTranslators | done |
| `Regex.IsMatch(s, pattern)` | `REGEXP_LIKE(expr, pattern)` | `expr REGEXP pattern` | QueryTranslators | done |

## 索引 DDL

> 文档：`reference/object/indexes.md`

| 项 | XuguDB | MySQL/Pomelo | Provider |
|----|--------|-------------|----------|
| 创建 | `CREATE [UNIQUE] INDEX name ON table (cols) [INDEXTYPE IS BTREE\|BITMAP]` | `CREATE INDEX … USING BTREE` / FULLTEXT | `XuguMigrationsSqlGenerator.IndexOptions` |
| 删除 | `DROP INDEX table.index_name` | `ALTER TABLE … DROP INDEX …` | `Generate(DropIndexOperation)` |
| 重命名 | `ALTER INDEX table.old RENAME TO new` | `ALTER TABLE … RENAME INDEX …` | `Generate(RenameIndexOperation)` |
| 位图索引 | `INDEXTYPE IS BITMAP` | 不支持 | `HasIndexType(Bitmap)` |
| 全文/RTREE | 文档未对外发布 FULLTEXT tail opt | FULLTEXT / SPATIAL | Migration **NotSupported** |

## 已知 XuguDB vs MySQL 差异（必读）

| 差异点 | XuguDB | MySQL/Pomelo | 处理 |
|--------|--------|-------------|------|
| 自增列 | `IDENTITY(1,1)` | `AUTO_INCREMENT` | Migrations + Convention |
| 兼容模式 | 需 SET compatible_mode | 不需要 | Connection 初始化 |
| 分页 | LIMIT + TOP 都支持 | 主要 LIMIT | QuerySqlGenerator |
| 标识符（MYSQL模式） | 不转换大小写 | 不转换 | 一致 |
| RETURNING | 支持 | MySQL 8.0.21+ 部分支持 | UpdateSqlGenerator 可利用 |
| DateTime.Date | `DATE(expr)` | `CONVERT(expr, date)` | DateTimeMemberTranslator |
| DateTime.Add* | `TIMESTAMPADD(unit, n, dt)` | `DATE_ADD(dt, INTERVAL n unit)` | DateTimeMethodTranslator |
| DateTime 部分提取 | `YEAR()`/`MONTH()` 等独立函数 | `EXTRACT(part FROM …)` | DateTimeMemberTranslator |
| Guid 生成 | `SYS_GUID()` | `UUID()` | NewGuidTranslator |
| 类型转换 | `CAST(expr AS type)` 标准 SQL | MySQL CAST 映射表 | QuerySqlGenerator |
| DateTimeOffset.LocalDateTime | **无 CONVERT_TZ** | `CONVERT_TZ(..., @@session.time_zone)` | 不翻译（客户端求值） |
| DateOnly.ToDateTime | `MAKE_TIMESTAMP(Y,M,D,h,m,s)` | `ADDTIME(CAST(date AS datetime), time)` | DateTimeMethodTranslator |
| TimeOnly.Add* | `ADDTIME(CAST(t AS TIME), INTERVAL n unit)` | `DATE_ADD` / `ADDTIME` | DateTimeMethodTranslator |
| EF.Functions.Degrees/Radians | `DEGREES()` / `RADIANS()` | 同左 | DbFunctionsExtensionsMethodTranslator |
| double.RadiansToDegrees | `DEGREES()` | 同左 | MathMethodTranslator |
| DateTimeOffset.Now | `SYSTIMESTAMP()` | `UTC_TIMESTAMP()` | DateTimeMemberTranslator |
| DateDiff (DbFunctions) | `TIMESTAMPDIFF(unit, …)` | 同左 | XuguDateDiffFunctionsTranslator |
| byte[] Contains | `LOCATE(sub, src) > 0` | 同左 | XuguByteArrayMethodTranslator |
| byte[] First | `ASCII(blob)` | 同左 | XuguByteArrayMethodTranslator |
| DbFunctions.Like | `LIKE … [ESCAPE …]` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| DbFunctions.Hex | `HEX(expr)` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| DbFunctions.Unhex | `UNHEX(expr)` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| object.ToString() | `CAST(expr AS VARCHAR)` | 同左 | XuguObjectToStringTranslator |
| Regex.IsMatch | `REGEXP_LIKE(expr, pattern)` | `expr REGEXP pattern` | XuguRegexIsMatchTranslator |
| ConvertTimeZone | **无 CONVERT_TZ** | `CONVERT_TZ(dt, from, to)` | **不实现**（defer） |
| FULLTEXT IsMatch | **无 MATCH AGAINST** | `MATCH … AGAINST` | **不实现** |
| HasTables | `DBA_TABLES`（`VALID='T'`, `IS_SYS='F'`） | `information_schema.tables` | XuguDatabaseCreator |

## DDL 差异

| 操作 | 文档路径 | 负责 Agent | 状态 |
|------|---------|-----------|------|
| CREATE TABLE | `reference/object/table/create.md` | Migrations | done |
| IDENTITY 列 | `reference/object/table/create.md#4-opt_serial` | Migrations | done |
| ALTER COLUMN | `reference/object/table/alter.md` | Migrations | done |
| CREATE INDEX | `reference/object/indexes.md` | Migrations | done |
| 迁移锁 | `reference/object/table/lock.md` | Migrations | done |
| Schema diff（ModelDiffer） | EF Core #25899 字符串 NOT NULL | Migrations | done |
| Scaffolding 元数据 | `reference/system-view/dba/dba_tables.md`, `reference/system-view/all/all_columns.md` | Migrations | partial |

## Scaffolding 元数据（DBA 视图）

> 文档：`reference/system-view/dba/dba_tables.md`, `reference/system-view/all/all_columns.md`

| 项 | XuguDB | MySQL/Pomelo | Provider |
|----|--------|-------------|----------|
| 表列表 | `DBA_TABLES`（`VALID='T'`, `IS_SYS='F'`） | `INFORMATION_SCHEMA.TABLES` | `XuguDatabaseModelFactory` |
| 列信息 | `DBA_COLUMNS`（`IS_SERIAL` → IDENTITY） | `INFORMATION_SCHEMA.COLUMNS` | 同上 |
| CHAR/VARCHAR | `VARYING` + `SCALE` | `DATA_TYPE` + `CHARACTER_MAXIMUM_LENGTH` | `BuildStoreType()` |
| NUMERIC 精度 | `SCALE/65536`, `SCALE%65536` | 直接列 | `BuildStoreType()` |
| 主键/索引/FK | `ALL_INDEXES` + `DBA_CONSTRAINTS` | `INFORMATION_SCHEMA` | `XuguDatabaseModelFactory` |
| 外键动作 | `DELETE_ACTION`/`UPDATE_ACTION` 单字符 (n/c/u/d/r) | RESTRICT/CASCADE 等字符串 | `MapReferentialAction()` |
| 索引类型 | `INDEX_TYPE` 0–3 (BTREE/RTREE/FULLTEXT/BITMAP) | FULLTEXT/SPATIAL 注解 | `XuguIndexType` Fluent API + Migration DDL |
| Collation/Charset | **不适用**（连接级 `CHAR_SET`） | 表/列级 HasCharSet | 不实现 Pomelo Collation |

## 变更日志

| 日期 | 变更 | 作者 |
|------|------|------|
| 2026-07-06 | 批次 C：NorthwindFunctions 组合测试；TimeOnly ADDTIME 实库修复；Degrees/Radians Translator | Orchestrator |
| 2026-07-06 | 批次 B：Unhex/ObjectToString Translator；NorthwindDbFunctions + DateOnly/TimeOnly 测试；TypeMapping NUMERIC/BINARY | Orchestrator |
| 2026-07-06 | 波次 7：DateDiff/ByteArray/DbFunctions.Like Translator；HasTables via DBA_TABLES | Orchestrator |
| 2026-07-06 | 波次 6：实库索引 create/rename/drop 验收；`ALL_INDEXES.VALID=1` 用于集成测试断言 | Orchestrator |
| 2026-07-06 | 波次 5：Git 追踪 + Index DDL + Scaffolding 集成测试 + CI 打包脚本 | Orchestrator |
| 2026-07-06 | Phase 4：IDENTITY(1,1) MigrationsSqlGenerator、LOCK TABLE 迁移锁、HistoryRepository | Agent-Migrations |
| 2026-07-06 | Phase 3 扩展：Convert/DateTimeOffset/TimeOnly/DateOnly Translator + AssertSql 测试 | Agent-QueryExtensions |
| 2026-07-06 | Phase 3：DateTime/TimestampAdd/CAST/SYS_GUID 函数映射；DateTimeQueryTests | Agent-Query |
| 2026-07-06 | Phase 3：LENGTH/ABS/CONCAT/LIKE 函数映射；Query DI 注册 | Agent-Query |
| 2026-07-06 | 初稿，基于 XuguDB 官方文档整理 | Orchestrator |
