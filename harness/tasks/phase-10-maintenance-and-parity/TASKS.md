# Phase 10 — 维护与剩余对等

> **状态**：`in_progress`（Wave 1 **done**）  
> **前置**：Phase 9 `done`（676 列测，~64% Pomelo，**2.0.0**）  
> **目标**：巩固 2.0.0 维护线、补齐高价值剩余测试、落地 defer 项与发布/CI

## 里程碑

| ID | 目标 | 验收 |
|----|------|------|
| 10.M1 | 维护基线稳定 | 676 列测实库 **0 FAIL**；CI build 绿 |
| 10.M2 | 测试 +80~120 | 列测 ≥ **750**（~71% Pomelo） |
| 10.M3 | 发布就绪 | NuGet 可发布；用户文档与 XUGU-VS-MYSQL 同步 |
| 10.M4 | 可选深度对等 | 列测 ≥ **850** 或 Monster/Spec 子集 done |

---

## P0 — 立即（维护与发布）

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 10.001 | **CI/CD 实库矩阵** | Infra | 9.I6 | **done** | `.github/workflows/ci.yml` + `.gitlab-ci.yml`；secrets 见 `docs/TESTING.md` |
| 10.002 | **全量门禁回归** | Testing | Phase 9 | **done** | `verify.ps1 -RunTests`；CI integration job |
| 10.003 | **NuGet 发布流水线** | Release | 7.R3 | **done** | dry-run 验证 **2.0.0**；GitLab `publish-nuget` manual job |
| 10.004 | **用户文档刷新** | Docs | 10.003 | **done** | `GETTING-STARTED.md` → 2.0.0；链到 `XUGU-VS-MYSQL.md` |
| 10.005 | **剩余测试 triage** | Testing | 9.O3 | **done** | `harness/references/phase-10-test-triage.md`；Wave 2–6 计划 |

---

## P1 — 高价值对等

| ID | 任务 | Pomelo 参考 | 依赖 | 状态 | 说明 |
|----|------|-------------|------|------|------|
| 10.101 | **Monster Fixup 子集** | `MonsterFixup*MySqlTest` | 10.005, 9.I4 | todo | 变更跟踪/fixup 高价值用例；表前缀隔离 |
| 10.102 | **Specification Tests 子集** | `EFCore.Specification.Tests` 数据库相关 | 10.005 | todo | 与 Xugu 能力交集；skip JSON/Spatial |
| 10.103 | **Query 深覆盖 Wave** | 剩余 Northwind/AdHoc | 9.W6 | todo | +80~120 列测；目标 ~71% |
| 10.104 | **9.T defer 补全** | 见下表 | 9.T* | todo | WithConstructors insert、SaveChangesInterception 全量、CompositeKey 等 |
| 10.105 | **ROW_COUNT 乐观并发** | `OptimisticConcurrencyMySqlTest` | 驱动/方言 | todo | 解锁 `Stale_concurrency_token_throws_*`；需回归 CRUD |
| 10.106 | **Retry Strategy 实装** | `MySqlRetryingExecutionStrategy` | 驱动 XGCI | todo | 前置：驱动 `IsTransient` 或 Message 码契约稳定 |
| 10.107 | **EF 版本矩阵** | Pomelo 多 TFM | — | todo | 评估 net8.0 目标；与 EF Core 9 对齐策略 |
| 10.108 | **JSON 列调研** | `Json*MySqlTest` | XuguDB 文档 | todo | **可选** — 若官方支持 JSON 类型再开 10.109 实现 |

### 10.104 defer 子项

| 来源 | 测试/能力 | Phase 10 处置 |
|------|-----------|---------------|
| 9.T25 | `WithConstructorsTests` insert ×2 | 10.104 — 构造函数图持久化 |
| 9.T20 | `SaveChangesInterceptionTests` 全矩阵 | 10.104 — 拦截器 async/顺序 |
| 9.T22 | `SeedingTests` EnsureCreated+HasData | 10.104 — 设计时模型 seed |
| 9.T10 | `ConvertToProviderTypes` 全矩阵 | 10.104 — BuiltIn 标量往返 |
| 9.T15/18 | computed 列 | 10.104 — 若 XuguDB 支持 |
| 9.T23 | optional complex null | 10.104 — 跟踪 EF #31376 |

---

## P2 — defer / 平台 / 低优先级

| ID | 任务 | 原 ID | 状态 | 说明 |
|----|------|-------|------|------|
| 10.201 | 参数内联 | 8.Q14 | todo | 查询性能优化 |
| 10.202 | FOR UPDATE / 窗口函数 | 8.Q12 | todo | EF 无标准 Tag 入口；调研后定 |
| 10.203 | 位运算返回类型 | 8.Q11 | todo | `BitwiseOperationReturnTypeCorrecting` |
| 10.204 | RelationalCommand 表面 | 8.S8–S10 | todo | Database/Command 扩展 API |
| 10.205 | **Linux x64 RID 打包** | 8.N1–N3 | todo | `xugusql` linux 二进制 + nuspec |
| 10.206 | **9.IT2 IntegrationTests** | 9.IT2 | todo | ASP.NET + Vegeta 性能宿主；低价值，按需 |
| 10.207 | DateOnly/TimeOnly SaveChanges | P3-11 | todo | 依赖 csharp-driver 原生参数 |
| 10.208 | ConnectionString 校验器 | BACKLOG | todo | Xugu 键值对格式校验 |
| 10.209 | Scaffolding Baselines | 9 skip | **skip** | 维护成本过高，不移植 |
| 10.210 | ConvertTimeZone / FULLTEXT / NTS / Collation | 8.* skip | **skip** | 文档确认不实现 |

---

## 永久 skip（不进入 Phase 10 实现）

| 类别 | Pomelo 源 |
|------|-----------|
| Spatial / NTS | `SpatialMySqlTest` |
| FULLTEXT | `MatchQueryMySqlTest` |
| JSON 反序列化（无扩展时） | `BadDataJsonDeserializationMySqlTest` |
| Scaffolding Baselines 全量快照 | Pomelo baseline 文件 |
| Lazy loading proxies | 无测试宿主 |

---

## Wave 建议顺序

```
Wave 1（P0）: 10.001–10.005 — CI + 文档 + triage  ✅ done
Wave 2（P1）: 10.103 + 10.104 — Query defer 补全
Wave 3（P1）: 10.101 + 10.102 — Monster/Specification
Wave 4（P1）: 10.105 + 10.106 — 驱动依赖项（并行调研）
Wave 5（P2）: 10.205 + 10.201 — 平台/性能
Wave 6（可选）: 10.108 JSON — 文档确认后
```

---

## 门禁

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests   # 基线 676，Wave 后递增
dotnet test Xugu.EFCore.Xugu.sln -c Release                   # 0 FAIL
```

---

## 参考

- `harness/tasks/ROADMAP.md` — Phase 10 摘要
- `harness/handoffs/phase9-m3-test-parity-2026-07-07.md`
- `harness/references/test-parity-matrix.md`
- `harness/references/phase-10-test-triage.md`
- `docs/XUGU-VS-MYSQL.md`
- `harness/tasks/BACKLOG.md`
