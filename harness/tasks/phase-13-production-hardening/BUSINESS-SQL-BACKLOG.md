# Phase 13 — 业务 SQL / 函数频度清单

> **状态**：**frozen**（13.301–302，2026-07-19）  
> **规则**：每项必须有 Xugu 官方文档路径；处置仅允许 `implement` / `defer` / `excluded`  
> **验收**：Translator + AssertSql + **客户端物化**实库测（见 `docs/TESTING.md` 三类绿）  
> **文档根**：`E:\Work\docs\content\`（或等价）

## 冻结表

| ID | 能力 / 表达式 | 频度 | 文档路径 | 处置 | 状态 | 备注 |
|----|---------------|------|----------|------|------|------|
| B01 | 行锁 `FOR UPDATE` | P1 | `reference/sql/select.md`（锁子句） | **excluded** | **done** | EF 无标准 API；12.301 OOS；LIMITATIONS |
| B02 | 窗口函数 RANK/ROW_NUMBER | P1 | `reference/function/analytic-functions/` | **defer** | **done** | 无应用入口；EF 原始 SQL 可用 |
| B03 | 字符串 Trim/长度/子串 | P0 | `reference/function/string-functions/` | **implement** | **done** | 既有 Translator；App/Northwind 物化覆盖 |
| B04 | 日期差 DateDiff* | P0 | `reference/function/datetime-functions/timestampdiff.md` | **implement** | **done** | 3.0.1 CAST；RuntimeGap/AppMatrix |
| B05 | 日期部件 Year/Month/Day | P0 | `reference/function/datetime-functions/` | **implement** | **done** | 既有 DbFunctions |
| B06 | JSON_VALUE / 路径投影 | P0 | `reference/function/json-functions/`、`reference/sql/datatype/json.md` | **implement** | **done** | JsonIntegrationTests；整列 LOB=LIMITATIONS |
| B07 | JSON 整列实体物化 | P1 | 同上 | **defer** | **done** | 13.206；推荐 JsonValue |
| B08 | ExecuteUpdate/Delete 核心 | P0 | `reference/sql/update.md`、`delete.md` | **implement** | **done** | 核心路径；边缘见 13.205 |
| B09 | Owned/TPT 批量 DML | P0 | 同上 | **excluded** | **done** | 链 13.205；SaveChanges |
| B10 | Identity 回读 LAST_INSERT_ID | P0 | `reference/function/system-infos-functions/last_insert_id.md` | **implement** | **done** | RETURNING 不可读（13.203） |
| B11 | INSERT…RETURNING | P1 | `reference/sql/insert.md` | **defer** | **done** | 驱动 FieldCount=0；probe 跟踪 |
| B12 | 乐观并发 ROW_COUNT | P0 | （无函数文档；E10049） | **excluded** | **done** | 13.201 决策 C |
| B13 | LIKE / 简单谓词 | P0 | `reference/sql/select.md` | **implement** | **done** | 标准翻译 |
| B14 | GROUP BY + HAVING COUNT | P0 | `reference/sql/select.md` | **implement** | **done** | CAST COUNT；RuntimeGap |
| B15 | 事务 Begin/Commit | P0 | `reference/sql/transaction.md` | **implement** | **done** | AppMatrix |
| B16 | 序列 NEXTVAL（非 IDENTITY） | P2 | `reference/object/sequence.md` | **defer** | **done** | 低频；另立 |
| B17 | 分区表 DDL | P2 | `reference/object/partition.md` | **excluded** | **done** | EF 罕触达 |
| B18 | CONVERT_TZ / 时区会话 | P1 | （无 CONVERT_TZ） | **excluded** | **done** | OOS-04 |
| B19 | REGEXP_LIKE | P1 | `reference/function/string-functions/` | **implement** | **done** | 既有（FULLTEXT 替代路径） |
| B20 | MYSQL compat 仍不同：IDENTITY/DBA_*/无 ROW_COUNT | P0 | `compatible_mode.md` + LIMITATIONS | **implement** | **done** | 13.306 验收表 |

## 终态统计（13.308）

| 处置 | 数量 |
|------|------|
| implement done | 12 |
| defer | 4 |
| excluded | 4 |
| **合计** | **20** |

无 `open` 行。P0 实现项均已有 Translator/测试或正式 exclusion；P1 未落地项均为 defer/excluded 并登记文档路径。
