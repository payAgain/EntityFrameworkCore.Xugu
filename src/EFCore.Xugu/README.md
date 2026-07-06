# Microsoft.EntityFrameworkCore.Xugu

Entity Framework Core database provider for [XuguDB](https://www.xugudb.com/) (虚谷数据库).

## Installation

```shell
dotnet add package Microsoft.EntityFrameworkCore.Xugu
```

## Usage

```csharp
optionsBuilder.UseXugu(
    "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8",
    XuguServerVersion.Default);
```

## Native dependency

The package includes `xugusql.dll` under `runtimes/win-x64/native/`. The ADO.NET driver (`Xuguclient`) must be able to locate this native library at runtime.

## Documentation

See the project repository for dialect notes, migration guidance, and development setup.
