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

## Arguments

| Argument | Description |
|---|---|
| `--ut` | Run unit tests |
| `--it` | Run integration tests |
| `--spec` | Run specification tests |
| `--sln=<path>` | Path to a `.sln` file for test project discovery |
| `--dir=<path>` | Directory to scan recursively for `*.tests.csproj` files |
| `--assemblyDir=<path>` | Directory to scan for pre-built `*tests.dll` assemblies |

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

The process exits with code 1 if any tests fail.
