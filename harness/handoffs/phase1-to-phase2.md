## Handoff: Phase 1 → Phase 2

**任务 ID**: phase-1/complete  
**状态**: done  
**Orchestrator**: 2026-07-06

### XuguDB 文档依据

- `E:\BaiduSyncdisk\docs\content\ecosystem\orm\dotnet\efcore.md`
- `E:\BaiduSyncdisk\docs\content\reference\system-configuration-parameter\session-parameter\compatible_mode.md`
- `E:\BaiduSyncdisk\docs\content\reference\system-configuration-parameter\xugu.ini\compatible\def_identity_mode.md`

### Phase 1 交付物

| 模块 | 文件 |
|------|------|
| Infrastructure | `XuguOptionsExtension`, `UseXugu()`, `XuguDbContextOptionsBuilder`, `XuguOptions` |
| Storage | `XuguRelationalConnection`, `XuguSqlGenerationHelper`, `XuguTypeMappingSource`, `XuguDatabaseCreator` |
| DI | `AddEntityFrameworkXugu()`（Phase 1 最小注册） |
| 测试 | `CanConnectTests`（2 通过） |

### 下游接口（Phase 2 可依赖）

- `IXuguOptions` / `XuguOptions` — ServerVersion、SetCompatibleModeOnOpen
- `XuguSqlGenerationHelper` — 反引号标识符（MYSQL 兼容模式）
- `XuguTypeMappingSource` — 基础 CLR 类型映射
- 连接打开后：`SET compatible_mode TO 'MYSQL'`

### Phase 2 并行任务

| Agent | 任务文件 | 验收 |
|-------|---------|------|
| Agent-Metadata | `harness/tasks/phase-2-metadata-update/TASKS.md` §Metadata | 模型约定 + 注解 |
| Agent-Update | `harness/tasks/phase-2-metadata-update/TASKS.md` §Update | SaveChanges INSERT/UPDATE/DELETE |

### Pomelo 参考

- **版本**: tag `9.0.0`（见 `harness/references/pomelo-version.md`）
- Metadata: `external/Pomelo.../Metadata/`
- Update: `external/Pomelo.../Update/`, `ValueGeneration/`

### 约束

- **不要**编辑 `XuguServiceCollectionExtensions.cs`（Orchestrator 合并 DI 注册）
- SQL 自增用 XuguDB `IDENTITY(1,1)`，非 `AUTO_INCREMENT`
