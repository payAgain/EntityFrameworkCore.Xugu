# Implement â€?Republish GitHub 9.0.0

1. [ ] Confirm `Version.props` = 9.0.0 and `v9.0.0^{}` == HEAD
2. [ ] `dotnet pack ... -p:UseLocalXuguDriver=false -o artifacts/`
3. [ ] Write release notes (Trellis included)
4. [ ] `gh release delete v9.0.0 --yes` if exists; `gh release create v9.0.0 ...`
5. [ ] Verify `gh release view v9.0.0` shows assets
6. [ ] Archive this Trellis task
