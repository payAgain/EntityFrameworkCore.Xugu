# ADO Driver Contract — XuguClient × EF Core Provider

> **状态**：v1（Phase 13.105–106，2026-07-19）  
> **范围**：应用/审核关键读写路径上，驱动 `GetXxx` / 参数绑定 / 结果集行为与 Provider 处置  
> **原则**：无 silent gap；每行必须是 `ok` / `provider-workaround` / `vendor-ticket`  
> **交叉引用**：`sql-dialect.contract.md`；用户边界见 `docs/LIMITATIONS.md`

## 状态枚举

| 状态 | 含义 |
|------|------|
| **ok** | 驱动原生路径可用，Provider 无特殊适配 |
| **provider-workaround** | 驱动有缺口或易踩坑；Provider 已 CAST/Converter/改写 SQL |
| **vendor-ticket** | 需驱动/服务端修复；Provider 已文档化或 Skip |

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
| DateTimeOffset 写 | SaveChanges 参数 | 无完整原生绑定；偏移易丢 | string converter + 带偏移字面量 | **provider-workaround** | RuntimeGap B1 |
| DateTimeOffset 读 | `…+8` / `…+08:00` | 驱动常见单数字小时 | 读回兼容两种格式 | **provider-workaround** | RuntimeGap B1 |
| JSON 标量路径 | `JSON_VALUE` / `JsonValue` | 标量投影可用 | Translator + 物化测 | **ok**（标量） | JsonIntegrationTests |
| JSON 整列物化 | 实体 JSON 列 → string/doc | LOB/`GetString` 边界因驱动版本而异 | 推荐 `JsonValue` 投影；整列见 LIMITATIONS | **provider-workaround** | 13.206 |
| INSERT…RETURNING | identity 回读 | `FieldCount=0`，无 NextResult | `INSERT` + `SELECT … LAST_INSERT_ID()` | **vendor-ticket** / **provider-workaround** | UpdateSqlGenerator；probe-returning |
| ROW_COUNT() | 乐观并发 rows-affected | **E10049** 即使 MYSQL compat | batch 用 `SELECT 1` 占位；并发异常 Skip | **vendor-ticket** | VT-XUGU-ROWCOUNT-001；PLAT-01 |
| 参数绑定（标量） | int/string/decimal/datetime | 常规路径可用 | TypeMapping | **ok** | BuiltInDataTypes |
| Linux `libxugusql.so` | RID linux-x64 | 无预编译 native | Windows-only GA；pack 条件打包 | **vendor-ticket** | VT-XUGU-LINUXRID-001 |

## 缺口登记（无 silent）

| ID | 项 | 处置 | Ticket / 文档 |
|----|-----|------|---------------|
| G-01 | COUNT/TIMESTAMPDIFF GetInt32 | provider-workaround（保留） | LIMITATIONS §COUNT |
| G-02 | 时态 DateOnly/TimeOnly/DTO | provider-workaround（保留） | LIMITATIONS §DateOnly / DateTimeOffset |
| G-03 | RETURNING DataReader | 保持 LAST_INSERT_ID；probe 脚本跟踪 | 13.203–204 |
| G-04 | ROW_COUNT 乐观并发 | 产品声明见 13.201 决策 | V-01 |
| G-05 | Linux RID | signed-off 续期 | V-02 |
| G-06 | JSON 整列 LOB | LIMITATIONS + JsonValue 推荐 | 13.206 |

## 变更日志

| 日期 | 变更 |
|------|------|
| 2026-07-19 | v1 初版（Phase 13.105–106） |
