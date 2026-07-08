# Phase 11 Wave 对齐校正 Handoff — W3–5 偏差修复轨

> **日期**：2026-07-08  
> **状态**：`done`  
> **触发**：用户更新 `ROADMAP.md` 插入 W3–5 偏差修复轨；子文档仍沿用旧 W3–W6 编号

---

## 漂移摘要

| 漂移项 | 错误状态 | 校正后（以 ROADMAP 为准） |
|--------|----------|---------------------------|
| Wave 编号 | BACKLOG 将 NuGet/发布标为 **W3** | **W6**（11.301–11.303）；建议 W5 后进入 |
| Wave 编号 | ConnectionString/集成/文档标为 **W4** | **W7**（11.208, 11.304, 11.305） |
| Wave 编号 | FunctionalTests 标为 **W5** | **W8**（11.401–11.403） |
| Wave 编号 | 可选驱动轨标为 **W6** | **W9**（11.105, 11.205 等） |
| 缺失轨 | BACKLOG 无 W3–5 任务 | 新增 **11.501–11.705** 偏差修复轨 |
| RELEASE-SCOPE | 无 native CI 门禁 | 增补 W4–5 dual matrix 要求 |
| PACKAGING | `test-nuget-pack`「W3 起」 | 改为 **W6 起** |
| W2 指针 | BACKLOG「W2 待开工」 | **W2 进行中**（11.109a done，867 列测） |
| 11.109 位置 | 无漂移 | 正确位于 **W2**；非 premature W3 工作 |

**未改 ROADMAP 用户意图**：W1 done → W2 进行中 → W3–5 todo → W6+ 发布轨。

---

## 校正文件

| 文件 | 变更 |
|------|------|
| `harness/tasks/BACKLOG.md` | W3–9 重映射；P0 增偏差修复轨；统计 867 列测 |
| `harness/tasks/phase-11-xugu-native-release/PACKAGING-AND-INTEGRATION.md` | W6 发布门禁；W8 测试深化 |
| `docs/RELEASE-SCOPE.md` | native CI 子集；2.1.0 含偏差修复轨 |
| `harness/tasks/phase-11-xugu-native-release/TASKS.md` | 11.301 依赖 W5；Release Gate W8 标注 |
| `harness/tasks/ROADMAP.md` | 进度日志：文档对齐 + W2 进行中 |

---

## 当前 Wave 指针

```
W1 done ✅ → W2 进行中（11.109a done；11.109b–d todo）
         → W3–5 偏差修复轨 todo（NATIVE-DIALECT-ROADMAP.md）
         → W6+ 发布轨 todo
```

**下一工作**：11.109b JSON 路径/函数 LINQ 翻译（W2 关键路径）。

**W3 入口条件**：W2 done（11.109d 实库 PASS）。

---

## 验证

```powershell
harness/scripts/verify.ps1   # PASS（仅文档变更）
```

无 `src/` 代码变更；11.109a 实现已在 commit `597e69e`。

---

## 参考

- `harness/tasks/ROADMAP.md` — Wave W1–W9 权威
- `harness/tasks/phase-11-xugu-native-release/NATIVE-DIALECT-ROADMAP.md` — W3–5 详情
- `harness/handoffs/phase11-wave2-2026-07-08.in_progress.md` — W2 进度
