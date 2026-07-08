# Phase 11 — NuGet 打包与集成验证计划

> **状态**：`planned`  
> **目标**：2.1.0 发布前验证「包可安装、样本可跑、测试策略可延续」  
> **前置**：Phase 10 NuGet dry-run（2.0.0）已 PASS

---

## 1. NuGet Pack → Install → 冒烟

### 1.1 自动化脚本

```powershell
# 自仓库根目录
harness/scripts/test-nuget-pack.ps1
```

脚本步骤（见 `harness/scripts/test-nuget-pack.ps1`）：

1. `publish-nuget.ps1 -Pack` → `artifacts/Microsoft.EntityFrameworkCore.Xugu.{version}.nupkg`
2. 在临时目录创建 **干净** `net9.0` 控制台项目（无 ProjectReference）
3. 添加本地 feed：`dotnet nuget add source {artifacts} --name xugu-local`
4. `dotnet add package Microsoft.EntityFrameworkCore.Xugu`
5. `dotnet build` — 必须 PASS
6. 可选：`-SmokeConnect` 时执行 `CanConnect()`（需 `XUGU_CONNECTION` 环境变量）

### 1.2 手动验收清单

| 步骤 | 命令 / 检查 | 通过标准 |
|------|-------------|----------|
| Pack | `harness/scripts/publish-nuget.ps1 -Pack` | nupkg 存在于 `artifacts/` |
| 依赖 | 解压 nupkg 检查依赖 | 声明 `Xuguclient` NuGet，非本地 dll |
| 干净安装 | `test-nuget-pack.ps1` | 无 ProjectReference 编译 PASS |
| 设计时 | 消费项目 `dotnet ef migrations add` | 与 `samples/EfDesignSample` 同等 |
| 原生 dll | Windows x64 运行 | `xugusql.dll` 随 Xuguclient 或文档说明部署 |

### 1.3 与 CI 的关系

- **PR 门禁**：`ci-build.ps1`（build + test + verify）— 沿用 Phase 10
- **发布门禁**：`test-nuget-pack.ps1` — Phase 11 **W6** 起加入 manual / release job（建议 W5 偏差修复轨完成后）
- **实库**：依赖 `XUGU_CONNECTION` / CI secrets（见 `docs/TESTING.md`）

---

## 2. 真实场景模板

### 2.1 现有样本

| 路径 | 用途 | Phase 11 动作 |
|------|------|---------------|
| `samples/EfDesignSample/` | `dotnet ef` migrations + scaffold | 保持；README 链到 2.1.0 |
| `test/integration-sample/` | **新建** 最小集成宿主 | 11.304 骨架 |

### 2.2 integration-sample 场景（骨架）

**目标**：验证 NuGet 消费方典型路径，非性能测试。

```
test/integration-sample/
├── README.md
├── IntegrationSample.sln          # 可选，或纳入主 sln
├── MinimalApi/
│   ├── MinimalApi.csproj          # PackageReference Provider（或 ProjectReference 开发态）
│   ├── Program.cs                 # WebApplication + UseXugu
│   ├── AppDbContext.cs
│   └── appsettings.json           # ConnectionStrings:Xugu
└── scripts/
    └── run-smoke.ps1              # migrate + GET /health + POST/GET entity
```

**冒烟流程**：

1. `dotnet ef database update`（或 `EnsureCreated` 开发模式）
2. `POST /api/items` — Insert
3. `GET /api/items` — Query
4. `PUT /api/items/{id}` — Update（含 concurrency token 可选）
5. `DELETE /api/items/{id}` — Delete

**环境变量**：`XUGU_CONNECTION` 或 `appsettings.Development.json`（不提交密钥）。

### 2.3 JSON 场景（11.109 完成后追加）

在 `AppDbContext` 增加 `JsonDocument` / `string` JSON 列实体；验证：

- 迁移生成 `JSON` 列 DDL
- LINQ 过滤（`EF.Functions` 或原生翻译路径）
- 与 `LIMITATIONS.md` 一致

---

## 3. EF Core 全量测试套件可行性评估

### 3.1 Pomelo 实际运行什么

| 套件 | 位置 | 规模（约） | Pomelo 做法 |
|------|------|-----------|-------------|
| **FunctionalTests** | `test/EFCore.MySql.FunctionalTests/` | ~1050 列测 | 主门禁；Xugu 已移植 ~82% |
| **EFCore.Specification.Tests** | EF Core 仓库子模块 | **数千** 列测 | Pomelo 通过 `MySqlFixture` 跑关系库相关子集 |
| **Monster / Graph** | FunctionalTests 内 | ~30–50 | Xugu 已有子集（10.101） |
| **IntegrationTests** | ASP.NET + 负载 | ~15–20 | Pomelo 有；Xugu **defer**（10.206） |

### 3.2 EF.Specification.Tests — 诚实评估

| 维度 | 评估 |
|------|------|
| **全量跑通** | **不现实**作为 Phase 11 门禁。EF Core 官方 Specification 覆盖所有 Provider 抽象行为，含大量与 Xugu 无关或已 skip 的场景（Spatial、JSON Pomelo 扩展等）。 |
| **当前 Xugu** | Phase 10 已落地子集：`DesignTimeXuguTest`、`KeysWithConverters`、`TransactionBasics`（10.102）。 |
| **Phase 11 建议** | **Phase 2 扩展**（11.402）：再增 3–5 个高价值类（如 `BulkUpdates`、`GraphUpdates` 子集），目标 +30~50 列测，**非**全量。 |
| **工作量粗估** | 每个 Specification 类：4–16h（Fixture 适配 + 实库 skip 标记）；全量 **数人月** — **不推荐**作为 2.1.0 条件。 |

### 3.3 Monster Tests

| 项 | 状态 | Phase 11 |
|----|------|----------|
| `MonsterFixupXuguTests` | done（10.101） | 可选扩展 11.403 |
| `StoreGeneratedFixupXuguTests` | done | 同上 |
| Pomelo 全量 Monster | ~40 方法 | 许多依赖 MySQL 专有或已 skip 能力 |

**建议**：维持手写 Xugu 兼容模型策略；不追求 Pomelo Monster 文件名一一对应。

### 3.4 分阶段测试路线图

```
Phase 11（2.1.0）: compat ≥861 列测 0 FAIL + native CI 核心子集 0 FAIL + JSON 测试 + NuGet 冒烟（W6）
Phase 11 W8（P2）: FunctionalTests +20~40（非 skip、非 MySQL 专有）
Phase 12+（可选）: Specification 再扩展 50~100 方法
长期（非承诺）: 随 EF Core 版本升级跟进 Specification 增量
```

### 3.5 跑 Specification 的技术前置（若扩展 11.402）

1. 引用 `Microsoft.EntityFrameworkCore.Specification.Tests`（与 EF 9 版本对齐）
2. 复用 `XuguSpecificationFixtureHelper` + `XuguTestStore`
3. 每类测试：`[XuguTestCondition]` / skip 永久 OUT OF SCOPE 场景
4. CI：实库 job 超时与并行度控制（Phase 10 已配置）

---

## 4. 发布前集成检查表

复制到 Phase 11 Handoff：

- [ ] `test-nuget-pack.ps1` PASS
- [ ] `integration-sample` README 步骤可执行（有实库）
- [ ] `EfDesignSample` migrations 与 2.1.0 包版本一致
- [ ] JSON 实体冒烟（11.109 后）
- [ ] `docs/TESTING.md` 更新 secrets / 本地 XuguDB 说明
- [ ] 全量 `dotnet test` 0 FAIL

---

## 5. 参考

- `harness/scripts/publish-nuget.ps1`
- `harness/scripts/test-nuget-pack.ps1`
- `samples/EfDesignSample/README.md`
- `harness/references/phase-10-test-triage.md`
- `test/EFCore.Xugu.Tests/TestUtilities/XuguSpecificationFixtureHelper.cs`
