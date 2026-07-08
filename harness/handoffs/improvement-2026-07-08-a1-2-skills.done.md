# Improvement 2026-07-08 — A1-2 9 个 SKILL.md status 字段更新

> **状态**：done  
> **日期**：2026-07-08  
> **范围**：`harness/skills/provider-*/SKILL.md`（9 个模块）

## Before

9 个 SKILL.md 的 status/当前 Phase 字段停留在 Phase 9 测试对等：

| SKILL | 旧 status |
|-------|-----------|
| provider-extensions | `...当前 Phase 9。` |
| provider-update | `...当前 Phase 9。` |
| provider-query | `...当前 Phase 9 测试对等。` |
| provider-metadata | `...当前 Phase 9。` |
| provider-migrations | `...当前 Phase 9 测试移植。` |
| provider-storage | `...当前 Phase 9。` |
| provider-infrastructure | `...当前 Phase 9。` |
| provider-query-translators | `...当前 Phase 9 测试移植。` |
| provider-testing | `## 当前 Phase` → `**Phase 9** — Pomelo 测试对等...` |

## After

所有 9 个 SKILL.md 的当前 Phase 引用统一更新到 **Phase 10 维护与剩余对等**；各模块 Phase 8 完成描述与 defer 项保留不变。

| SKILL | 新 status |
|-------|-----------|
| provider-extensions | `...当前 Phase 10。` |
| provider-update | `...当前 Phase 10。` |
| provider-query | `...当前 Phase 10 维护与剩余对等。` |
| provider-metadata | `...当前 Phase 10。` |
| provider-migrations | `...当前 Phase 10 维护与剩余对等。` |
| provider-storage | `...当前 Phase 10。` |
| provider-infrastructure | `...当前 Phase 10。` |
| provider-query-translators | `...当前 Phase 10 维护与剩余对等。` |
| provider-testing | `## 当前 Phase` → `**Phase 10** — 维护与剩余对等（Wave 1/2/3 done；Monster/Specification 子集 + Query +119；850 列测）。` |

## Diff 摘要

- 9 个 SKILL.md 各替换 1 行 status
- 历史段落（Phase 9 基础设施交付记录、Phase 8 完成清单、defer 项）原样保留
- `provider-testing/SKILL.md:42` 的 "Phase 9：对齐 Pomelo MySqlTestStore" 是历史文件描述，保留

## 校验

- `grep "当前 Phase" harness/skills/**/SKILL.md` → 9 处全部为 Phase 10 ✓
- 未触及 `src/`、`external/` ✓
