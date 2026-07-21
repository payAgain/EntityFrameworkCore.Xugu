# Testing — Microsoft.EntityFrameworkCore.Xugu

> **架构与用例规模汇总**：[TEST-ARCHITECTURE.md](TEST-ARCHITECTURE.md)  
> **L1 / L2 / L3 基线**（2026-07-17）。PR 强制 Unit；main / nightly / release 强制 Integration 双方言全量 + Experiential。

## 三层门禁

| 层 | 项目 | 触发 | 数据库 | 命令 |
|----|------|------|--------|------|
| **L1 Unit** | `test/EFCore.Xugu.Tests.Unit` | **每次 PR / push** | 不需要 | `harness/scripts/run-unit-gate.ps1` |
| **L2 Integration** | `test/EFCore.Xugu.Tests.Integration` | **main / nightly / tag `v*`** | **必需**（`XUGU_REQUIRE_DATABASE=true`） | native 先：`run-native-gate.ps1`；再 compat：`run-compat-gate.ps1`（均为**全量**） |
| **L3 Experiential** | pack + EfDesignSample + MinimalApi | **nightly / tag `v*`** | 必需 | `harness/scripts/run-experiential-gate.ps1` |

共享基建：`test/EFCore.Xugu.Tests.Shared`（非测试项目）。

### Spec Functional（对齐虚谷开源矩阵）

| 项 | 说明 |
|----|------|
| 工程 | `test/EFCore.Xugu.Tests.Functional` |
| 对照 | [Xugu-Open-Source/efcore](https://github.com/Xugu-Open-Source/efcore) `v8.0.0-xugu` FunctionalTests |
| 形态 | 继承 `Microsoft.EntityFrameworkCore.Relational.Specification.Tests` `*TestBase` |
| W1 套件 | NullSemantics、GearsOfWar(+TPT/TPC)、ComplexNavigations、Owned、PrimitiveCollections、TPC Inheritance Query、BulkUpdates |
| 规模 | `--list-tests` 约 **8500+**（含 Theory 展开） |
| 运行 | 需实库；`XUGU_REQUIRE_DATABASE=true`；共享 SYSTEM + 表前缀（非 CREATE DATABASE） |
| 金标 | Wave1 **结果断言优先**；AssertSql 多已 deferred |

```powershell
$env:XUGU_DIALECT_MODE = 'native'
$env:XUGU_REQUIRE_DATABASE = 'true'
dotnet test test/EFCore.Xugu.Tests.Functional -c Release --filter "FullyQualifiedName~NullSemantics"
```

设计：`docs/superpowers/specs/2026-07-21-spec-matrix-alignment-design.md`。

### 方言矩阵（L2）

| Job | `XUGU_DIALECT_MODE` | 范围 |
|-----|---------------------|------|
| **native（主）** | `native` | Integration **完整套件** |
| **compat** | `compat` | Integration **完整套件**（不降级） |

产品默认仍为 native-first；compat 完整测试用于对照与回归，**不是**产品方言定义。

### QualityMatrix（质量补强子集）

Trait `Category=QualityMatrix`，覆盖此前证据缺口：

| 类 | 覆盖 |
|----|------|
| `UpdatesMatrixTests` | 图插入/更新/删除、多实体同批 SaveChanges、Attach 修改 |
| `ExecuteBulkBoundaryTests` | LIMITATIONS 13.205 支持/拒绝全矩阵（TPH/TPT/TPC/Owned/ORDER·LIMIT/DISTINCT/GROUP BY/导航目标） |
| `JsonBoundaryTests` | 大 LOB 物化边界、`ToJson` owned 非支持路径 |
| `RetryFaultInjectionTests` | 实库路径上拦截器注入 `[E19886]` + `EnableRetryOnFailure` |
| `RetryServerDisconnectTests` | 实库 `DROP SESSION`（`USERENV('SID')`）+ `EnableRetryOnFailure` 恢复 |
| `SequenceIntegrationTests` | `CREATE SEQUENCE` / `NEXTVAL` / `DROP SEQUENCE IF EXISTS` |
| `ClusterIntegrationTests` | 多监听端口集群：各节点 Open、`SHOW CLUSTERS`、跨节点读写 / EF SaveChanges（需 `XUGU_CLUSTER_PORTS`） |

```powershell
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests.Integration -c Release --filter "Category=QualityMatrix"

# 三节点同机多端口集群示例（dym-cluster）
$env:XUGU_CONNECTION_STRING = 'IP=192.168.2.239; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5287; AUTO_COMMIT=on; CHAR_SET=UTF8'
$env:XUGU_CLUSTER_PORTS = '5287,5288,5289'
$env:XUGU_REQUIRE_DATABASE = 'true'
dotnet test test/EFCore.Xugu.Tests.Integration -c Release --filter "Category=Cluster"
```

### 冻结列测下限

| 项目 | `--list-tests` 下限 |
|------|---------------------|
| Unit | **≥ 250**（当前冻结约 **252**） |
| Integration | **≥ 850**（当前冻结约 **855**） |

回归低于下限即门禁失败（见 `run-unit-gate.ps1` / `run-integration-gate.ps1`）。

## 前置条件

| 项 | 说明 |
|----|------|
| .NET SDK | 9.0+（`global.json`） |
| XuguDB（L2/L3） | 默认 `127.0.0.1:5138`；`harness/scripts/start-xugudb.ps1` |
| 原生驱动 | Integration 构建复制 `xugusql.dll` |
| `dotnet-ef`（L3） | `dotnet tool install --global dotnet-ef` |

## 环境变量

| 变量 | 用途 | 默认 |
|------|------|------|
| `XUGU_CONNECTION_STRING` | 实库连接串 | `IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8` |
| `XUGU_CONNECTION` | EfDesignSample 别名 | 同连接串 |
| `XUGU_CLUSTER_PORTS` | 集群监听端口列表（逗号分隔）；与主连接串同主机/凭据 | 未设置则 `Category=Cluster` Skip |
| `XUGU_DIALECT_MODE` | `compat` 或 `native` | **未设置 = native**（产品默认）；L2 compat job 须显式 `compat` |
| `XUGU_REQUIRE_DATABASE` | 不可达时 **失败**（非 Skip） | L2/L3 gate 设为 `true` |

## 本地命令

```powershell
# L1（无库）
harness/scripts/run-unit-gate.ps1 -Configuration Release

# L2 native 全量 → compat 全量（需实库）
harness/scripts/run-native-gate.ps1 -Configuration Release -MaxAttempts 3
harness/scripts/run-compat-gate.ps1 -Configuration Release -MaxAttempts 3

# 或 verify -RunTests（L1 + L2 双方言）
harness/scripts/verify.ps1 -RunTests

# L3 实测（需实库 + dotnet-ef）
harness/scripts/run-experiential-gate.ps1 -Configuration Release
```

## 项目内测试类型

### L1 Unit

- SQL 金标：`NativeSqlBaselineTests` + `Baselines/Native/*`
- TypeMapping / Migration SQL / TranslatorSql / UpdateSqlGenerator
- NotSupported 消息矩阵：`NotSupportedMessageTests`
- Options / Retry 检测 / API consistency

### L2 Integration

- 实库 CRUD / Northwind Query / Migrations / JSON / 并发 token / Spec 子集
- `SkipIfUnavailable`：仅在 **未** 设置 `XUGU_REQUIRE_DATABASE` 时 Skip；L2 gate 强制失败
- 串行：`DisableTestParallelization = true`

### L3 Experiential

1. `publish-nuget.ps1 -Pack`
2. 临时目录 PackageReference 消费 nupkg + `CanConnect`
3. `samples/EfDesignSample`：`dotnet ef migrations list` + `database update`
4. `test/integration-sample` MinimalApi HTTP 冒烟

## CI

| Job | GitHub Actions | GitLab |
|-----|----------------|--------|
| L1 | 所有 PR / push | `l1-unit` |
| L2 native → compat | `main` push、schedule、`v*` tag | 同规则 |
| L3 | schedule、`v*` tag | 同规则 |
| pack | `v*` tag | `v*` tag |

Windows-only 实库（Linux RID：PLAT-02 blocked）。

## 三类绿验收（Phase 13 — Translator / 应用路径）

新 Translator、DbFunction 映射或驱动适配修复，必须同时满足三类证据，**禁止**仅 AssertSql 即宣称 done：

| 类型 | 含义 | 典型证据 |
|------|------|----------|
| **1. SQL 形状** | 生成 SQL 符合 Xugu 文档与 `sql-dialect.contract.md` | Unit `AssertSql` / NativeSqlBaseline |
| **2. 服务端执行** | SQL 在实库可执行（无语法/函数错误） | Integration 查询跑通 |
| **3. 客户端物化** | CLR 值正确（`GetInt32`/`GetString`/Converter 等） | 断言返回值；`RuntimeGap` / `AppCapabilityMatrix` |

审核漏测复盘：仅有 SQL 形状绿、物化失败（如 `E34412`）仍算缺口。门禁：

| 脚本 | 覆盖 |
|------|------|
| `run-runtime-gap-gate.ps1` | A1–A4/B1/C1（Category=`RuntimeGap`） |
| `run-app-matrix-gate.ps1` | 应用矩阵（Category=`AppCapabilityMatrix`；native+compat） |

二者均设 `XUGU_REQUIRE_DATABASE=true`（库不可达 = FAIL，禁止 Skip 计绿）。CI L2 在全量 Integration 前必跑 RuntimeGap + AppMatrix。

## 参考

- 限制：[LIMITATIONS.md](LIMITATIONS.md)
- 驱动契约：`harness/contracts/ado-driver-contract.md`
- Skill：`harness/skills/provider-testing/SKILL.md`
- 旧单体项目已退役：见 `test/EFCore.Xugu.Tests/README.md`
