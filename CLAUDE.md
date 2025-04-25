# CLAUDE.md

Instructions for Claude Code when working with this repository.

## Build Commands
- Build: `cmd.exe /c dotnet build`
- Run: `cmd.exe /c dotnet run --project SearchitBreakher`
- Test: `cmd.exe /c dotnet test SearchitTest/SearchitTest.csproj`
- Package: `cmd.exe /c dotnet mgcb-editor`
- Clean: `cmd.exe /c dotnet clean`

## Linting Process
1. Static analysis: `cmd.exe /c dotnet build /p:TreatWarningsAsErrors=true`
2. Style check: `cmd.exe /c dotnet format`
3. Run both before submitting changes
4. Fix any style/formatting issues flagged

## Development Workflow
1. **Analysis**: 
   - Take tasks from todo.txt
   - Translate them into formal requirements in Features.txt
   - Ensure requirements are clear and testable
   - Anything described below the ------- marker is ONLY for context, and should not be implemented yet

2. **Test Writing**:
   - Create unit tests following strict TDD approach
   - Use Arrange/Act/Assert pattern consistently
   - Use fluent assertions (`Assert.That`) where possible
   - Test each requirement separately

3. **Implementation**:
   - Write minimal code to make tests pass
   - Only implement what's described in Features.txt
   - Remove functionality not in Features.txt
   - Add missing functionality described in Features.txt

4. **Refactoring**:
   - Improve code without changing behavior
   - Apply best practices and Code Style guidelines
   - Extract repeated patterns into reusable components

5. **Quality Check**:
   - Run linting process (see Linting Process section)
   - Fix any style/formatting issues
   - Ensure all tests still pass

6. **Finalization**:
   - Commit changes with descriptive message

## Code Style
- Naming:
  - Private fields: camelCase with underscore (`_graphics`)
  - Public: PascalCase for properties, methods, classes, fields
- Structure:
  - Namespaces: Alphabetical, System namespaces first
  - Methods: Focused, under 50 lines, performs 1 task, low complexity
  - Lines: Under 100 characters
  - Braces: New lines, use for all control structures
- Best Practices:
  - Explicit access modifiers always
  - Use `var` when type is obvious
  - Use `readonly` for immutable fields
  - Follow MonoGame conventions
  - Apply SOLID principles
  - Minimize changes to necessary ones only
