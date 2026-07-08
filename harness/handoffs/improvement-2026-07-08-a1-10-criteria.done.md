# Improvement 2026-07-08 — A1-10 新建 RELEASE-2.0.0-CRITERIA.md

> **状态**：done  
> **日期**：2026-07-08  
> **范围**：`harness/verification/RELEASE-2.0.0-CRITERIA.md`（新建）

## Before

- 仅存在 `RELEASE-1.0.0-CRITERIA.md`（141 测试 / Phase 7 验收 / 10 大类）与 `RELEASE-1.0.0-REPORT.md`
- 无 2.0.0 验证矩阵
- 1.0.0 矩阵的 defer 项仅 L1–L6，未反映 Phase 9/10 新增 defer（ROW_COUNT、optional complex、LazyLoad、Linux RID、10.201–10.204）
- 1.0.0 矩阵无来源血缘门禁（T7）、无覆盖率类目（1.11）、无 CI 矩阵检查（S5）、无 Monster/Specification 端到端（E8）

## After

新建 `RELEASE-2.0.0-CRITERIA.md`，10 大类 + 1 个新增占位类目（覆盖率）：

| 类目 | 1.0.0 ID | 2.0.0 ID | 说明 |
|------|----------|----------|------|
| 1.1 构建与包 | B1–B7 | B1–B7 | 版本号 1.0.0 → 2.0.0；linux RID defer 至 10.205 |
| 1.2 自动化测试 | T1–T6 | T1–T7 | 新增 T7 来源血缘；T5 关键路径扩展 Monster/Specification/Interception/ConvertToProvider；T6 基线 141 → 850 |
| 1.3 核心能力冒烟 | C1–C7 | C1–C12 | 新增 C8 Translators / C9 Monster / C10 Specification / C11 SaveChangesInterception / C12 ConvertToProvider |
| 1.4 文档完备性 | D1–D6 | D1–D8 | 新增 D5 TESTING.md / D6 XUGU-VS-MYSQL.md；D7 README.md 2.0.0 |
| 1.5 Harness 一致性 | H1–H4 | H1–H8 | 新增 H4 BACKLOG / H5 SKILL.md / H6 contract / H8 handoff 链 |
| 1.6 已知限制 | L1–L6 | L1–L11 | 新增 L7 ROW_COUNT / L8 optional complex / L9 LazyLoad / L10 Linux RID / L11 10.201–10.204 |
| 1.7 安全与发布 | S1–S4 | S1–S5 | 新增 S5 CI 矩阵 |
| 1.8 Phase 验收 | Phase 7 P0 | Phase 9 M1/M2/M3 + Phase 10 Wave 1/2/3 | 10.001–10.005 / 10.101–10.104 必须 done |
| 1.9 实库集成 | R1–R6 | R1–R7 | 新增 R7 连接稳定性 |
| 1.10 EF 集成 | E1–E7 | E1–E8 | 新增 E8 Monster/Specification 端到端 |
| 1.11 覆盖率（新增） | — | CV1–CV4 | Pomelo 可比覆盖率 ≥80%（当前 ~81%） |

### 判定标准升级

- **PASS**：12 条（1.0.0 为 10 条），新增 T7、T6 ≥850、CV1–CV4、Phase 10 Wave 1/2/3 done
- **CONDITIONAL PASS**：扩展风险项清单（L7–L11 + E7 Wave 4–6）
- **FAIL**：新增 `verify-source-lineage.ps1` 失败、列测 < 850、Phase 10 Wave 1/2/3 未完成

## Diff 摘要

- 新建文件 213 行
- 10 大类保持（1.1–1.10）+ 1 个新增占位类目（1.11 覆盖率）
- §5 新增"与 1.0.0 矩阵的差异说明"表，逐维度对比

## 校验

- 10 大类保持 ✓（构建/Tests/能力冒烟/文档/Harness/defer/安全/EF 集成/实库集成/覆盖率占位）
- 测试基线 141 → 850 ✓
- defer 项移除已解决（CREATE/DROP DATABASE 保留，Collation/FULLTEXT 保留），新增 Phase 9/10 defer ✓
- 与 ROADMAP / BACKLOG / TASKS / CHANGELOG 状态一致 ✓
- 未触及 `src/`、`external/` ✓
