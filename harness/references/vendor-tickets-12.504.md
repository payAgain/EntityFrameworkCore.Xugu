# Phase 12.504 — Vendor Ticket 登记

> **状态**：**filed (internal)** @ Phase 12 W5  
> **用途**：ROW_COUNT / Linux RID 驱动依赖项追踪；待虚谷官方分配外部 ticket # 后回填  
> **关联**：`platform-limitations-signed-off-12.509.md`

---

## 登记摘要

| 内部 ID | 主题 | 依赖方 | 状态 | 登记日 |
|---------|------|--------|------|--------|
| **VT-XUGU-ROWCOUNT-001** | `ROW_COUNT()` / affected-rows API | XuguDB 方言 + XuguClient ADO | **open** — 待 vendor # | 2026-07-09 |
| **VT-XUGU-LINUXRID-001** | `libxugusql.so` linux-x64 RID | XuguClient / csharp-driver Release | **open** — 待 vendor # | 2026-07-09 |

> **外部 ticket #**：虚谷技术支持尚未分配公开编号；本表为项目内权威追踪，解锁时更新 `外部 ticket` 列。

---

## VT-XUGU-ROWCOUNT-001

| 字段 | 内容 |
|------|------|
| **请求** | XuguDB 提供 MySQL 兼容 `ROW_COUNT()` 或文档化等价 affected-rows 函数；和/或 XuguClient 在 UPDATE/DELETE batch 后可靠暴露 affected row count 供 EF Core 消费 |
| **XGCI 码** | **E10049** — `字段变量或函数"ROW_COUNT"()不存在` |
| **复验** | 2026-07-08（10.105）、**2026-07-09**（12.501）— 仍失败 |
| **影响** | `DbUpdateConcurrencyException` 自动检测不可用；1 测试 Skip |
| **Provider 缓解** | 并发 token 列映射 + UPDATE 含 token 列仍可用；业务层可手工版本检查 |
| **解锁任务** | 移除 `SELECT 1` 占位 → Pomelo 风格 `ROW_COUNT()`；启用 `OptimisticConcurrencyTests.Stale_*` |

---

## VT-XUGU-LINUXRID-001

| 字段 | 内容 |
|------|------|
| **请求** | 在 `csharp-driver` 仓库或 `Xuguclient` NuGet 发布 `linux-x64` 预编译 `libxugusql.so` |
| **仓库证据** | `external/csharp-driver/test_xugusql/64/xugusql.dll` ✅；`linux-x64/libxugusql.so` ❌ |
| **复验** | 2026-07-08（10.205）、**2026-07-09**（12.505）— 仍缺失 |
| **影响** | NuGet 仅 `win-x64` RID；Linux 部署需自行编译 C++ 驱动或等待官方包 |
| **Provider 预备** | `NativeAssets.props` + `EFCore.Xugu.csproj` 条件打包已就绪 |
| **解锁任务** | 启用 `linux-x64` nupkg；添加 GitHub `integration-linux` job（12.508） |

---

## 回填模板（vendor 响应后）

```markdown
| VT-XUGU-ROWCOUNT-001 | 外部 #XXXX | closed YYYY-MM-DD | 版本 X.Y.Z |
| VT-XUGU-LINUXRID-001 | 外部 #YYYY | closed YYYY-MM-DD | Xuguclient Z.Z.Z |
```
