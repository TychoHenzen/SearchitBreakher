# CLAUDE.md

Instructions for Claude Code when working with this repository.

## Build Commands
- Build: `cmd.exe /c dotnet build`
- Run: `cmd.exe /c dotnet run --project SearchitBreakher`
- Test: `cmd.exe /c dotnet test SearchitTest/SearchitTest.csproj`
- Package: `cmd.exe /c dotnet mgcb-editor`
- Clean: `cmd.exe /c dotnet clean`

## Development Workflow
- Follow strict TDD: Write tests FIRST before implementing features
- Test structure: Use Arrange/Act/Assert pattern consistently
- Use fluent assertions (`Assert.That`) where possible
- Features: Only implement what's in `SearchitBreakher/features.txt`
- Remove functionality not in `SearchitBreakher/features.txt`
- Add missing functionality described in `SearchitBreakher/features.txt`

## Code Style
- Naming:
  - Private fields: camelCase with underscore (`_graphics`)
  - Public: PascalCase for properties, methods, classes, fields
- Structure:
  - Namespaces: Alphabetical, System namespaces first
  - Methods: Focused, under 50 lines
  - Lines: Under 100 characters
  - Braces: New lines, use for all control structures
- Best Practices:
  - Explicit access modifiers always
  - Use `var` when type is obvious
  - Use `readonly` for immutable fields
  - Follow MonoGame conventions
  - Apply SOLID principles
  - Minimize changes to necessary ones only