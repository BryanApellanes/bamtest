# bamtest

Multi-project test runner CLI tool that discovers, executes, and aggregates test results across Bam Framework test projects, with optional code coverage support.

## Overview

bamtest is an executable command-line tool that orchestrates running bam.test-based test suites across multiple projects or pre-built assemblies. Unlike individual test projects that run their own tests in-process, bamtest discovers test projects (by parsing `.sln` files or scanning directories for `*.tests.csproj`), runs each one as a separate `dotnet run` process, captures and parses their output for pass/fail counts, and produces an aggregate summary. This makes it the top-level test orchestrator for the Bam Framework monorepo.

bamtest supports three discovery modes: solution-based (via `--sln` argument), directory-based (via `--dir` argument), and auto-discovery (finds a `.sln` in the current directory or falls back to directory scan). It can also run pre-built test assemblies directly via `--assemblyDir`. The tool supports all three test types via the standard switches: `--ut` (unit tests), `--it` (integration tests), and `--spec` (specification tests).

Code coverage collection is integrated via `dotnet-coverage`. When `--coverage` is specified, bamtest wraps each test project execution in a `dotnet-coverage collect` invocation, generates per-project coverage reports, and then merges them into a single aggregate report. Coverage format (default: cobertura), output path, and module include patterns are all configurable. The tool overrides the default `ITestSwitchExecutor` from bam.test with `BamTestSwitchExecutor`, which performs the multi-project out-of-process execution rather than running tests in the current assembly.

## Key Classes

| Class | Description |
|---|---|
| `BamTestRunner` | Core orchestrator. Discovers projects/assemblies, runs them via `TestProjectRunner`/`TestAssemblyRunner`, collects results into `BamTestSummary`, and optionally merges coverage reports. |
| `BamTestSwitchExecutor` | Custom `ITestSwitchExecutor` that intercepts `--ut`/`--it`/`--spec` switches and runs tests across multiple projects out-of-process instead of in the current assembly. |
| `TestProjectDiscovery` | Discovers `*.tests.csproj` files from a `.sln` file (by regex parsing), from a directory (recursive search), or via auto-discovery. |
| `TestAssemblyDiscovery` | Discovers pre-built test assemblies (`*tests.dll`, `*tests.exe`) from a directory. |
| `TestProjectRunner` | Runs a single test project via `dotnet run --project <csproj> -- --<switch>` as a child process. Streams output in real-time and parses the `Test Summary:` line for pass/fail counts. Optionally wraps in `dotnet-coverage collect`. |
| `TestAssemblyRunner` | Runs a pre-built test assembly via `dotnet <assembly.dll> --<switch>` as a child process. Same streaming and parsing behavior as `TestProjectRunner`. |
| `TestProjectResult` | Result from running a single project/assembly: project path, name, exit code, passed/failed counts, duration, and optional coverage output path. |
| `BamTestSummary` | Aggregate summary: list of `TestProjectResult`, total passed/failed, `AllPassed` flag, merged coverage path. Prints a formatted summary table. |
| `ConsoleCommands` | Interactive menu container providing `[ConsoleCommand]` and `[MenuItem]` entries for running unit, spec, and integration tests from the interactive menu. |

## Dependencies

**Project References:**
- `bam.base` -- Core framework primitives, DI, string extensions
- `bam.test` -- Test framework (CoverageOptions, test attributes)

**Package References:**
- `FluentAssertions` 4.17.0
- `NSubstitute` 4.2.2
- `PuppeteerSharp` 3.0.0
- `SharpZipLib` 1.4.2

**Target Framework:** net10.0
**Output Type:** Exe (NuGet-packable CLI tool, v2.0.0)

## Usage Examples

### Run all unit tests across all projects (auto-discover)
```bash
dotnet run --project bamtest.csproj -- --ut
```

### Run unit tests for a specific solution
```bash
dotnet run --project bamtest.csproj -- --ut --sln=path/to/solution.sln
```

### Run integration tests from a directory
```bash
dotnet run --project bamtest.csproj -- --it --dir=path/to/projects
```

### Run unit tests from pre-built assemblies
```bash
dotnet run --project bamtest.csproj -- --ut --assemblyDir=path/to/build/output
```

### Run with code coverage
```bash
dotnet run --project bamtest.csproj -- --ut --coverage --coverage-output=coverage.cobertura.xml --coverage-format=cobertura
```

### Run interactively (menu-driven)
```bash
dotnet run --project bamtest.csproj -- --i
```

### Example output
```
--- Running bam.console.tests (--ut) ---
...
--- bam.console.tests: [PASS] 12 passed, 0 failed (3.2s) ---

========================================
  bamtest Aggregate Summary
========================================
  [PASS] bam.console.tests - 12 passed, 0 failed (3.2s)
  [PASS] bam.shell.tests - 1 passed, 0 failed (2.1s)
----------------------------------------
  Total: 13 passed, 0 failed across 2 project(s)
  Result: ALL PASSED
========================================
```

## Known Gaps / Not Yet Implemented

None identified. The tool provides complete end-to-end functionality for multi-project test discovery, execution, aggregation, and coverage merging. The excluded files (`CommandLineTestTool.cs`, `Program_bak.cs`, `Arguments.cs`, `SettingsConsoleActions.cs`, `ConsoleMenu.cs`, `SemanticAssemblyInfo.cs`) are legacy code no longer in use.
