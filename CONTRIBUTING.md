# Contributing to HM STH Companion

Thanks for your interest in contributing. This project follows a strict, issue-driven workflow. Please read this document before opening an issue or pull request.

## Ground rules

- **Issue-driven:** every change (feature, bug fix, docs, chore) must be tied to a GitHub issue. Bugs and feature requests can be reported by anyone using the issue templates.
- **GitHub Flow:** every task is developed on a separate branch created from `main`, named `<type>/<short-name>` (for example `feature/memory-reader`, `fix/pointer-offset`).
- **No direct pushes to `main`:** all changes land through pull requests.
- **Conventional Commits:** each commit contains exactly one logical change, with messages like `feat: add memory reader skeleton`. Types: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`, `perf`, `build`, `ci`, `revert`.
- **Public-safe:** never commit `.env` files, tokens, secrets, internal endpoints, or personal data. Temporary work belongs in `temp/`, which is gitignored.
- **Text hygiene:** files use `LF` line endings and `UTF-8` without BOM; the CI `build` job enforces this for Markdown. Do not hardwrap paragraph text in Markdown documents — let lines run continuously; line breaks are only for table rows, bullet points, and code blocks.

## Development workflow

1. Pick or open an issue and comment that you are taking it.
2. Create a branch from `main`.
3. Make your change in small, atomic commits following the commit style above.
4. Open a pull request into `main` and fill in the PR checklist.
5. Wait for the CI `build` check to pass, then wait for review.

## Verification before every pull request

- `dotnet build` succeeds.
- `dotnet format --verify-no-changes` passes.
- `dotnet test` is green (once tests exist).

## Merge strategy

The maintainer chooses the merge method per pull request:

| Pull request source | Method |
|---------------------|--------|
| External contributor (including bots) | Squash merge |
| Collaborator branch with one author | Rebase merge |
| Shared branch (2+ collaborators) | Merge commit |

## Reporting security issues

Please do not report security vulnerabilities through public issues. See [SECURITY.md](SECURITY.md) for the responsible disclosure process.

## Licensing

By contributing you agree that your contributions are licensed under the [MIT License](LICENSE).
