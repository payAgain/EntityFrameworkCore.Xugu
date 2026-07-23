# Fix v9.0.0 release acceptance gaps

## Goal

根据 `E:\Work\Tests\entityframeworkcore-xugu-release-test\TEST-REPORT.md`，把发布定位推进为：**Windows 上核心 EF 用户主路径可试用，且 Wave A 约定门禁通过**（非完整 Functional 0 FAIL）。

## Decisions

- **验收线：A — 快速可试用门禁**（2026-07-23）
- **Integration 其余失败：C — EF_TS + E18012 + Northwind 重音本波都修到绿**（2026-07-23）
- **完工验收：C — 重跑独立验收套件对应门禁后，覆写 GitHub Release `v9.0.0`**（2026-07-23）
- **Functional Skip：A — 仅 Skip `ApplyNotSupported` / LATERAL 簇**（2026-07-23）

## Background / confirmed facts

报告对象：`v9.0.0` @ `8f31715`，Release NuGet 黑盒。结论原为 Reject。

| 层级 | 报告 | 仓库核实 |
|------|------|----------|
| Unit 6 FAIL | 全 `TypeMappingSourceTests` TimeOnly/temporal | 测试期望默认 `TIME(3)` / precision facet / `.fff`；实现见 `XuguTemporalValueConverters`、`XuguTimeOnlyTypeMapping` — 与自身 Unit 不一致 |
| Integration 3 | 期望 `EF_TS_*` | `XuguTestStoreFactory` 已 SHA1；`XuguTestStoreTests` / `NorthwindSeedDataTests` 仍断言旧前缀 — **测试过时** |
| Integration 5 migration | `E18012 权限不够` | 稳定失败；环境权限 vs Provider 路径待定；**本波要求修到绿** |
| Integration 5 Northwind 重音 | 全量红、隔离绿 | 不稳定；**本波要求修到绿**（隔离/fixture/编码） |
| Functional ~650 | APPLY 238 等 | Provider 已 `ApplyNotSupported`；本波仅 Skip 该簇 |
| 发布包 | description 乱码等 | csproj `<Description>` 已确认乱码 |

核心用户路径（CRUD/Include/常用 LINQ/Bulk/事务/并发/CLI/Minimal API）报告通过。

## Requirements（Wave A）

| ID | 要求 |
|----|------|
| R1 | 修复 Unit 6：TimeOnly/temporal 映射与 `TypeMappingSourceTests` 对齐；改实现时须有 Xugu 文档/驱动依据 |
| R2 | 修复发布包 description 乱码（`EFCore.Xugu.csproj`） |
| R3 | 文档：native dll / `Xuguclient`、Linux、`RELEASE-SCOPE` 与 Wave A 门禁一致 |
| R4 | Integration：`EF_TS_*` 断言改为与 SHA1 工厂一致 |
| R5 | Integration：migration `E18012` 修到绿（须根因说明） |
| R6 | Integration：Northwind 重音修到绿（优先隔离/fixture/编码） |
| R7 | Functional：仅对 `ApplyNotSupported` / LATERAL 相关用例补 Skip |
| R8 | 本机 Unit+Integration 通过 → 重跑独立套件 Wave A 门禁 → 覆写 GitHub `v9.0.0` |

## Acceptance criteria

- [ ] Unit Release：**0 FAIL**
- [ ] Pack description **无乱码**
- [ ] Integration：**原 13 FAIL → 0 FAIL**
- [ ] Functional：APPLY/LATERAL 簇已 Skip，不再以未 Skip FAIL 计入拒收
- [ ] 独立套件重跑通过 Wave A 门禁
- [ ] GitHub Release `v9.0.0` 已覆写
- [ ] `RELEASE-SCOPE` / `LIMITATIONS` 与 Wave A 一致
- [ ] 不要求 Functional 全量 0 FAIL

## Out of scope

- Functional Comparable Set 双模式 0 FAIL
- 大规模 LINQ 翻译 / E19132 / E17010
- Linux 生产可用宣称
- nuget.org 上架
