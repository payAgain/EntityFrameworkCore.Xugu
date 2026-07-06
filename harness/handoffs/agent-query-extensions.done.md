# Agent-Query Extensions Handoff (Phase 3 扩展项)

**Agent**: Agent-QueryExtensions  
**Status**: Phase 3 扩展项完成  
**Date**: 2026-07-06

## 任务

Phase 3 扩展（非阻塞）：

1. Convert Translator
2. DateTimeOffset 时区相关
3. TimeOnly / DateOnly 完整覆盖
4. AssertSql 单元测试

## XuguDB 文档依据

- `E:\BaiduSyncdisk\docs\content\reference\sql\expression\type_conversion.md` — CAST()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\systimestamp.md` — SYSTIMESTAMP()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\utc_timestamp.md` — UTC_TIMESTAMP()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\timestampdiff.md` — TIMESTAMPDIFF()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\time.md` — TIME()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\make_timestamp.md` — MAKE_TIMESTAMP()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\to_days.md` — TO_DAYS()
- `E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\timestampadd.md` — TIMESTAMPADD()

## 变更文件

| 文件 | 说明 |
|------|------|
| `Query/ExpressionTranslators/Internal/XuguConvertTranslator.cs` | Convert.To* → CAST |
| `Query/ExpressionTranslators/Internal/XuguDateTimeMemberTranslator.cs` | TimeOnly 成员、DateOnly.DayNumber、DateTimeOffset.Now/UtcNow |
| `Query/ExpressionTranslators/Internal/XuguDateTimeMethodTranslator.cs` | TimeOnly/DateOnly/DateTimeOffset UnixTime 方法 |
| `Query/Internal/XuguMethodCallTranslatorProvider.cs` | 注册 ConvertTranslator |
| `test/EFCore.Xugu.Tests/TranslatorSqlTests.cs` | AssertSql 单元测试（9 项） |
| `test/EFCore.Xugu.Tests/ExtensionQueryTests.cs` | 扩展集成测试（4 项） |
| `test/EFCore.Xugu.Tests/Fixtures/XuguDatabaseFixture.cs` | 新测试表 + 原始 SQL 种子 |
| `harness/contracts/sql-dialect.contract.md` | 扩展函数映射与差异 |
| `harness/tasks/phase-3-query/TASKS.md` | 扩展项状态更新 |

## MySQL vs Xugu 新发现差异

| 场景 | Pomelo/MySQL | XuguDB | 处理 |
|------|-------------|--------|------|
| Convert.To* | `CONVERT(x, type)` | `CAST(x AS type)` | XuguConvertTranslator + QuerySqlGenerator |
| DateTimeOffset.Now | `UTC_TIMESTAMP()` | `SYSTIMESTAMP()` | DateTimeMemberTranslator |
| DateTimeOffset.LocalDateTime | `CONVERT_TZ(..., @@session.time_zone)` | **无 CONVERT_TZ 函数** | 不翻译（客户端求值） |
| DateOnly.ToDateTime(t) | `ADDTIME(CAST(d AS datetime), t)` | `MAKE_TIMESTAMP(Y,M,D,h,m,s)` | DateTimeMethodTranslator |
| TimeOnly.AddHours | `DATE_ADD` | `TIME(TIMESTAMPADD(HOUR,n,CAST(t AS DATETIME)))` | DateTimeMethodTranslator |
| DateTimeOffset.ToUnixTime* | `TIMESTAMPDIFF` | `TIMESTAMPDIFF`（参数顺序一致） | DateTimeMethodTranslator |

## 验收结果

```powershell
dotnet build Xugu.EFCore.Xugu.sln          # PASS
dotnet test test/EFCore.Xugu.Tests         # 29/29 PASS
verify.ps1                                 # PASS
```

| 测试类 | 结果 |
|--------|------|
| **全量** | **29/29 通过** |
| TranslatorSqlTests (9) | CAST / TIMESTAMPADD / DATE / SYS_GUID / LENGTH / TIME / MAKE_TIMESTAMP / SYSTIMESTAMP / UTC_TIMESTAMP |
| ExtensionQueryTests (4) | Convert / TimeOnly / DateOnly / DateTimeOffset |
| DateTimeQueryTests (4) | Year / AddDays / Date / Month+Day |
| QueryTests (4) | Where / OrderBy / Skip+Take / Count |
| CrudTests (3) | Insert / Update / Delete |
| CanConnectTests (2) | 连接验证 |
| MigrationTests (2) | Migrate / AddColumn |
| 其他 (1) | — |

## 未完成项

| 项 | 原因 |
|----|------|
| DateTimeOffset.LocalDateTime SQL 翻译 | XuguDB 文档无 CONVERT_TZ / AT TIME ZONE 等价函数 |
| DateTimeOffset 参数化写入（SaveChanges） | ADO.NET 驱动尚未支持 DateTimeOffset 参数绑定（集成测试用原始 SQL 种子） |
| DateOnly/TimeOnly 参数化写入 | 同上，驱动层待 Phase 2+ 完善 |
