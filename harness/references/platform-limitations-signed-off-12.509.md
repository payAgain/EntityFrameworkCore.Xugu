# Phase 12.509 — Platform Limitations Signed-Off（Path B）

> **状态**：**approved** @ Phase 12 W5 / 12.M5  
> **权威**：`docs/RELEASE-SCOPE.md` Path B — **vendor ticket + signed-off**  
> **关联**：`vendor-tickets-12.504.md`；`stub-and-exclusion.contract.md` §6

---

## 审批摘要

| 项 | 值 |
|----|-----|
| 审批 Wave | **12.W5**（12.501–12.510） |
| 平台限制项 | **2**（ROW_COUNT、Linux RID） |
| 解锁路径 | 驱动/XuguDB vendor 修复后 2.1.x / 2.2 patch |
| GA 3.0.0 阻塞 | **否** — Path B 签收完成，W6 可进入 |

> **用户批准**：下列平台能力在 v3.0.0 GA **不承诺**；每项均有实库/仓库证据 + vendor ticket 登记，**禁止**静默回退或假装支持。

---

## PLAT-01 — ROW_COUNT / DbUpdateConcurrencyException

| 字段 | 值 |
|------|-----|
| **ID** | **PLAT-01** |
| **Pomelo 对等** | `MySqlUpdateSqlGenerator` — `SELECT ROW_COUNT()` / `WHERE ROW_COUNT() = n` |
| **Provider 现状** | `XuguUpdateSqlGenerator` — `SELECT 1` / `WHERE 1 = n` 占位 |
| **实库证据** | `[E10049 L3 C9] 字段变量或函数"ROW_COUNT"()不存在` |
| **复验日期** | **2026-07-09**（12.501）— 仍 E10049；`COMPATIBLE_MODE=MYSQL` 亦不可用 |
| **Vendor ticket** | **VT-XUGU-ROWCOUNT-001**（见 `vendor-tickets-12.504.md`） |
| **Disposition** | **signed-off blocked** |
| **测试** | `OptimisticConcurrencyTests.Stale_concurrency_token_throws_*` — 显式 Skip（evidence） |
| **自动化探针** | `PlatformLimitationProbeTests.ROW_COUNT_function_returns_E10049` |

### 12.503 RecordsAffected fallback 结论

| 路径 | 可行性 | 说明 |
|------|--------|------|
| `XGDataReader.RecordsAffected` | **不可行** | EF `UpdateAndSelectSqlGenerator` 从 batch 末 `SELECT` 标量读 affected rows，非 ADO `ExecuteNonQuery().RecordsAffected` |
| 自定义 `RelationalCommand` | **不可行** | 需改 EF Relational 管道；超出 Provider 范围 |
| `SELECT 1` 占位维持 | **是** | 并发 token 列映射/UPDATE 正常；仅 `DbUpdateConcurrencyException` 自动检测不可用 |

**解锁条件**：XuguDB 提供 `ROW_COUNT()` 或等价 affected-rows SQL 函数，**或** 驱动在 batch UPDATE 后可靠返回 affected count 供 EF 消费。

---

## PLAT-02 — Linux x64 RID / libxugusql.so

| 字段 | 值 |
|------|-----|
| **ID** | **PLAT-02** |
| **Pomelo 对等** | `runtimes/linux-x64/native/` 多 RID NuGet |
| **仓库证据** | `external/csharp-driver/test_xugusql/linux-x64/` **不存在**；仅 `test_xugusql/64/xugusql.dll` |
| **复验日期** | **2026-07-09**（12.505）— `.so` 仍缺失 |
| **Vendor ticket** | **VT-XUGU-LINUXRID-001**（见 `vendor-tickets-12.504.md`） |
| **Disposition** | **signed-off platform exclusion** |
| **已预备** | `NativeAssets.props` + `EFCore.Xugu.csproj` 条件 `runtimes/linux-x64/native/` 打包 |
| **CI** | Windows-only 实库 job；Linux agent **signed-off defer**（12.508） |

**解锁条件**：虚谷在 `csharp-driver` 或 `Xuguclient` NuGet 发布 `linux-x64` 预编译 `libxugusql.so`。

---

## 显式 Skip 终态（平台项 — 1 方法）

| 测试 | Skip 字符串 | disposition |
|------|-------------|-------------|
| `OptimisticConcurrencyTests.Stale_concurrency_token_throws_DbUpdateConcurrencyException` | Signed-off **12.509/PLAT-01** E10049 | signed-off blocked |

**开放 defer Skip**：**0**（含平台项 evidence）

---

## 参考

- `docs/LIMITATIONS.md` §乐观并发 / §平台
- `harness/handoffs/phase10-wave4-2026-07-08.done.md` — 首次 E10049 实库验证
- `harness/handoffs/phase10-wave5-2026-07-08.done.md` — Linux RID 调研
- `harness/scripts/probe-platform-limitations.ps1` — 可重复探针
