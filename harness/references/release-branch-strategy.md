# Release branch strategy (pointer)

完整策略文档见 **[docs/RELEASE-BRANCH-STRATEGY.md](../../docs/RELEASE-BRANCH-STRATEGY.md)**。

| 分支 | 用途 |
|------|------|
| `phase-8/feature-parity` | 开发单体（harness + external 子模块） |
| `release/3.0.0` | GA 发布线（自 `v3.0.0`） |

脚本：`harness/scripts/prepare-release-branch.ps1`（`-Mirror` 生成公开最小树）。
