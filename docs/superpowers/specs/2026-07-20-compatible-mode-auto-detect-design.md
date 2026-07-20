# 兼容模式自动探测与标识符适配 — 设计规格

> **状态**：已审阅待实现  
> **日期**：2026-07-20  
> **依据**：虚谷官方文档 `compatible_mode.md` / `def_compatible_mode.md` / `identifier.md`；项目方案 1（探测 + 原生 SQL 不变）

## 1. 背景与结论

虚谷 `COMPATIBLE_MODE`（`NONE` / `ORACLE` / `MYSQL` / `POSTGRESQL`）是**异构库部分使用习惯**兼容，官方明确影响主要为：

- 未加引号标识符的大小写折叠
- 默认别名规则、少数函数/类型边缘行为（见 `def_compatible_mode` 表）

**不是**完整 MySQL / Oracle / PostgreSQL SQL 方言引擎。

当前 EF Provider（3.x）默认生成 **Xugu 原生 SQL**，`EnableCompatibleModeOnOpen` 仅会话 `SET`，SQL 生成不按模式分支。

**本设计采用方案 1**：连接打开后自动获知有效 `compatible_mode`，完整适配**标识符/引号/编译缓存**；**不**按模式切换 DDL/DML/函数方言。

## 2. 目标与非目标

### 2.1 目标

- 连接打开后自动获知有效 `compatible_mode`（显式配置优先，否则探测会话）。
- Provider 的标识符/引号策略与编译查询缓存键与有效模式一致，用户无需手写分支。
- API / 文档明确：这是「会话兼容感知」，不是「方言切换」。

### 2.2 非目标

- 不按模式生成 MySQL / Oracle / PostgreSQL 风格 DDL、DML、函数 SQL。
- 不实现 Oracle / PostgreSQL SQL Translator；不承诺零改动异构迁移。
- 不自动改写应用手工 SQL。
- 不把 `def_group_by_mode` / `IDENTITY_MODE` 绑进本方案（可另开任务）。

### 2.3 成功标准

- 服务器 `def_compatible_mode=MYSQL`、应用未调用 `EnableCompatibleModeOnOpen` 时：Provider 探测到 MYSQL，缓存键与标识符策略正确。
- 显式 `EnableCompatibleModeOnOpen(Oracle)` 时：以显式为准（SET + SHOW 校验）。
- 单元/集成覆盖探测优先级、各模式 SET/探测；SQL 方言金标不因模式漂移（仅引号字符可变）。

## 3. 探测时序与优先级

### 3.1 优先级（高 → 低）

| 优先级 | 来源 | 行为 |
|--------|------|------|
| 1 | `EnableCompatibleModeOnOpen(mode)` 且 `mode ≠ None` | Open 时 `SET`；有效模式 = 显式值；仍 `SHOW` 校验，不一致则失败 |
| 2 | 显式 `None`（默认）+ AutoDetect 开启（默认开） | 不 `SET`；Open 后强制 `SHOW`；有效模式 = 会话实际值 |
| 3 | 显式关闭 AutoDetect | 不 `SET`、不 `SHOW`；有效模式固定 `None`（旧「无视服务器」行为） |

### 3.2 强制解析管线

引入 `IXuguCompatibleModeResolver`（与 DbContext / 连接生命周期一致）：

```
EnsureResolvedAsync()
  1. 若已解析 → 返回 EffectiveMode
  2. 确保底层连接 Open
  3. 若 Explicit ≠ None → SET … → SHOW → 断言一致
  4. 若 Explicit = None 且 AutoDetect → SHOW → 写入 EffectiveMode
  5. 标记 Resolved = true
```

**全部挂载点（无遗漏）：**

- `XuguRelationalConnection.Open` / `OpenAsync`
- `XuguCompiledQueryCacheKeyGenerator`（生成 key **前** `EnsureResolved`）
- `XuguSqlGenerationHelper` / 标识符策略（首次 `DelimitIdentifier` 前确保已解析）
- Migrations / Scaffolding / Update 路径随 connection Open 覆盖
- 设计时 `dotnet ef`：工厂建连后同样走 Open → 解析

### 3.3 编译早于 Open

- **禁止**在未解析时用占位模式生成可缓存 SQL。
- Cache key / SQL 生成入口统一 `EnsureResolved`；必要时同步打开连接并 `SHOW`。
- 解析结果**按连接**缓存：每个新物理连接必须各自走完 SET（若需）+ `SHOW`，不得跨连接复用旧 SHOW 结果。
- 同一连接上多次 `EnsureResolved` 直接返回已解析的 `EffectiveMode`。
- 服务器在线改 `def_compatible_mode` 不影响已有连接（官方语义）；后续新建连接重新 `SHOW`。

### 3.4 失败策略

| 情况 | 行为 |
|------|------|
| `SHOW` 失败 / 无法解析 | **抛**明确异常（`.resx`），不静默回退 |
| 显式 SET 后 SHOW 不一致 | **抛**配置冲突异常 |
| 返回值不在 `{NONE,ORACLE,MYSQL,POSTGRESQL}` | **抛**不支持值异常 |

### 3.5 不变式

有效模式**只**驱动标识符策略、引号形态、缓存键；**不**分支 `UpdateSqlGenerator` / Migrations / Query translators 的 SQL 方言文本。

## 4. 标识符完整适配

依据 `identifier.md`：引号可用 `"` 或 `` ` ``；未加引号按模式折叠；加引号区分大小写。

| EffectiveMode | 折叠（未加引号语义） | Provider 分隔符 | 模型名写入 SQL |
|---------------|---------------------|-----------------|----------------|
| **NONE** | → 大写 | 双引号 `"` | 始终 `Delimit`；规范化按大写 |
| **ORACLE** | → 大写 | 双引号 `"` | 同上 |
| **MYSQL** | 不折叠 | 反引号 `` ` `` | 始终 `Delimit`；保留模型大小写 |
| **POSTGRESQL** | → 小写 | 双引号 `"` | 始终 `Delimit`；规范化按小写 |

- **一律加引号输出**，避免未加引号被服务器二次折叠。
- `XuguSqlGenerationHelper` 按 `EffectiveMode` 选择开闭引号并实现转义。
- Scaffolding / Migrations / Update / Query **共用同一 Helper**。
- SQL 方言文本不变：`IDENTITY`、`LAST_INSERT_ID`、`SYS_GUID`、`DBA_*` 等。

## 5. API

```csharp
// 显式指定（优先级最高）→ SET + SHOW 校验
options.UseXugu(cs, x => x.EnableCompatibleModeOnOpen(XuguCompatibleMode.Mysql));

// 默认：不 SET，Open 后 SHOW 自动探测
options.UseXugu(cs); // AutoDetectEffectiveCompatibleMode = true

// 高级：关闭探测（旧行为 / 无库单元测试）
options.UseXugu(cs, x => x.DisableCompatibleModeAutoDetect());
```

| API | 含义 |
|-----|------|
| `EnableCompatibleModeOnOpen(XuguCompatibleMode)` | 显式 SET；`None` 表示不 SET |
| `EnableCompatibleModeAutoDetect(bool = true)` | 默认 true；仅当未显式 SET 时 SHOW |
| `DisableCompatibleModeAutoDetect()` | 等价关闭 AutoDetect |
| （只读）`IXuguCompatibleModeResolver.EffectiveMode` | 解析完成后的有效模式 |

保留 `SetCompatibleModeOnOpen` / `DisableCompatibleModeOnOpen` 为转发兼容别名。

### 5.1 缓存与选项哈希

- `XuguCompiledQueryCacheKeyGenerator`：key = 关系键 + ServerVersion + **EffectiveMode**（解析后）。
- `XuguOptionsExtension` 信息哈希：含 `CompatibleModeOnOpen` + `AutoDetect` 标志（不含探测结果；探测结果在连接/解析器侧）。

### 5.2 可观测性

- 解析成功：可选 Debug 级记录 EffectiveMode。
- 冲突/失败：用户可见异常走 `.resx`。

## 6. 测试矩阵

| # | 场景 | 断言 |
|---|------|------|
| T1 | 默认 + 服务器 NONE | 不 SET；SHOW=NONE；分隔符 `"`；缓存键含 None |
| T2 | 默认 + 服务器 MYSQL | 不 SET；Effective=Mysql；分隔符 `` ` `` |
| T3 | 默认 + 服务器 ORACLE / POSTGRESQL | Effective 正确；`"`；大小写规范化按矩阵 |
| T4 | 显式 Mysql/Oracle/Postgresql | SET 后 SHOW 一致；策略匹配 |
| T5 | 显式与 SHOW 不一致 | 抛配置冲突 |
| T6 | `DisableCompatibleModeAutoDetect` | 不 SHOW；Effective=None；引号按 None |
| T7 | SHOW 失败 | 抛解析失败，不静默回退 |
| T8 | SQL 金标（Update/Migrations/Query） | 四种 EffectiveMode 下方言文本不变（仅引号可变） |
| T9 | CompiledQuery | 不同 EffectiveMode 不串缓存 |
| T10 | 编译早于首次业务查询 | `EnsureResolved` 会 Open+SHOW，无占位 key |
| T11 | 设计时 / Scaffolding | 同一 Helper；引号随 EffectiveMode |
| T12 | 现有布尔 `EnableCompatibleModeOnOpen()` | 与显式 Mysql 一致 |

层级：单元（SET SQL、Helper 引号、Resolver 优先级）+ 集成（实库 SHOW/SET；Category 含 Native + CompatibleModeApi）。

## 7. 风险与迁移

| 风险 | 对策 |
|------|------|
| 首次查询多一次 Round-trip（SHOW） | 每连接一次；文档说明；可 opt-out AutoDetect |
| 引号从全局反引号改为按模式切换 | 金标按模式参数化；CI 矩阵扩展 |
| 对象未加引号大写创建、模型小写 | 一律 Delimit + 规范化；USER-GUIDE 命名建议 |
| 被误解为完整方言适配 | LIMITATIONS / USER-GUIDE / CHANGELOG 醒目标注 |

**对现有用户：**

- 默认将探测服务器模式并按矩阵选择 `"` 或 `` ` ``。
- 依赖「永远反引号」：显式 `EnableCompatibleModeOnOpen(Mysql)`，或评估 `DisableCompatibleModeAutoDetect()`。
- 已调用 `EnableCompatibleModeOnOpen()`：增强为 SET + SHOW 校验，语义兼容。
- 文档：`USER-GUIDE`、`XUGU-VS-MYSQL`、`LIMITATIONS`、`sql-dialect.contract.md` 同步更新。

## 8. 实现交付物

1. `IXuguCompatibleModeResolver` + 连接 Open 钩子  
2. 模式感知的 `XuguSqlGenerationHelper`  
3. Options / API + `.resx` 错误字符串  
4. 测试 T1–T12  
5. 契约与用户文档更新  

## 9. 官方文档索引

| 文档 | 路径（`E:\Work\docs\content\`） |
|------|----------------------------------|
| 会话兼容模式 | `reference/system-configuration-parameter/session-parameter/compatible_mode.md` |
| 系统默认兼容模式 | `reference/system-configuration-parameter/xugu.ini/compatible/def_compatible_mode.md` |
| 标识符 | `reference/sql/identifier.md` |
| 默认别名 | `reference/sql/default_alias.md` |
| 自增填充（本方案范围外） | `session-parameter/identity_mode.md` |
| GROUP BY 模式（本方案范围外） | `xugu.ini/compatible/def_group_by_mode.md` |

## 10. 批准记录

| 节 | 用户确认 |
|----|----------|
| 方案 1 | 认可 |
| §1 目标与非目标 | 可以 |
| §2 完整探测时序 | 可以（拒绝简化落地） |
| §3 标识符矩阵 + API | 可以 |
| §4 测试与迁移 | 可以 |
