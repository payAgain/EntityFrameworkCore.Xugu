# Handoff: Phase 11 Closure — 2.1.0

**状态**: done  
**日期**: 2026-07-08  
**版本**: **2.1.0**（未打 tag；Release Gate 除实库 native 子集外已满足）

## 完成 Waves

| Wave | 范围 | 状态 |
|------|------|------|
| W1–W2 | 文档 + JSON Provider | done（前序） |
| W3–W5 | RETURNING / compat flip / 双 CI / 契约 | done |
| W6 | Version 2.1.0 + CHANGELOG + NuGet | done |
| W7 | 连接串校验器 + 集成样本 + GETTING-STARTED | done |
| W8 | Specification Phase 2 + 测试扩展 | done（+23 列测 → 898） |
| W9 | 驱动可选轨 | documented defer |

## 测试

- **898** 列测（compat 矩阵；`XUGU_DIALECT_MODE=compat`）
- 实库不可用时 SkippableFact skip（CI 设 `XUGU_CI_INTEGRATION=true` 跑全量）
- native 子集：`Category=NativeDialect`

## 延后（post-11）

- 11.105 ROW_COUNT（E10049 blocked）
- 11.205 Linux RID（无 libxugusql.so）
- 11.207 DateOnly SaveChanges
- 11.107 net8.0 TFM
- 11.202–11.204 FOR UPDATE / 位运算 / RelationalCommand

## Tag

未创建 `v2.1.0` tag（用户未明确要求；可在 Release Gate 全绿后手动 `git tag v2.1.0`）。
