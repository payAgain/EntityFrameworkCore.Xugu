### Task 7: Documentation Wave A alignment

**Files:**
- Modify: `docs/RELEASE-SCOPE.md`
- Modify: `docs/LIMITATIONS.md`
- Modify: `docs/CHANGELOG.md`
- Modify as needed: `docs/USER-GUIDE.md` / packaging notes for native dll vs `Xuguclient`

- [ ] **Step 1: RELEASE-SCOPE**

Add a clear **9.0.0 Wave A acceptance** section:
- Windows trialable bar: Unit 0, Integration (report 13) 0, core user paths
- Functional: APPLY/LATERAL skipped; full Comparable Set **not** claimed 0 FAIL
- Remove or qualify any checked claim that implies full Functional matrix already green for 9.0.0

- [ ] **Step 2: LIMITATIONS / USER-GUIDE**

- Native: nuspec depends on `Xuguclient`; embedded win-x64 `xugusql.dll` may be overridden by package assets 鈥?document
- Linux: 3.3.6-bionic may ship `.so`; Wave A **does not** claim Linux verified
- APPLY/LATERAL: rejected; tests skipped
- E18012 / catalog privilege note if Task 4 left an env requirement

- [ ] **Step 3: CHANGELOG**

Entry under 9.0.0 (overwrite): TimeOnly Unit contract, Description UTF-8, Integration prefix/E18012/Northwind isolation, APPLY Skip hygiene, Wave A acceptance.

- [ ] **Step 4: Optional commit**

```text
docs: align RELEASE-SCOPE with Wave A acceptance gate
```

---

