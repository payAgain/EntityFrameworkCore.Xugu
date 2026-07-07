# 测试稳定性笔记（Phase 9 收口）

> **日期**：2026-07-07  
> **来源**：Phase 9 全量实库调试（根目录 `test-run*.txt` / `test-out*.txt` 已清理）  
> **权威 handoff**：`phase9-m3-test-parity-2026-07-07.md`

## 最终门禁

| 指标 | 值 |
|------|-----|
| 列测总数 | **676** |
| 通过 | **671** |
| 显式 Skip | **5** |
| 失败 | **0** |
| 耗时（全量顺序） | ~38 s（加固后） |

```powershell
dotnet test test/EFCore.Xugu.Tests -c Release
# 已通过! — 失败: 0，通过: 671，已跳过: 5，总计: 676
```

## 调试期观察到的问题（已缓解）

| 现象 | 典型错误 | 根因 | 处置 |
|------|---------|------|------|
| 实库长跑偶发连接失败 | `[E34305]: InValidConnectionException` | 并行 Open + 驱动瞬态 | `XuguRelationalConnection` 8 次重试 + 全局 Semaphore 串行化 Open |
| 测试 DDL 争用 | 同上 / `Net Error recv_noQuery` | 多测试同时建表/清表 | `XuguTestConnection.OpenConnection()` 同模式；`maxParallelThreads: 1` |
| Skip 雪崩 | 大量 SkippableFact skip（48+） | `IsAvailable()` 探测失败连锁 | 轻量 2 次探测 + 3s 缓存 |
| `SeedingTests` 单测失败 | `read-optimized model` / `GetSeedData` | EF Core 设计时模型 API 用法 | 测试已修正或标 defer（EnsureCreated+HasData） |
| `ManyToManyTrackingTests` 间歇失败 | `[E34501]: CommandExecuteException` | 连接/DDL 与上同 | 稳定性加固后纳入全绿 |

## 显式 Skip（5 条，冻结）

| 测试 | 原因 |
|------|------|
| `LazyLoadTests.Lazy_loading_proxies_not_supported_in_harness` | 无 lazy proxy 宿主 |
| `OptimisticConcurrencyTests.Stale_concurrency_token_throws_DbUpdateConcurrencyException` | ROW_COUNT / affected rows defer |
| `WithConstructorsTests` ×2 | 构造函数图 insert defer |
| `ComplexTypesTrackingTests.Nullable_complex_property_can_be_null` | optional complex defer（EF #31376） |

## 运维建议

1. 全量实库测试前确认 XuguDB 可达：`harness/scripts/start-xugudb.ps1`
2. 勿提高 xUnit 并行度；实库套件设计为 **顺序执行**
3. 无实库环境：SkippableFact 跳过集成测试属 **预期**，非失败
4. 覆盖率分母：`dotnet test --list-tests` → 676；Pomelo 可比 ~64%（÷1050）

## 参考

- `harness/references/test-parity-matrix.md`
- `docs/TESTING.md`
- `docs/LIMITATIONS.md`（ROW_COUNT、Retry defer）
