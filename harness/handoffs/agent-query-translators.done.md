# Agent-Query Handoff (Phase 3 — DateTime Translators + QuerySqlGenerator CAST)

**Agent**: Agent-QueryTranslators  
**Status**: Phase 3 核心目标达成（DateTime + Guid + CAST）；扩展项待办  
**Date**: 2026-07-06

## 任务

Phase 3.T2（DateTime Translators）+ 3.Q1 CAST 扩展 + Guid.NewGuid + DateTimeQueryTests

## XuguDB 文档依据

- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\year.md` — YEAR()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\timestampadd.md` — TIMESTAMPADD()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\date.md` — DATE()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\current_timestamp.md` — CURRENT_TIMESTAMP()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\utc_timestamp.md` — UTC_TIMESTAMP()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\curdate.md` — CURDATE()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\dayofyear.md` — DAYOFYEAR()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\dayofweek.md` — DAYOFWEEK()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\microsecond.md` — MICROSECOND()
- `E:\BaiduSyncdisk\docs\content\reference\function\uuid-functions\sys_guid.md` — SYS_GUID()
- `E:\BaiduSyncdisk\docs\content\reference\sql\expression\type_conversion.md` — CAST()

## 变更文件

| 文件 | 说明 |
|------|------|
| `Query/ExpressionTranslators/Internal/XuguDateTimeMemberTranslator.cs` | Year/Month/Day/Date/Now 等 |
| `Query/ExpressionTranslators/Internal/XuguDateTimeMethodTranslator.cs` | AddDays/AddMonths 等 → TIMESTAMPADD |
| `Query/ExpressionTranslators/Internal/XuguNewGuidTranslator.cs` | Guid.NewGuid → SYS_GUID |
| `Query/Internal/XuguMethodCallTranslatorProvider.cs` | 注册 DateTime/NewGuid |
| `Query/Internal/XuguMemberTranslatorProvider.cs` | 注册 DateTime Member |
| `Query/Internal/XuguQuerySqlGenerator.cs` | CAST 类型转换生成 |
| `test/EFCore.Xugu.Tests/DateTimeQueryTests.cs` | DateTime LINQ 集成测试 |
| `test/EFCore.Xugu.Tests/Fixtures/XuguDatabaseFixture.cs` | EF_TEST_EVENTS 表 |
| `harness/contracts/sql-dialect.contract.md` | DateTime/Guid/CAST 差异 |
| `harness/tasks/phase-3-query/TASKS.md` | 任务状态更新 |

## MySQL vs Xugu 新发现差异

| 场景 | Pomelo/MySQL | XuguDB |
|------|-------------|--------|
| DateTime.Add* | `DATE_ADD(dt, INTERVAL n unit)` | `TIMESTAMPADD(unit, n, dt)` |
| DateTime.Date | `CONVERT(dt, date)` | `DATE(dt)` |
| DateTime 部分提取 | `EXTRACT(part FROM dt)` | 独立函数 `YEAR()`/`MONTH()` 等 |
| Guid.NewGuid | `UUID()` | `SYS_GUID()` |
| CAST | MySQL 专用映射表 | 标准 `CAST(expr AS type)` |

## 验收结果

```powershell
dotnet build Xugu.EFCore.Xugu.sln          # PASS
dotnet test test/EFCore.Xugu.Tests         # 13/13 PASS
verify.ps1                                 # PASS（dotnet 不在 PATH 时跳过 build）
```

| 测试类 | 结果 |
|--------|------|
| **全量** | **13/13 通过** |
| DateTimeQueryTests (4) | Year / AddDays / Date / Month+Day |
| QueryTests (4) | Where / OrderBy / Skip+Take / Count |
| CrudTests (3) | Insert / Update / Delete |
| CanConnectTests (2) | 连接验证 |

## 剩余待办（Phase 3 扩展）

| 项 | 状态 |
|----|------|
| Convert Translator | pending |
| DateTimeOffset 时区（LocalDateTime 等） | pending |
| TimeOnly 完整支持 | pending |
| AssertSql 单元测试 | pending |
