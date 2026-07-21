# Phase 13.201 — 乐观并发策略决策（修订）

**日期**：2026-07-19（初版）；**2026-07-20**（Path A 落地）

| 选项 | 描述 | 结论 |
|------|------|------|
| **(A)** | Provider：`DbDataReader.RecordsAffected`（无 `ROW_COUNT()`） | **采纳（2026-07-20）** |
| (B) | Vendor：`ROW_COUNT()` SQL 标量 | 仍跟踪 VT-XUGU-ROWCOUNT-001（可选简化） |
| (C) | 产品声明不支持自动 `DbUpdateConcurrencyException` | **废止**（由 A 取代） |

## Path A 证据（SYSDBA @ 192.168.2.239，对齐 xgconsole_local.bat）

| 路径 | 命中 UPDATE/DELETE | 陈旧 token → 0 | 备注 |
|------|-------------------|----------------|------|
| `ExecuteNonQuery`（字面量/`:param`） | ≥1 | 0 | ok |
| `ExecuteReader.RecordsAffected`（UPDATE/DELETE + `:param`） | ≥1 | 0 | **EF 主路径** |
| `ExecuteReader.RecordsAffected`（INSERT + `:param`） | 恒为 0 | — | Provider 对 `EntityState.Added` 跳过误报 |
| `@p` 参数名 | 恒为 0 | — | 须用 `XuguSqlGenerationHelper` 的 `:` |

## 实现

- `XuguUpdateSqlGenerator.AppendSelectAffectedCountCommand`：不再生成 `SELECT 1`
- `XuguModificationCommandBatch.ConsumeResultSetWithRowsAffectedOnly*`：读 `RecordsAffected`；INSERT 零值特例
- `OptimisticConcurrencyTests.Stale_*`：**启用**（不再 Skip）

## 文档

- `LIMITATIONS.md` / `ado-driver-contract.md` / `CHANGELOG` Unreleased
