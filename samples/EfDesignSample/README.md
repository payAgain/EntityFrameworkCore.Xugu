# EfDesignSample — dotnet ef 端到端验证

消费项目，用于验证 `dotnet ef migrations add` / `dbcontext scaffold`。

## 前置

- 本机 XuguDB 可连接（默认 `127.0.0.1:5138`）
- 已安装 `dotnet-ef` 工具：`dotnet tool install --global dotnet-ef`

## migrations add

```powershell
cd E:\Work\xuguefcore\samples\EfDesignSample
$env:XUGU_CONNECTION = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
dotnet ef migrations add InitialCreate --project . --startup-project .
dotnet ef database update --project . --startup-project .
```

## dbcontext scaffold（骨架）

```powershell
dotnet ef dbcontext scaffold $env:XUGU_CONNECTION Microsoft.EntityFrameworkCore.Xugu `
  --project . --startup-project . --context ScaffoldedDbContext --table YOUR_TABLE
```

> Scaffolding 当前读取 `DBA_TABLES` / `DBA_COLUMNS`；主键/索引/FK 映射待 Phase 4 扩展。
