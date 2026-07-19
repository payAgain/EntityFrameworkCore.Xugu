# Phase 13.401 — W4 需求门控签字

**日期**：2026-07-19  
**来源**：用户明确要求执行 Phase 13 **全部 Wave W1→W4**，并将本指令视为 **13.401 需求门控签字**。

## 客户场景（书面）

- 需要会话级 `SET compatible_mode` API，覆盖文档取值：`NONE` / `MYSQL` / `ORACLE` / `POSTGRESQL`
- 需要标识符折叠行为与官方表一致的测试/断言
- **只要**会话兼容模式 + 标识符策略；**不**实现 Oracle / PostgreSQL SQL 方言翻译
- **不**承诺异构库零改动迁移

## 范围边界

| 做 | 不做 |
|----|------|
| `EnableCompatibleModeOnOpen(XuguCompatibleMode)` | Oracle/PG SQL Translator |
| 连接 Open 时 `SET compatible_mode TO '…'` | 伪装 MySQL/Oracle/PG 迁移完成 |
| 实库/单元标识符与 SHOW 断言 | 改写 EF 生成 SQL 为 Oracle/PG 语法 |

签字生效 → 开启 13.402–13.405。
