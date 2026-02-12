# bamtest

Cross-project test discovery and execution tool for the bam toolkit. Discovers test projects from a solution file or directory, runs them out-of-process via `dotnet run`, streams output in real-time, and prints an aggregate summary.

## Installation

Build from source:

```bash
dotnet build submodules/bamtest/bamtest/bamtest.csproj
```

To produce a standalone executable:

```bash
dotnet publish submodules/bamtest/bamtest/bamtest.csproj -c Release -o ~/.bam/tools/bamtest
```

This outputs a `bamtest.exe` (Windows) or `bamtest` (Linux/macOS) that can be run directly.

## Usage

### From a Compiled Executable

```bash
# Run all unit tests (auto-discovers .sln in current directory)
bamtest --ut

# Run all unit tests from a specific solution
bamtest --ut --sln=bamtk.sln

# Run all unit tests from projects under a directory
bamtest --ut --dir=submodules/bam.test/

# Run all spec tests
bamtest --spec

# Run all integration tests
bamtest --it

# Run tests from pre-built assemblies
bamtest --ut --assemblyDir=~/.bam/build/Debug/

# Interactive menu
bamtest
```

### From Source (dotnet run)

```bash
# Run all unit tests (auto-discovers .sln in current directory)
dotnet run --project submodules/bamtest/bamtest/bamtest.csproj -- --ut

# Run all unit tests from a specific solution
dotnet run --project submodules/bamtest/bamtest/bamtest.csproj -- --ut --sln=bamtk.sln

# Run all unit tests from projects under a directory
dotnet run --project submodules/bamtest/bamtest/bamtest.csproj -- --ut --dir=submodules/bam.test/

# Run all spec tests
dotnet run --project submodules/bamtest/bamtest/bamtest.csproj -- --spec

# Run all integration tests
dotnet run --project submodules/bamtest/bamtest/bamtest.csproj -- --it

# Run tests from pre-built assemblies
dotnet run --project submodules/bamtest/bamtest/bamtest.csproj -- --ut --assemblyDir=~/.bam/build/Debug/
```

Note: when using `dotnet run`, arguments after `--` are passed to bamtest.

### Interactive Menu

Run without test switches to enter the interactive menu:

```bash
bamtest
# or
dotnet run --project submodules/bamtest/bamtest/bamtest.csproj
```

This presents a menu:

```
bamtest options

Select an option below:
  1. Run All Unit Tests
  2. Run Unit Test
  3. Run All Spec Tests
  4. Run Spec Test
  5. Run All Integration Tests
  6. Run Integration Test
```

"Run All" options discover and run every test project. "Run" options (without "All") prompt you to select a specific project from the discovered list.

## Code Coverage

bamtest integrates with [`dotnet-coverage`](https://learn.microsoft.com/en-us/dotnet/core/additional-tools/dotnet-coverage) to collect code coverage for any test run. Because bam.test is a custom test runner (not xUnit/NUnit), the standard `dotnet test --collect` workflow does not apply; `dotnet-coverage collect` is used instead, which can instrument any .NET process.

### Prerequisites

Install the `dotnet-coverage` global tool:

```bash
dotnet tool install --global dotnet-coverage
```

### Single Project (bam.test)

When running a single test project with `--coverage`, bam.test re-launches the current process under `dotnet-coverage collect`. The `--coverage` flag is stripped from the child process args to prevent recursion.

```bash
# Collect coverage for a single test project
dotnet run --project submodules/bam.console/bam.console.tests/bam.console.tests.csproj -- --ut --coverage

# Custom output file
dotnet run --project submodules/bam.console/bam.console.tests/bam.console.tests.csproj -- --ut --coverage --coverage-output=myreport.xml

# Custom format
dotnet run --project submodules/bam.console/bam.console.tests/bam.console.tests.csproj -- --ut --coverage --coverage-format=xml
```

This produces a `coverage.cobertura.xml` (or the specified filename) in the working directory.

### Multi-Project (bamtest)

When running across multiple projects with `--coverage`, bamtest wraps each project's execution in `dotnet-coverage collect`, producing a separate coverage file per project.

```bash
# Coverage for all discovered test projects
bamtest --ut --coverage --sln=bamtk.sln

# Via dotnet run
dotnet run --project submodules/bamtest/bamtest/bamtest.csproj -- --ut --coverage --sln=bamtk.sln
```

Each project gets a file named `{projectName}.coverage.cobertura.xml`. The aggregate summary includes coverage file locations:

```
========================================
  bamtest Aggregate Summary
========================================
  [PASS] bam.base.tests - 12 passed, 0 failed (4.2s)
  [PASS] bam.console.tests - 5 passed, 0 failed (2.1s)
----------------------------------------
  Total: 17 passed, 0 failed across 2 project(s)
  Result: ALL PASSED
----------------------------------------
  Coverage reports:
    bam.base.tests: C:\src\repos\bamtk\bam.base.tests.coverage.cobertura.xml
    bam.console.tests: C:\src\repos\bamtk\bam.console.tests.coverage.cobertura.xml
========================================
```

## Arguments

| Argument | Description |
|---|---|
| `--ut` | Run unit tests |
| `--it` | Run integration tests |
| `--spec` | Run specification tests |
| `--sln=<path>` | Path to a `.sln` file for test project discovery |
| `--dir=<path>` | Directory to scan recursively for `*.tests.csproj` files |
| `--assemblyDir=<path>` | Directory to scan for pre-built `*tests.dll` assemblies |
| `--coverage` | Enable code coverage collection via `dotnet-coverage` |
| `--coverage-output=<path>` | Coverage output file path (default: `coverage.cobertura.xml`) |
| `--coverage-format=<fmt>` | Coverage output format (default: `cobertura`) |

Arguments use `=` to separate name and value (e.g., `--sln=bamtk.sln`).

## Test Discovery

bamtest supports three discovery strategies:

1. **Solution file** (`--sln=<path>`) -- Parses the `.sln` file for project entries matching `*.tests.csproj`.
2. **Directory scan** (`--dir=<path>`) -- Recursively searches the directory for `*.tests.csproj` files.
3. **Auto-discover** (no `--sln` or `--dir`) -- Looks for a `.sln` file in the current directory; falls back to a recursive directory scan.
4. **Assembly scan** (`--assemblyDir=<path>`) -- Scans for pre-built `*tests.dll` and `*tests.exe` files.

## Test Execution

Each discovered test project is run out-of-process:

- **Projects**: `dotnet run --project <csproj> -- --ut` (or `--it`, `--spec`)
- **Assemblies**: `dotnet <assembly.dll> --ut` (or `--it`, `--spec`)

Output from each project is streamed to the console in real-time. After all projects complete, an aggregate summary is printed:

```
========================================
  bamtest Aggregate Summary
========================================
  [PASS] bam.base.tests - 12 passed, 0 failed (4.2s)
  [PASS] bam.console.tests - 5 passed, 0 failed (2.1s)
  [FAIL] bam.data.objects.tests - 8 passed, 1 failed (3.7s)
----------------------------------------
  Total: 25 passed, 1 failed across 3 project(s)
  Result: FAILURES DETECTED
========================================
```

When `--coverage` is enabled, the summary also lists the coverage report paths (see [Code Coverage](#code-coverage)).

The process exits with code 1 if any tests fail.
