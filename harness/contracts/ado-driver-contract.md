# ADO Driver Contract — XuguClient × EF Core Provider

> **状态**：v1.2（Phase 13.105–106，2026-07-19；生产风险标注 2026-07-20；DATETIME_TZ 缺口 2026-07-21）  
> **范围**：应用/审核关键读写路径上，驱动 `GetXxx` / 参数绑定 / 结果集行为与 Provider 处置  
> **原则**：无 silent gap；每行必须是 `ok` / `provider-workaround` / `vendor-ticket`  
> **交叉引用**：`sql-dialect.contract.md`；用户边界见 `docs/LIMITATIONS.md`；驱动缺陷详表见 `E:\Work\worksummary\xuguefcore-production-docs\XuguClient-驱动缺陷汇总-EF联调发现.md`（DRV-*）

## 生产依赖风险（必读）

下列 **provider-workaround** / **vendor-ticket** 行表示：**生产正确性依赖驱动当前行为或 Provider 旁路**。驱动升级（`GetXxx`、Message 格式、RETURNING、`RecordsAffected`）可能静默破坏；发版前须对照本表跑 RuntimeGap / AppMatrix / AffectedRows 探针。

| 风险等级 | 行 | 说明 |
|----------|-----|------|
| **高** | COUNT / TIMESTAMPDIFF CAST；RETURNING→LAST_INSERT_ID；RecordsAffected 并发 | 主路径 OLTP |
| **高** | Linux RID | 非 Windows 不可部署 |
| **中** | DateOnly/TimeOnly/DTO converters | 时态读写 |
| **高** | DATETIME_TZ `GetValue`/`GetString`/相等过滤（DRV-04/05/06） | 物化可旁路；**相等/`Contains` 不可靠** |
| **中** | 瞬态检测靠 Message 解析 | 无结构化 `IsTransient` |
| **低–中** | JSON 整列 LOB | 推荐函数投影 |

## 契约表

| 路径 | SQL / CLR | 驱动行为 | Provider 处置 | 状态 | 证据 |
|------|-----------|----------|---------------|------|------|
| COUNT → Int32 | `COUNT(*)` / `GetInt32` | 高精度聚合可能 `E34412` | `CAST(COUNT(…) AS INTEGER)` | **provider-workaround** | RuntimeGap / AppMatrix |
| LongCount → Int64 | `COUNT(*)` / `GetInt64` | 同上风险 | `CAST(COUNT(…) AS BIGINT)` | **provider-workaround** | RuntimeGap |
| GroupBy Count | 投影 `Count()` | 同上 | 同上 CAST | **provider-workaround** | RuntimeGap / AppMatrix |
| 导航 Count | `SELECT COUNT` 子查询 | 同上 | 同上 CAST | **provider-workaround** | RuntimeGap / AppMatrix |
| TIMESTAMPDIFF → int | `DateDiff*` | 官方返回 BIGINT；`GetInt32` 可能失败 | 按 BIGINT 生成再转 `INTEGER` | **provider-workaround** | RuntimeGap A4 |
| DATE 读 | `DateOnly` 物化 | 原生绑定不完整 | string converter | **provider-workaround** | RuntimeGap C1 |
| TIME 读 | `TimeOnly` 物化 | 同上；默认 `TIME(3)` | string converter | **provider-workaround** | RuntimeGap C1 |
| DATE Include | Include 含 DATE 列 | 同上 | 同上 | **provider-workaround** | AppMatrix |
| DateTimeOffset 写 | SaveChanges 参数 | `XGDbType` 无 DATETIME_TZ；`DbType.DateTimeOffset`→Binary | string converter + `AnsiString` | **provider-workaround** | RuntimeGap B1；DRV-06 |
| DateTimeOffset 读（物化） | `GetValue` / `GetString` | `GetValue` 对 `XG_C_DATETIME_TZ`(14) 抛「未支持的列转化类型」；`GetString` 截断亚秒与亚小时偏移 | `GetString` + converter；测试 `GetExpectedValue` | **provider-workaround** | RuntimeGap B1；DRV-04/05 |
| DateTimeOffset 相等/`Contains` | `Timeline = @p` / `Contains` | 仅「写入完整字符串」ADO 可命中；截断往返 / `TIMESTAMP` 字面量常 0 行 | **无可靠旁路** → 功能测试 Skip | **vendor-ticket** | GearsOfWar Skip；production-docs DRV-06 |
| JSON 标量路径 | `JSON_VALUE` / `JsonValue` | 标量投影可用 | Translator + 物化测 | **ok**（标量） | JsonIntegrationTests |
| JSON 整列物化 | 实体 JSON 列 → string/doc | LOB/`GetString` 边界因驱动版本而异 | 推荐 `JsonValue` 投影；整列见 LIMITATIONS | **provider-workaround** | 13.206 |
| INSERT…RETURNING | identity 回读 | `FieldCount=0`，无 NextResult | `INSERT` + `SELECT … LAST_INSERT_ID()` | **vendor-ticket** / **provider-workaround** | UpdateSqlGenerator；probe-returning |
| ROW_COUNT() | 乐观并发 rows-affected | **E10049** 即使 MYSQL compat | **不依赖**：`RecordsAffected` Path A（UPDATE/DELETE）；INSERT reader 恒 0 → Added 特例 | **provider-workaround** | AffectedRowsProbeTests；OptimisticConcurrencyTests；13.201(A) |
| 参数绑定（标量） | int/string/decimal/datetime | 常规路径可用 | TypeMapping | **ok** | BuiltInDataTypes |
| 未映射 DbType | `UInt32`/`UInt16`/`UInt64`/`Single`/`Guid`/`SByte`/`StringFixedLength` | `DbType_to_XuguDbType` default→Binary，强转 `byte[]` | `ConfigureParameter` 旁路 + unsigned/float/sbyte signed converter 物化；Guid→`XGDbType.Guid` | **provider-workaround** | TypeMappingSourceTests；Northwind BulkUpdates seed |
| Linux `libxugusql.so` | RID linux-x64 | 无预编译 native | Windows-only GA；pack 条件打包 | **vendor-ticket** | VT-XUGU-LINUXRID-001 |

## 缺口登记（无 silent）

| ID | 项 | 处置 | Ticket / 文档 |
|----|-----|------|---------------|
| G-01 | COUNT/TIMESTAMPDIFF GetInt32 | provider-workaround（保留） | LIMITATIONS §COUNT |
| G-02 | 时态 DateOnly/TimeOnly/DTO **物化/写入** | provider-workaround（保留） | LIMITATIONS §DateOnly / DateTimeOffset |
| G-02b | DATETIME_TZ 相等/`Contains` 过滤 | **vendor-ticket**；测试 Skip（DRV-06） | production-docs 驱动缺陷汇总 §10；LIMITATIONS §DateTimeOffset |
| G-03 | RETURNING DataReader | 保持 LAST_INSERT_ID；probe 脚本跟踪 | 13.203–204 |
| G-04 | ROW_COUNT 乐观并发 | 产品声明见 13.201 决策 | V-01 |
| G-05 | Linux RID | signed-off 续期 | V-02 |
| G-06 | JSON 整列 LOB | LIMITATIONS + JsonValue 推荐 | 13.206 |
| G-07 | 未映射 DbType→Binary | provider-workaround（保留） | TypeMappingSourceTests |

## 变更日志

| 日期 | 变更 |
|------|------|
| 2026-07-21 | v1.2：拆分 DateTimeOffset 读/写/相等过滤；登记 DRV-04/05/06 / G-02b；交叉引用 production-docs |
| 2026-07-21 | 未映射 DbType（unsigned/float/Guid/SByte/StringFixedLength）→ Binary：Provider 旁路绑定/物化 |
| 2026-07-20 | v1.1：增加「生产依赖风险」表；ROW_COUNT 行对齐 Path A |
| 2026-07-19 | v1 初版（Phase 13.105–106） |
