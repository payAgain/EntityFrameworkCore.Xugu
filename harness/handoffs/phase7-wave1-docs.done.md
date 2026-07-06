# Phase 7 Wave 1 — 文档任务 Handoff

> 日期：2026-07-06  
> 任务：7.R1 + 7.R4 + 7.R2  
> Agent：Infra / Orchestrator（文档波次）

---

## 一、完成情况

| 任务 ID | 描述 | 状态 |
|---------|------|------|
| 7.R1 | 发版文档与用户快速开始 | ✅ done |
| 7.R4 | Xuguclient 依赖策略文档 | ✅ done |
| 7.R2 | README / ROADMAP / BACKLOG 数字同步 | ✅ done |

**未执行**：7.R3（Wave 5）、Provider 代码变更、git push。

---

## 二、新建文件

| 路径 | 内容 |
|------|------|
| `docs/GETTING-STARTED.md` | 连接串、`UseXugu`、兼容模式、迁移、`dotnet ef`、常见错误、限制文档链接 |
| `docs/LIMITATIONS.md` | defer/skip/pending 汇总：Retry、DateOnly SaveChanges、CONVERT_TZ、Collation、ExecuteDelete（7.Q1 pending）等 |
| `docs/xuguclient-dependency-strategy.md` | `UseLocalXuguDriver` vs NuGet `Xuguclient`、版本 pin（3.3.5 / 3.3.6-bionic）、CI `-Pack` 用法 |

---

## 三、更新文件

| 路径 | 变更 |
|------|------|
| `README.md` | 新增「用户文档」节，链接三份 `docs/` 文档 |
| `src/EFCore.Xugu/README.md` | 补充迁移说明、兼容模式示例、文档表 |
| `harness/tasks/phase-7-release-1.0.0/TASKS.md` | 7.R1 / 7.R2 / 7.R4 → **done** |
| `harness/tasks/ROADMAP.md` | 进度日志：Phase 7 W1 文档完成 |
| `harness/tasks/BACKLOG.md` | 7.R4 映射 → **done** |

---

## 四、7.R2 数字同步核对

| 指标 | README | ROADMAP | BACKLOG | TASKS | 实测/统计 |
|------|--------|---------|---------|-------|-----------|
| 测试方法 | 116/116 | 116/116 | 116 | ≥116 | 基线 116（Phase 6 handoff） |
| Provider .cs | 85 | 85 | 85 | — | `Get-ChildItem src/EFCore.Xugu -Recurse -Filter *.cs` → **85** |
| Pomelo .cs | 194 | 194 | — | — | 未变 |
| 当前 Phase | 7 | 7 active | — | active | 一致 |
| 版本 | 0.1.0-preview | 0.1.0-preview | 0.1.0-preview | 目标 1.0.0 | `Version.props` 未改（7.V1） |

**说明**：本环境 `dotnet test` 因并行 Wave 1 实现任务（7.Q2）引入编译错误暂未能全量重跑；数字与 Phase 6 关闭 handoff 及仓库内多处文档一致，7.R2 仅做交叉核对与指针维护，未修改计数。

---

## 五、文档要点摘要

### GETTING-STARTED

- Xugu 键值对连接串（非 MySQL 格式）
- 默认 `SET compatible_mode TO 'MYSQL'`（可 `DisableCompatibleModeOnOpen()`）
- `dotnet ef migrations add` / `database update` / scaffold
- `EnsureCreated`、幂等脚本、过滤索引不支持
- 链接 `LIMITATIONS.md`、`xuguclient-dependency-strategy.md`

### LIMITATIONS

- **pending**：ExecuteDelete/Update（7.Q1）、TypeMapping（7.S1）、冒烟测试（7.T1）
- **defer**：Retry、DateOnly/TimeOnly SaveChanges、CREATE/DROP DATABASE、CONVERT_TZ、linux native
- **skip**：Collation、FULLTEXT、JSON/NTS、Scaffolding baselines

### xuguclient-dependency-strategy

- 默认 `UseLocalXuguDriver=true` → `external/csharp-driver` ProjectReference
- 发布 `-p:UseLocalXuguDriver=false` → NuGet `Xuguclient` `VersionOverride=3.3.6-bionic`
- `Directory.Packages.props` 中央版本 `3.3.5`
- `ci-build.ps1 -Pack` 强制 NuGet 模式

---

## 六、下游影响

| 任务 | 关系 |
|------|------|
| 7.T2 | 可在 7.S2 结论后审计/扩充 `LIMITATIONS.md` |
| 7.R3 | 可引用本文档中的 feed / 版本策略 |
| 7.T3 | CHANGELOG 可链接 `GETTING-STARTED.md` |
| 7.V1 | 发版时更新文档中的版本号字符串 |

---

## 七、验收

文档任务无 `verify-module.ps1` 门禁；未修改 Provider 逻辑。

可选后续（非本波次）：

```powershell
# 待 7.Q2 等实现任务合并后
harness/scripts/verify.ps1
```

---

## 八、Git

变更可单独 commit；**未 push**（按任务约束）。
