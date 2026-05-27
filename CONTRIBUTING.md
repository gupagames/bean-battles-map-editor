# Contributing

Thanks for your interest in contributing!
No direct pushes to main. All changes go through PR please.

## Flow
1. Open an issue describing what you want to change or add
2. Wait for the issue to be approved before starting work
3. Fork the repo and make your changes
4. Open a PR referencing the issue. I(Duck) review and approve all merges

## Commits
Commits must be detailed and end with an emoji for fun, e.g:
- Updated setup instructions in README 📝
- Fixed Steam build pipeline for Mac users 🔧
- Cleaned up MapSettings logic ♻️

## Backwards Compatibility
All changes must be backwards compatible:
- Only add new fields with sensible default values
- Never rename or remove existing fields, mark them `[Obsolete]` instead and leave them in place
- Existing Bean Battles saves/data must never break

## Versioning
Update the version in [`package.json`](./Assets/GG/MapEditor/package.json) with every PR:
- **Major (1.x)** — changes that require updates in Bean Battles itself
- **Minor (x.1)** — localized editor-only changes, no Bean Battles update needed