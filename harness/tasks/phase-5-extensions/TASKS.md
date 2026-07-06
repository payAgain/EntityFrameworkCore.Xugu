# Phase 5：Extensions + 高级 Fluent API



> 状态：`done`  

> 负责 Agent：Agent-Extensions



## 目标



完善 XuguDB 专用 Fluent API，对齐 Pomelo Extensions 子集（IDENTITY、模型级约定）。



## 任务



| ID | 描述 | 状态 |

|----|------|------|

| 5.E1 | `UseIdentityColumns()` 模型级 IDENTITY 约定 | done |

| 5.E2 | `UseXuguIdentityColumn()` 属性级 Fluent API | done |

| 5.E3 | `XuguAnnotationCodeGenerator` 脚手架代码生成 | done |

| 5.E4 | EntityType / Index 扩展（Collation N/A — Xugu 无表级 charset） | done |

| 5.E5 | `XuguDbContextOptionsBuilder` 高级选项 | done |



## 验收



- [x] `dotnet build` 通过

- [x] 公共 Fluent API 可用于 Migration 消费项目

- [x] `FluentApiExtensionTests` 通过



## 文档依据



- `reference/object/table/create.md` — IDENTITY

- `reference/system-view/all/all_indexes.md` — INDEX_TYPE

- `reference/system-configuration-parameter/xugu.ini/compatible/def_identity_mode.md`



## 并行说明



- 与 Phase 4 Scaffolding **可并行**（不同目录：`Extensions/` vs `Scaffolding/`）

- **不可**与 Orchestrator 同时改 `XuguServiceCollectionExtensions.cs`（本 Phase 无 DI 变更）
