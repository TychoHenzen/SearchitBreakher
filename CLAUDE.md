# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands
- Build project: `dotnet build`
- Run project: `dotnet run --project SearchitBreakher`
- Package content: `dotnet mgcb-editor`
- Clean solution: `dotnet clean`

## Code Style Guidelines
- Use camelCase for private fields with underscore prefix (e.g., `_graphics`)
- Use PascalCase for properties, methods, classes, and public fields
- Organize namespaces alphabetically, with System namespaces first
- Keep methods focused and under 50 lines where possible
- Prefer explicit access modifiers (public, private, etc.)
- Use var for local variables when type is obvious from assignment
- Keep lines under 100 characters when possible
- Use braces for all control structures, even single-line statements
- Place braces on new lines
- Follow MonoGame conventions for game loop methods and content loading
- Use readonly for fields that don't change after initialization