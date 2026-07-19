# AppCapabilityMatrix

Phase **13.101** 应用能力矩阵样例：覆盖审核关键路径的独立物化验收。

## 覆盖路径

| # | 能力 | 说明 |
|---|------|------|
| 1 | Count | `CountAsync` → Int32 |
| 2 | 投影 Count | 导航/`Select` 内 Count |
| 3 | DateDiff 投影 | `DateDiffYear` 标量 |
| 4 | DateTimeOffset | 非零偏移往返 |
| 5 | DateOnly / TimeOnly | SaveChanges + 物化 |
| 6 | Include DATE | Include 含 DATE 列 |
| 7 | 事务 + DML | BeginTransaction + SaveChanges |

自动化门禁（推荐）：

```powershell
$env:XUGU_REQUIRE_DATABASE = "true"
# 可选：$env:XUGU_CONNECTION_STRING = "IP=192.168.2.216; ..."
harness/scripts/run-app-matrix-gate.ps1
```

等价集成测：`Category=AppCapabilityMatrix`（`test/EFCore.Xugu.Tests.Integration/AppCapabilityMatrixTests.cs`）。

## 运行本样例

```powershell
$env:XUGU_CONNECTION_STRING = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
dotnet run --project samples/AppCapabilityMatrix -c Release
```

退出码：`0` = 全部路径 PASS；非 0 = 失败（库不可达亦失败，禁止假绿）。
