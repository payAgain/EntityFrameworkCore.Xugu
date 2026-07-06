# Microsoft.EntityFrameworkCore.Xugu

Entity Framework Core database provider for [XuguDB](https://www.xugudb.com/) (虚谷数据库).

## Installation

```shell
dotnet add package Microsoft.EntityFrameworkCore.Xugu
```

## Usage

```csharp
optionsBuilder.UseXugu(
    "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8");
```

With provider-specific options (MySQL compatible mode is enabled on connection open by default):

```csharp
optionsBuilder.UseXugu(
    connectionString,
    xugu => xugu.SetCompatibleModeOnOpen());
```

## Migrations

Requires `Microsoft.EntityFrameworkCore.Design` and an `IDesignTimeDbContextFactory` (or startup project). See repository docs:

- [docs/GETTING-STARTED.md](../../docs/GETTING-STARTED.md) — connection string, `dotnet ef`, troubleshooting
- [docs/LIMITATIONS.md](../../docs/LIMITATIONS.md) — known limitations and deferred features

## Native dependency

The package includes `xugusql.dll` under `runtimes/win-x64/native/` when built with native assets. The ADO.NET driver (`Xuguclient`) must be able to locate this native library at runtime.

Driver reference strategy (local vs NuGet): [docs/xuguclient-dependency-strategy.md](../../docs/xuguclient-dependency-strategy.md).

## Documentation

| Document | Description |
|----------|-------------|
| [GETTING-STARTED.md](../../docs/GETTING-STARTED.md) | Quick start |
| [LIMITATIONS.md](../../docs/LIMITATIONS.md) | Known limitations |
| [xuguclient-dependency-strategy.md](../../docs/xuguclient-dependency-strategy.md) | `Xuguclient` dependency policy |
