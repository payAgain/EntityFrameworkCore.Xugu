# XuguDB SQL 方言契约（Living Document）

> **SQL 唯一权威**：`E:\BaiduSyncdisk\docs\content\`（XuguDB 官方文档）  
> 所有 Agent 实现 SQL 相关代码前必须阅读本文档 + 对应官方文档。  
> 发现新差异时更新本文档并注明文档路径。

> **⚠️ 非权威来源（禁止作为 SQL 依据）**  
> - Pomelo / MySQL 语法与行为 — **仅** C# 架构、DI、Translator 模式参考  
> - `COMPATIBLE_MODE=MYSQL` 下的偶然兼容 — **不是**产品语义（见下文）  
> - 本文「与 MySQL/Pomelo 差异」列 — 帮助对照，**不**定义 Xugu 应实现的 SQL

## 参考源优先级

```
1. E:\BaiduSyncdisk\docs\content\              ← SQL 方言、类型、函数（唯一权威）
2. harness/contracts/sql-dialect.contract.md   ← 项目内已登记规则
3. harness/contracts/stub-and-exclusion.contract.md ← 无文档能力时的 stub/skip 策略
4. docs/LIMITATIONS.md / RELEASE-SCOPE.md      ← 产品范围
5. external/Pomelo.EntityFrameworkCore.MySql     ← C# 架构参考（MySqlJson* 等命名可借鉴，SQL 不可照搬）
```

## 数据库信息

| 项 | 值 |
|----|-----|
| 数据库 | XuguDB（虚谷数据库） |
| EF Core 包名 | `Microsoft.EntityFrameworkCore.Xugu` |
| 连接 API | `UseXugu(connectionString)` |
| 连接串示例 | `IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8` |
| ADO.NET 驱动 | `XuguClient.dll` + 原生 `xugusql.dll`（见 `external/csharp-driver/`） |
| 兼容模式 | `SET compatible_mode TO 'MYSQL';`（**可选开发/对照便利**；产品 SQL 以 Xugu 原生文档为准，见 `docs/RELEASE-SCOPE.md`） |
| 文档根目录 | `E:\BaiduSyncdisk\docs\content\` |

## 兼容模式行为

> 文档：`reference/system-configuration-parameter/session-parameter/compatible_mode.md`

**产品定位**：`COMPATIBLE_MODE=MYSQL` 是 **可选** 会话参数，用于开发对照与遗留脚本调试；**不是** Provider 的产品目标。**2.1.0 起默认关闭**（连接打开时不执行 `SET compatible_mode`）；显式 `EnableCompatibleModeOnOpen()` 启用。

| COMPATIBLE_MODE | 标识符处理 | Provider 默认（2.1.0+） |
|-----------------|-----------|-------------------------|
| NONE / ORACLE | 词法阶段转大写 | **默认（compat off）** |
| **MYSQL** | **不做大小写转换** | opt-in via `EnableCompatibleModeOnOpen()` |
| POSTGRESQL | 词法阶段转小写 | opt-in |

Provider 生成的 SQL 与验收标准仍以 **Xugu 官方文档** 为准，不以「零改动 MySQL 迁移」为设计目标。

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

## ExecuteDelete / ExecuteUpdate（批量 DML）

> 文档：`reference/sql/dml/delete.md`、`reference/sql/dml/update.md`

| 场景 | 生成 SQL 形态 | 文档依据 | Provider |
|------|--------------|---------|----------|
| 单表带谓词 DELETE | `DELETE FROM {table} WHERE …` | delete.md §语法 | `XuguQuerySqlGenerator.VisitDelete` |
| 单表 DELETE + ORDER BY / LIMIT | `DELETE FROM {table} WHERE … ORDER BY … LIMIT …`（无表别名） | delete.md + resultset-restricted.md | 同上（`_removeTableAlias`） |
| 多表 DELETE（JOIN） | `DELETE FROM {target} FROM {joins…} WHERE …` | delete.md §`opt_from_clause` 示例 | `IsValidSelectExpressionForExecuteDelete` + `VisitDelete` |
| 单表 UPDATE | `UPDATE {table} SET col = val WHERE …` | update.md §`update_filter_clause` | `XuguQuerySqlGenerator.VisitUpdate` |
| 多表 UPDATE（JOIN） | `UPDATE {target}, {joins…} SET … WHERE …` | update.md §`base_table_refs` 多表 | 同上 |
| UPDATE + LIMIT | 单表支持 `LIMIT`；**多表不支持** ORDER BY / LIMIT | update.md §提示 4 | `GenerateLimitOffset` |

**与 MySQL/Pomelo 差异**：Xugu 单表 DELETE 使用 `DELETE FROM table`（非 MySQL `DELETE alias FROM table`）；多表使用双 `FROM` 子句（`DELETE FROM t1 FROM t2`），非 MySQL 逗号 JOIN 列表。多表 UPDATE **不支持** LIMIT（`update.md` §提示 4）。

## HAVING / 布尔优化

> 文档：`reference/sql/select/group-by.md`

| 场景 | XuguDB | Provider |
|------|--------|----------|
| HAVING 聚合函数 | `HAVING COUNT(*) > N` | 标准翻译 |
| HAVING SELECT 别名 | `HAVING emp_count > 2`（文档示例） | `XuguHavingExpressionVisitor` 子查询 pushdown + `XuguColumnAliasReferenceExpression` |
| 布尔列谓词优化 | `WHERE col = TRUE` 利于索引 | `XuguBoolOptimizingExpressionVisitor`（`XuguParameterBasedSqlProcessor`） |

**与 MySQL 差异**：Xugu 文档明确支持 HAVING 引用 SELECT 别名；Pomelo 的 MySQL bug #103961 workaround 仍保留 pushdown 路径以兼容 EF Core 复杂 HAVING 表达式。

## Sequential GUID

> 文档：`reference/sql/datatype/guid.md`、`reference/function/uuid-functions/sys_guid.md`

| 场景 | XuguDB | Provider |
|------|--------|----------|
| 查询时 NewGuid | `SYS_GUID()` | `XuguNewGuidTranslator` |
| 插入时 Guid PK | 客户端顺序 GUID（ticks + random） | `XuguSequentialGuidValueGenerator` + `XuguValueGeneratorSelector` |

**与 MySQL/Pomelo 差异**：无 `GuidFormat` 连接选项；统一 RFC4122 大端序客户端生成。

## Migrations Differ 过滤

| 场景 | 行为 |
|------|------|
| 过滤索引 `HasFilter` | Differ 剥离 `Filter`（DDL 不支持，见 `FilteredIndexesNotSupported`） |
| 列 Collation 变更 | 忽略（连接级 `CHAR_SET`） |

## 外键 ReferentialAction

> 文档：`reference/object/constraints.md` §key_actions

| ReferentialAction | XuguDB DDL | Provider |
|-------------------|-----------|----------|
| Cascade | `ON DELETE/UPDATE CASCADE` | `XuguMigrationsSqlGenerator.ForeignKeyAction` |
| SetNull | `SET NULL` | 同上 |
| SetDefault | `SET DEFAULT` | 同上 |
| Restrict | `RESTRICT` | 同上 |
| NoAction | 省略子句（默认 NO ACTION） | 同上 |

**与 MySQL/Pomelo**：EF Core 关系模型仅映射 `DeleteBehavior` → `OnDelete`；`OnUpdate` 由迁移操作显式指定（DDL 支持 `ON UPDATE`）。

## 字符集 Fluent API

| MySQL/Pomelo | XuguDB | Provider |
|--------------|--------|----------|
| `ModelBuilder.HasCharSet` | 无表/模型级 charset | **skip** — 使用连接串 `CHAR_SET` |
| `EntityTypeBuilder.HasCharSet` | 无 | **skip** |
| `XuguTableBuilderExtensions.HasXuguComment` | `COMMENT ON TABLE` | **done** Wave 5 |

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

**参数内联（10.201）**：`Skip` 生成的 `OFFSET` 子句在参数值已知时内联为字面量（`XuguParameterInliningExpressionVisitor`）；JSON 动态路径使用 `CONCAT`/`JSON_EXTRACT`（11.109b）。

## JSON 列（Phase 11 — 11.109 done）

> **权威文档**：  
> - 类型：`reference/sql/datatype/json.md`  
> - 运算符：`reference/sql/operators/json-operators/column_path.md`（`->`）、`inline_path.md`（`->>`）  
> - 函数：`reference/function/json-functions/`（28+ 函数）

| 项 | XuguDB（官方文档） | Provider 2.0.x | Provider 2.1.0 目标（11.109） |
|----|-------------------|----------------|------------------------------|
| 原生 `JSON` 列类型 | **支持**（LOB，最大 2GB；Java `String` 绑定） | **未映射** | **`XuguJsonTypeMapping` + DDL `JSON`（11.109a done）** |
| `->` / `->>` 路径运算符 | **支持**（JSONPath，`$` 前缀；含 `last`、`**`、`[M to N]` 扩展） | 未翻译 | **`XuguJsonTraversalExpression` + `VisitJsonScalar`（11.109b done）** |
| `JSON_EXTRACT` / `JSON_VALUE` 等 | **28+ 函数**（见 `json.md` §预览表） | 未实现 | **`XuguJsonDbFunctionsExtensions` + Translator（11.109b done）** |
| `JSON_ARRAYAGG` / `JSON_OBJECTAGG` | 支持 | 未实现 | P2 / 按需 |
| EF `ToJson()` / owned JSON 列 | — | **不实现** | **不承诺** Pomelo 全矩阵；基础 JSON 列映射优先 |
| Pomelo `Json*MySqlTest` | — | **skip**（2.0.x） | 手写 Xugu 子集（11.109d done） |
| Fluent `HasXuguJsonColumn()` | — | — | **done**（11.109c） |

**与 MySQL/Pomelo 差异（实现时以 Xugu 文档为准，非 MySQL 字节级兼容）**：

- XuguDB JSON 比较/排序有独立类型优先级（BOOL > ARRAY > OBJECT > STRING > NUMBER > NULL；见 `json.md` §JSON比较与排序）。
- 路径语法支持 `last`、`**` 深度查找、`[M to N]` 切片等 Xugu 扩展（见 `json.md` §JSONPath）。
- `->` 返回 JSON 文本；`->>` 取消 JSON 类型引用（见 `column_path.md` / `inline_path.md`）。
- ADO.NET 驱动映射为 `java.sql.String`（`json.md` §特性表）；EF 需确认 `XuguClient` 参数绑定与反序列化策略。

### 11.109 实现脚手架（Wave 2 入口）

| 子任务 | Provider 模块 | Pomelo 架构参考（仅 C#） | Xugu 文档锚点 |
|--------|--------------|-------------------------|--------------|
| 11.109a | `Storage/Internal/XuguJsonTypeMapping.cs` | `MySqlJsonTypeMapping` | `json.md` §JSON存储类型、DDL 示例 | **done** |
| 11.109b | Query Translators（`JsonScalarExpression` 遍历） | `MySqlJson*` translators | `json-operators/`、`json-functions/json_extract.md` | **done** |
| 11.109c | Fluent API（`HasXuguJsonColumn`） | `MySqlEntityTypeBuilderExtensions` | 以 Xugu 文档为准 | **done** |
| 11.109d | 实库测试 | `JsonQueryMySqlTest` 可跑子集 | 手写断言 | **done** |

**2.1.0 状态**：Wave 2 done（875 列测；`JsonIntegrationTests` SkippableFact）。

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
- **SaveChanges 回读（2.1.0）**：
  - **默认与 compat 运行时 SQL**：`INSERT` + `SELECT … WHERE {identity_col} = LAST_INSERT_ID()`（Xugu 原生函数，见 `last_insert_id.md`）
  - **差异**：compat 模式连接打开时额外 `SET compatible_mode TO 'MYSQL'`
  - **RETURNING**：数据库与 `insert.md` 支持；**XuguClient ADO 暂不可读** — Provider 不使用 `AppendInsertReturningOperation` 直至驱动修复（11.506）
- 与 Pomelo 差异：**必须在 MigrationsSqlGenerator 和 Convention 中单独实现**
- **ROW_COUNT**：仍 **blocked**（10.105 / E10049）；RETURNING 路径 **不** 依赖 `ROW_COUNT()`

## 数据类型映射（CLR → XuguDB）

> 文档：`reference/sql/datatype/`

| CLR 类型 | XuguDB 类型 | 文档 |
|----------|------------|------|
| `bool` | `BOOLEAN` | `datatype/bool.md` |
| `byte` | `TINYINT` | `datatype/numerical.md` |
| `short` | `SMALLINT` | `datatype/numerical.md` |
| `int` | `INTEGER` | `datatype/numerical.md` |
| `long` | `BIGINT` | `datatype/numerical.md` |
| `uint` | `BIGINT`（无 unsigned 类型） | `datatype/numerical.md` |
| `ulong` | `NUMERIC(20,0)`（无 unsigned 类型） | `datatype/numerical.md` |
| `decimal` | `NUMERIC/DECIMAL/NUMBER` | `datatype/numerical.md` |
| `float` | `FLOAT` | `datatype/numerical.md` |
| `double` | `DOUBLE` | `datatype/numerical.md` |
| `string` | `VARCHAR(n)` / `CHAR(n)` | `datatype/character.md` |
| `DateTime` | `DATETIME` | `datatype/datetime.md` |
| `DateOnly` | `DATE` | `datatype/datetime.md` |
| `TimeOnly` | `TIME` | `datatype/datetime.md` |
| `DateTimeOffset` | `DATETIME WITH TIME ZONE` / `TIMESTAMP WITH TIME ZONE` | `datatype/datetime.md` |
| `Guid` | `GUID`（16 字节原生类型） | `datatype/guid.md` |
| `TimeSpan` | `TIME` | `datatype/datetime.md` |
| `byte[]` | 见二进制文档 | `datatype/binary.md` |

## INSERT 语句

> 文档：`reference/sql/dml/insert.md`

支持：

- 标准 INSERT（单行/多行）
- REPLACE INTO
- INSERT IGNORE INTO
- RETURNING 子句（XuguDB 特有，见 insert.md）

**Provider 实现（2.1.0）**：

| 模式 | INSERT identity 回读 SQL（2.1.0 运行时） |
|------|----------------------------------------|
| Native（默认） | `INSERT …`; `SELECT id FROM t WHERE id = LAST_INSERT_ID()` |
| Compat（opt-in） | 同上 + 连接级 `SET compatible_mode TO 'MYSQL'` |

**理想路径（驱动修复后）**：`INSERT INTO t (…) VALUES (…) RETURNING id`（`insert.md`）。当前 XuguClient 不暴露 RETURNING 结果集。

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
| `string.Equals(s, StringComparison)` | `LCASE(a)=LCASE(b)`（IgnoreCase）/ 直接 `=` | COLLATE utf8mb4_bin | QueryTranslators | done |
| `string.Trim/TrimStart/TrimEnd` | `TRIM([LEADING\|TRAILING] … FROM …)` | 同左 | QueryTranslators | done |
| `string.Replace` | `REPLACE()` | 同左 | QueryTranslators | done |
| `string.ToLower/ToUpper` | `LCASE()` / `UCASE()` | `LOWER()` / `UPPER()` | QueryTranslators | done |
| `string.PadLeft/PadRight` | `LPAD()` / `RPAD()`（常量参数） | 同左 | QueryTranslators | done |
| `string.IndexOf` | `LOCATE(sub, str) - 1` | 同左 | QueryTranslators | done |
| `string.Substring` | `SUBSTRING(str, start+1, len)` | 同左 | QueryTranslators | done |
| `Math.Floor/Ceiling/Round/Truncate` | `FLOOR/CEILING/ROUND/TRUNCATE` | 同左 | QueryTranslators | done |
| `Math.Sin/Cos/Tan/Sqrt/Pow/Exp` | `SIN/COS/TAN/SQRT/POWER/EXP` | 同左 | QueryTranslators | done |
| `Math.Log(x)` | `LN(x)` | `LOG(x)` | QueryTranslators | done |
| `Math.Log(x, base)` | `LOG(base, x)`（参数反转，见 log.md） | `LOG(x, base)` | QueryTranslators | done |
| `TimeSpan.Hours/Minutes/Seconds/Milliseconds` | `HOUR/MINUTE/SECOND/MICROSECOND` | `EXTRACT(part FROM …)` | QueryTranslators | done |
| `string.Split` | — | — | — | **defer**（无简单 LINQ→SQL 映射；`SPLIT_PART` 仅常量） |

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
| byte[] Length | `LENGTH(blob)` | 同左 | XuguSqlTranslatingExpressionVisitor |
| byte[] indexer / ElementAt | `ASCII(SUBSTRING(blob, index+1, 1))` | 同左 | XuguSqlTranslatingExpressionVisitor |
| Math.Max / Math.Min | `GREATEST(…)` / `LEAST(…)` | 同左 | XuguSqlTranslatingExpressionVisitor |
| TimeOnly subtract | `subtract(left, right)` → TIME | 同左 | XuguSqlTranslatingExpressionVisitor |
| DbFunctions.Like | `LIKE … [ESCAPE …]` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| DbFunctions.Hex | `HEX(expr)` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| DbFunctions.Unhex | `UNHEX(expr)` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| object.ToString() | `CAST(expr AS VARCHAR)` | 同左 | XuguObjectToStringTranslator |
| Regex.IsMatch | `REGEXP_LIKE(expr, pattern)` | `expr REGEXP pattern` | XuguRegexIsMatchTranslator |
| StringComparison.Equals | `LCASE` 双端（IgnoreCase） | `COLLATE utf8mb4_bin` | XuguStringComparisonMethodTranslator |
| Math.Log (1-arg) | `LN()` | `LOG()` | XuguMathMethodTranslator |
| Math.Log (2-arg) | `LOG(base, value)` 参数序与 CLR 相反 | `LOG(value, base)` | XuguMathMethodTranslator |
| TimeSpan members | `HOUR()` 等独立函数 | `EXTRACT(hour FROM …)` | XuguTimeSpanMemberTranslator |
| MySQL YEAR 类型 | **无 YEAR 列类型** | `YEAR` | **skip** |
| ConvertTimeZone | **无 CONVERT_TZ** | `CONVERT_TZ(dt, from, to)` | **不实现**（defer） |
| FULLTEXT IsMatch | **无 MATCH AGAINST** | `MATCH … AGAINST` | **不实现** |
| HasTables | `DBA_TABLES`（`VALID='T'`, `IS_SYS='F'`） | `information_schema.tables` | XuguDatabaseCreator |

## FOR UPDATE / 位运算（Phase 8 调研）

> 文档：`reference/sql/select/select.md` §FOR UPDATE；`reference/sql/datatype/bit.md`；`reference/sql/operators/bit-operators/`

| 场景 | XuguDB | Provider | 状态 |
|------|--------|----------|------|
| `SELECT … FOR UPDATE` | 支持行排他锁 | — | **defer**（8.Q12；EF Core 无标准 Tag 翻译入口） |
| 窗口函数 | 文档子集支持 | — | **defer**（8.Q12） |
| 整数位运算 `& \| ^ << >>` | BIGINT 返回；BIT 类型独立 | — | **defer**（8.Q11；暂无翻译类型不匹配报告） |
| `BitwiseOperationReturnTypeCorrecting` | Pomelo 用于 MySQL 返回类型修正 | — | **defer** |

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
| 列重命名 | **无 RENAME COLUMN**；`ADD + UPDATE + DROP`  workaround | MySQL 8 `RENAME COLUMN` / `CHANGE` | `XuguMigrationsSqlGenerator` |
| 表/列备注 | `COMMENT ON TABLE/COLUMN … IS …`；CREATE 内联 `COMMENT '…'` | MySQL `COMMENT=` | `XuguMigrationsSqlGenerator` |
| Identity PK 类型变更 | **不支持自动 ALTER**；需手工重建表 | Pomelo DropPrimaryKey+recreate | throws `NotSupportedException` |
| 索引前缀长度 | **无 INDEX(col(N)) 语法** | MySQL `HasPrefixLength` | 注解存储 only（8.E2） |
| 视图 Scaffolding | `ALL_VIEWS` + `ALL_VIEW_COLUMNS` | `INFORMATION_SCHEMA.VIEWS` | `XuguDatabaseModelFactory` |
| Convert 扩展 | `CAST(expr AS type)` | 同 | `XuguConvertTranslator` (+ unsigned/float) |
| SqlTranslatingVisitor | GREATEST/LEAST、byte[]、TimeOnly、string[] Concat/Join | Pomelo 对齐（无 JSON） | `XuguSqlTranslatingExpressionVisitor` |

## Scaffolding 元数据（DBA 视图）

> 文档：`reference/system-view/dba/dba_tables.md`, `reference/system-view/all/all_columns.md`

| 项 | XuguDB | MySQL/Pomelo | Provider |
|----|--------|-------------|----------|
| 表列表 | `DBA_TABLES`（`VALID='T'`, `IS_SYS='F'`） | `INFORMATION_SCHEMA.TABLES` | `XuguDatabaseModelFactory` |
| 列信息 | `DBA_COLUMNS`（`IS_SERIAL` → IDENTITY） | `INFORMATION_SCHEMA.COLUMNS` | 同上 |
| CHAR/VARCHAR | `VARYING` + `SCALE` | `DATA_TYPE` + `CHARACTER_MAXIMUM_LENGTH` | `BuildStoreType()` |
| NUMERIC 精度 | `SCALE/65536`, `SCALE%65536` | 直接列 | `BuildStoreType()` |
| 主键/索引/FK | `ALL_INDEXES` + `DBA_CONSTRAINTS` | `INFORMATION_SCHEMA` | `XuguDatabaseModelFactory` |
| 视图 | `ALL_VIEWS` + `ALL_VIEW_COLUMNS` | `INFORMATION_SCHEMA.VIEWS` | `XuguDatabaseModelFactory`（`DatabaseView`） |
| 外键动作 | `DELETE_ACTION`/`UPDATE_ACTION` 单字符 (n/c/u/d/r) | RESTRICT/CASCADE 等字符串 | `MapReferentialAction()` |
| 索引类型 | `INDEX_TYPE` 0–3 (BTREE/RTREE/FULLTEXT/BITMAP) | FULLTEXT/SPATIAL 注解 | `XuguIndexType` Fluent API + Migration DDL |
| Collation/Charset | **不适用**（连接级 `CHAR_SET`） | 表/列级 HasCharSet | 不实现 Pomelo Collation |

## 变更日志

| 日期 | 变更 | 作者 |
|------|------|------|
| 2026-07-06 | Phase 8 W4：FOR UPDATE/位运算 defer 登记；Translator/TypeMapping/Migration/Scaffolding 测试扩展 | Orchestrator |
| 2026-07-06 | Phase 8 W3：Having/BoolOptimizing/Postprocessor visitors；ExecuteUpdate 多表 LIMIT 守卫；SequentialGuid；MigrationsModelDiffer 索引/Collation 过滤 | Orchestrator |
| 2026-07-06 | Phase 8 W2：SqlTranslating/Convert 扩展；Migration 列重命名/备注；视图 Scaffolding；Extensions E1–E3 | Orchestrator |
| 2026-07-06 | Phase 8 W1：StringComparison/Math/TimeSpan Translators；专用 TypeMapping 注册表 | Orchestrator |
| 2026-07-06 | Phase 7 W1：TypeMapping 专用类（GUID/BOOL/TIME/uint/ulong）；Retry defer 文档化 | Storage |
| 2026-07-06 | 批次 B：Unhex/ObjectToString Translator；NorthwindDbFunctions + DateOnly/TimeOnly 测试；TypeMapping NUMERIC/BINARY | Orchestrator |
| 2026-07-06 | 波次 7：DateDiff/ByteArray/DbFunctions.Like Translator；HasTables via DBA_TABLES | Orchestrator |
| 2026-07-06 | 波次 6：实库索引 create/rename/drop 验收；`ALL_INDEXES.VALID=1` 用于集成测试断言 | Orchestrator |
| 2026-07-06 | 波次 5：Git 追踪 + Index DDL + Scaffolding 集成测试 + CI 打包脚本 | Orchestrator |
| 2026-07-06 | Phase 4：IDENTITY(1,1) MigrationsSqlGenerator、LOCK TABLE 迁移锁、HistoryRepository | Agent-Migrations |
| 2026-07-06 | Phase 3 扩展：Convert/DateTimeOffset/TimeOnly/DateOnly Translator + AssertSql 测试 | Agent-QueryExtensions |
| 2026-07-06 | Phase 3：DateTime/TimestampAdd/CAST/SYS_GUID 函数映射；DateTimeQueryTests | Agent-Query |
| 2026-07-06 | Phase 3：LENGTH/ABS/CONCAT/LIKE 函数映射；Query DI 注册 | Agent-Query |
| 2026-07-06 | 初稿，基于 XuguDB 官方文档整理 | Orchestrator |
| 2026-07-07 | 来源血缘校验脚本 `harness/scripts/verify-source-lineage.ps1`（禁止 AUTO_INCREMENT/INFORMATION_SCHEMA 等） | Orchestrator |
| 2026-07-07 | Phase 9 M3 关闭：`XuguTestStore` 全量 adoption、Northwind seed、`XuguQueryTestBase`、`AssertSql` 基线、20+ Collection fixtures；676 列测；2.0.0 发版 | Testing / Orchestrator |
| 2026-07-07 | Phase 10 Wave 1：CI 实库矩阵（GitHub + GitLab）+ `verify.ps1 -RunTests` 全量门禁；`docs/GETTING-STARTED.md` → 2.0.0；`docs/XUGU-VS-MYSQL.md` 用户对比文档；`harness/references/phase-10-test-triage.md` 剩余 ~374 测试 triage | Infra / Docs / Testing |
| 2026-07-07 | Phase 10 Wave 2：Query 深覆盖 +119（FromSql / TPH / Deep nested / DbFunctions / ComplexNav）对齐 Pomelo `NorthwindQueryMySqlTest` + `AdHocQueryMySqlTest` 子集；9.T defer 补全（SaveChangesInterception +6 / ConvertToProviderTypes +10 / Seeding +3 / WithConstructors insert ×2）；795 列测；10.M2 ✅ | Testing |
| 2026-07-08 | Phase 10 Wave 3：`MonsterFixupXuguTests` + `StoreGeneratedFixupXuguTests`（手写 Xugu 兼容模型，对齐 Pomelo `MonsterFixup*MySqlTest`）；`DesignTimeXuguTest` + `KeysWithConverters` + `TransactionBasics` 子集（对齐 `EFCore.Specification.Tests` 数据库相关）；850 列测；10.M4 ✅；~81% Pomelo 覆盖 | Testing |
| 2026-07-08 | Phase 10 Wave 5：OFFSET 参数内联（`XuguInlinedParameterExpression`）；Linux RID blocked 登记 | QueryCore |
| 2026-07-08 | Phase 10 Wave 4：`XuguRetryingExecutionStrategy` + `XuguTransientExceptionDetector`（10.106 ✅）；10.105 ROW_COUNT **blocked**（实库 E10049：`ROW_COUNT()` 不存在）；860 列测 | Storage / Testing |
| 2026-07-08 | defer 登记：10.105 ROW_COUNT 乐观并发（E10049 blocked）、10.107 EF 版本矩阵、10.108 JSON 列调研 | Orchestrator |
| 2026-07-08 | Phase 11 W1：方言权威声明强化；JSON § 扩展为 11.109 实现脚手架；COMPATIBLE_MODE 标注为可选开发便利 | Docs |
| 2026-07-08 | Phase 10 Wave 6（10.108）：JSON 原生类型 + 函数已确认；Provider defer 10.109 → Phase 11 | Orchestrator |
