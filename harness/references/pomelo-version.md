# Pomelo 参考版本（固定）

| 项 | 值 |
|----|-----|
| 仓库 | https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql |
| **Release** | **[9.0.0](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/releases/tag/9.0.0)** |
| Git tag | `9.0.0` |
| Commit | `58dd688` (Update branding to 9.0.0-rtm.1) |
| 本地路径 | `E:\Work\xuguefcore\external\Pomelo.EntityFrameworkCore.MySql` |
| EF Core 依赖 | `[9.0.0,9.0.999]`（Pomelo 9.0.0 Directory.Packages.props） |
| 我们的 EF Core NuGet | `9.0.0`（`Directory.Packages.props`） |

## 切换命令

```powershell
git -C external/Pomelo.EntityFrameworkCore.MySql fetch --tags
git -C external/Pomelo.EntityFrameworkCore.MySql checkout 9.0.0
```

## 注意

- 参考源码 **只读**，不修改 Pomelo 仓库
- 架构对齐 Pomelo 9.0.0 的目录与 DI 注册模式
- SQL 方言以 XuguDB 官方文档为准，**不**照搬 MySQL 语法（如 IDENTITY vs AUTO_INCREMENT）
