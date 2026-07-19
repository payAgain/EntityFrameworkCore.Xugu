# Phase 13.201 — 乐观并发策略决策

**日期**：2026-07-19  
**选项**：

| 选项 | 描述 | 结论 |
|------|------|------|
| (A) | 等待 VT-XUGU-ROWCOUNT-001 | 保留跟踪，不阻塞产品口径 |
| (B) | RecordsAffected / 替代 SQL | **否决** — EF batch 从末尾 SELECT 标量读 affected；驱动 RecordsAffected 不可插入该路径（12.503） |
| **(C)** | 产品声明不支持自动 `DbUpdateConcurrencyException` | **采纳** |

**实现**：

- LIMITATIONS 更新为决策 C
- `OptimisticConcurrencyTests` Skip 文案指向 13.201(C)
- `XuguStrings.OptimisticConcurrencyExceptionNotSupported` + `XuguExceptionHints`（E10049）

**解锁条件**：服务端 `ROW_COUNT()` 或驱动提供 EF 可消费的 batch affected count → 重新评估 B 并取消 Skip。
