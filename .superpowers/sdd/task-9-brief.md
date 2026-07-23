### Task 9: Independent acceptance suite + overwrite `v9.0.0`

**Files / paths:**
- Suite: `E:\Work\Tests\entityframeworkcore-xugu-release-test\`
- Artifacts: packed nupkg/snupkg from this repo
- GitHub: `payAgain/EntityFrameworkCore.Xugu` Release/tag `v9.0.0`

- [ ] **Step 1: Feed suite with new build**

Follow that suite鈥檚 README/scripts to point at the freshly packed 9.0.0 (or source tip equivalent to Release contents). Prefer the same black-box path the original TEST-REPORT used.

- [ ] **Step 2: Run Wave A gate**

Must show:
- Unit: 0 FAIL (former 6 fixed)
- Integration: former 13 鈫?0 FAIL
- Functional: APPLY/LATERAL cluster not counted as rejection (Skipped); remaining Functional FAIL do **not** block Wave A
- Pack description clean

Update or append `TEST-REPORT.md` with Wave A Pass (or keep evidence under `test-output/`).

- [ ] **Step 3: Overwrite GitHub Release** (only after Step 2 Pass)

```powershell
# Pack
dotnet pack "E:\Work\C#\xuguefcore\src\EFCore.Xugu\EFCore.Xugu.csproj" -c Release -p:UseLocalXuguDriver=false -o "E:\Work\C#\xuguefcore\artifacts"

# Move annotated tag to release tip, push, recreate Release assets
# (use gh release delete/create or upload 鈥?match prior overwrite process for 9.0.0)
```

Do **not** publish to nuget.org.

- [ ] **Step 4: Mark Trellis task ready for archive** after evidence attached; commit/push only if user requests.

---

## Plan self-review

| Spec requirement | Task |
|------------------|------|
| R1 Unit TimeOnly | Task 1 |
| R2 Description | Task 2 |
| R3 Docs / native / Linux / RELEASE-SCOPE | Task 7 (+ optional native note in Task 2) |
| R4 EF_TS | Task 3 |
| R5 E18012 | Task 4 |
| R6 Northwind accents | Task 5 |
| R7 APPLY Skip only | Task 6 |
| R8 Independent suite + overwrite v9.0.0 | Tasks 8鈥? |

No TBD placeholders. Version stays 9.0.0. Commits gated on user request.
