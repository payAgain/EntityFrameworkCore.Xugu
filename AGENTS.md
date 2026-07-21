<!-- TRELLIS:START -->
# Trellis Instructions

These instructions are for AI assistants working in this project.

This project is managed by Trellis. The working knowledge you need lives under `.trellis/`:

- `.trellis/workflow.md` — development phases, when to create tasks, skill routing
- `.trellis/spec/` — package- and layer-scoped coding guidelines (read before writing code in a given layer)
- `.trellis/workspace/` — per-developer journals and session traces
- `.trellis/tasks/` — active and archived tasks (PRDs, research, jsonl context)

If a Trellis command is available on your platform (e.g. `/trellis:finish-work`, `/trellis:continue`), prefer it over manual steps. Not every platform exposes every command.

If you're using Codex or another agent-capable tool, additional project-scoped helpers may live in:
- `.agents/skills/` — reusable Trellis skills
- `.codex/agents/` — optional custom subagents

Managed by Trellis. Edits outside this block are preserved; edits inside may be overwritten by a future `trellis update`.

<!-- TRELLIS:END -->

# XuguDB EF Core — project rules (preserved)

Trellis manages workflow under `.trellis/`. Dialect and engineering hard rules live in:

- [.trellis/spec/guides/xugu-provider-constraints.md](.trellis/spec/guides/xugu-provider-constraints.md)
- [docs/contracts/sql-dialect.contract.md](docs/contracts/sql-dialect.contract.md)
- [docs/contracts/stub-and-exclusion.contract.md](docs/contracts/stub-and-exclusion.contract.md)
- [docs/references/xugudb-docs-map.md](docs/references/xugudb-docs-map.md)

Local verification: `scripts/verify.ps1` / `scripts/verify-module.ps1`.
