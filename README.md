# ZipExtension

`ZipExtension` is a lightweight, testable C# library for working with ZIP archives using `System.IO.Abstractions`. It supports reading, extracting, and creating ZIP archives with attention to cross-platform compatibility and testability.

---

## Features

-  Open and read existing `.zip` files
-  Extract archive contents to disk
-  Create ZIP archives from directories (async)
-  Cross-platform path handling

---

## Installation

Install the required NuGet packages:
System.IO.Abstractions

## Unit Testing
This project includes comprehensive unit tests covering:

- Valid/invalid ZIP files
- Path validation (null, whitespace, invalid chars, too long)
- Archive extraction to various file system conditions
- Asynchronous ZIP creation, including:
- Empty directories
- Deep hierarchies
- Hidden and read-only files
- Files added/removed during runtime
- Symbolic links
- Special characters and long paths
- Low disk space simulation
- Very large files (e.g., 500MB)

Tests are located in the ZipExtensionTest project and use:

- System.IO.Abstractions.TestingHelpers
- NSubstitute
- NUnit
