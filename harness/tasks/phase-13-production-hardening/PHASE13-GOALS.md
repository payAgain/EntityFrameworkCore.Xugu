# Phase 13 — 生产硬化与应用验收（Post-GA）

> **状态**：**W1–W4 实现收口**（2026-07-19）  
> **前置**：`v3.0.0` GA + `v3.0.1` runtime-gap patch；RuntimeGap native/compat 9/9  
> **版本目标**：`3.0.2`（W1）→ `3.1.0`（W2）→ `3.2.0`（W3）→ `3.3.0`（W4）；`Version.props` = **3.3.0**  
> **北极星**：**应用路径硬验收 + 驱动阻抗治理 + 生产缺口**；**不再**以 Pomelo 对等 % 为唯一目标  
> **权威**：SQL = Xugu 官方文档；Pomelo = C# 架构参考 only；`COMPATIBLE_MODE=MYSQL` ≠ 产品语义

---

## Phase 目标（Done 定义）

Phase 13 **done** 当且仅当：

1. **应用矩阵** 进仓并进入严格实库门禁（禁止 Skip 计绿）
2. **驱动契约表** 覆盖聚合/时态/JSON/RETURNING 读写路径，缺口有 CAST/Converter 或 vendor ticket
3. **生产缺口** 有明确处置：并发探测、RETURNING、批量 DML 边缘、NuGet push
4. **文档漂移** Post-GA P1（12.PG2–PG7）清零；ROADMAP/BACKLOG 与 3.0.x 一致
5. **W3 业务函数清单** 按频度闭环或 formal defer（每项文档路径 + 物化测试）
6. **W4** 无客户需求时可标 `deferred-until-customer`，不阻塞 Phase 13 关闭

**明确不做（除非单独立项）**：NTS/FULLTEXT/Collation 硬搬；以 MYSQL compat 当迁移完成；无证据刷物理 `.cs` 194。

---

## 里程碑

| ID | 目标 | 版本 | 验收 |
|----|------|------|------|
| **13.M1** | 应用验收 + 契约 + 文档卫生 | **3.0.2** | 应用矩阵 CI 绿；契约表合并；Post-GA P1 清零；`v3.0.2` |
| **13.M2** | 生产缺口 + 发包 | **3.1.0** | 并发策略落地；RETURNING 路径评估/实现；NuGet push；`v3.1.0` |
| **13.M3** | 高频方言加深 | **3.2.0** | 业务函数清单 100% done/defer；`v3.2.0` |
| **13.M4** | 多兼容模式 API（可选） | 按需 | ORACLE/PG `SET` API + 标识符测试；**无 Oracle/PG SQL 方言承诺** |

---

## Wave 总览

```
Wave 1（应用验收面）     : 13.101–13.112 — 矩阵/契约/文档/3×门禁     → 13.M1 / 3.0.2
Wave 2（生产缺口）       : 13.201–13.210 — 并发/RETURNING/DML/NuGet → 13.M2 / 3.1.0
Wave 3（高频方言）       : 13.301–13.308 — 业务清单驱动 Translator   → 13.M3 / 3.2.0
Wave 4（兼容模式可选）   : 13.401–13.405 — ORACLE/PG API（客户门控） → 13.M4
Vendor 并行              : V-01/V-02/V-03 — ROW_COUNT / Linux / 驱动类型
```

| Wave | 任务 ID | 进入条件 | 退出条件 | 状态 |
|------|---------|----------|----------|------|
| **W1** | **13.101–13.112** | Phase 12 done + 3.0.1 | 13.M1 ✅ | **done**（13.107/111 证据/defer） |
| **W2** | **13.201–13.210** | W1 done | 13.M2 ✅ | **done**（13.209 NuGet push defer） |
| **W3** | **13.301–13.308** | W1 矩阵基线 | 13.M3 ✅ | **done** |
| **W4** | **13.401–13.405** | **客户门控已开** | 13.M4 ✅ | **done** |

---

## 禁止与约束

- **禁止**改 `external/csharp-driver` / Pomelo 源码（只读）
- **禁止**无 Xugu 文档路径的 SQL 实现
- **禁止**把门禁 Skip 当 PASS（`XUGU_REQUIRE_DATABASE=true`）
- **禁止**声称「覆盖全部 Xugu 语法」；验收口径 = EF ORM 能力矩阵
- MYSQL compat：仅 opt-in；验收必须包含「与 MySQL 仍不同」清单
