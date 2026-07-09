# Phase 12 W5 Handoff — Platform / CI（12.M5）

> **日期**：2026-07-09  
> **状态**：**done**  
> **里程碑**：**12.M5** Platform parity ✅  
> **前置**：W1（12.M1）+ W2（12.M2）+ W3（12.M3）+ W4（12.M4）done

---

## 完成项

| ID | 内容 | 结果 |
|----|------|------|
| 12.501 | ROW_COUNT 实库复验 | **仍 E10049** — `PlatformLimitationProbeTests` 自动化探针 |
| 12.502 | 乐观并发测试 | **signed-off** PLAT-01 — Skip 更新为 evidence 字符串 |
| 12.503 | RecordsAffected fallback | **不可行** — EF batch `SELECT` 路径，非 ADO `RecordsAffected` |
| 12.504 | Vendor ticket 登记 | `vendor-tickets-12.504.md` — VT-XUGU-ROWCOUNT-001 / VT-XUGU-LINUXRID-001 |
| 12.505 | Linux libxugusql.so | **仍缺失** — `test_xugusql/linux-x64/` 不存在 |
| 12.506 | Linux RID 打包 | **条件预备** — `EFCore.Xugu.csproj` 待 `.so` 自动打包 |
| 12.507 | NativeAssets.props | **done** — `XuguNativeSoPath` / `XuguNativeDllPath` |
| 12.508 | 跨平台 CI | **signed-off** — Windows-only 实库 job；Linux defer PLAT-02 |
| 12.509 | Platform limitation 文档 | `platform-limitations-signed-off-12.509.md` + `LIMITATIONS.md` |
| 12.510 | Production checklist | `PRODUCTION-RELEASE-CHECKLIST.md` P0 平台项 ✅ |

---

## 平台签收（Path B）

| ID | 能力 | 处置 | Vendor ticket |
|----|------|------|---------------|
| **PLAT-01** | `ROW_COUNT()` / `DbUpdateConcurrencyException` | **signed-off blocked** | VT-XUGU-ROWCOUNT-001 |
| **PLAT-02** | Linux x64 `libxugusql.so` / RID | **signed-off platform exclusion** | VT-XUGU-LINUXRID-001 |

**GA 阻塞**：**否** — W6 可进入 Release Gate

---

## 代码变更

| 文件 | 变更 |
|------|------|
| `PlatformLimitationProbeTests.cs` | **新增** — 12.501 ROW_COUNT E10049 自动化复验 |
| `OptimisticConcurrencyTests.cs` | Skip → signed-off PLAT-01 |
| `probe-platform-limitations.ps1` | **新增** — W5 可重复探针 |
| `platform-limitations-signed-off-12.509.md` | **新增** — Path B 签收表 |
| `vendor-tickets-12.504.md` | **新增** — vendor ticket 登记 |
| `LIMITATIONS.md` | ROW_COUNT + Linux 平台节 |
| `PRODUCTION-RELEASE-CHECKLIST.md` | W5 P0 平台项勾选 |
| `TESTING.md` / `ci.yml` | 12.508 Windows-only 注释 |
| `TASKS.md` / `ROADMAP.md` / `BACKLOG.md` | W5 done → W6 |

---

## 验证

```powershell
$env:PATH = "C:\Program Files\dotnet;$env:PATH"
harness/scripts/verify.ps1                                    # PASS
harness/scripts/probe-platform-limitations.ps1              # ROW_COUNT E10049 + .so MISSING
$env:XUGU_DIALECT_MODE = 'compat'
dotnet test test/EFCore.Xugu.Tests -c Release -v q          # 0 FAIL（1057 列测）
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect" -v q  # 0 FAIL
```

---

## W6 入口（仅预览 — 不在本 Wave 执行）

| 任务 | 范围 |
|------|------|
| 12.601 | Release Gate 全量复验 |
| 12.602 | Dual matrix 3× 0 FAIL |
| 12.603–604 | LIMITATIONS 3.0.0 + CHANGELOG |
| 12.605–606 | Version.props 3.0.0 + publish-nuget |
| 12.609 | `git tag v3.0.0` |

**Critical path 延续**：W6 GA Gate → `v3.0.0` tag
