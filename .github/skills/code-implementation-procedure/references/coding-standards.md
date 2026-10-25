# Coding Standards Reference

Look up the language from ADR 0002, fetch the linked official guide(s) with the web fetch tool, and apply them.

| Language / Stack | Style and conventions | Formatter / linter |
|------------------|-----------------------|--------------------|
| C# / .NET | https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions and https://learn.microsoft.com/dotnet/standard/design-guidelines/naming-guidelines | `dotnet format`, `.editorconfig`, Roslyn analyzers |
| Java | https://google.github.io/styleguide/javaguide.html | google-java-format, Checkstyle |
| Python | https://peps.python.org/pep-0008/ and https://peps.python.org/pep-0257/ | Black or Ruff, flake8/Ruff, mypy |
| TypeScript / JavaScript | https://google.github.io/styleguide/tsguide.html | Prettier, ESLint |
| Go | https://go.dev/doc/effective_go | `gofmt`, `go vet`, golangci-lint |
| Kotlin | https://kotlinlang.org/docs/coding-conventions.html | ktlint or detekt |
| Rust | https://doc.rust-lang.org/style-guide/ and https://rust-lang.github.io/api-guidelines/ | `rustfmt`, `clippy` |

## Language not listed
1. Search for the language's official style guide (language website or foundation), else the most widely adopted community guide.
2. Record the guide's name and URL in the final report.
3. Use the language's standard formatter and linter.

## What to extract from each guide
- Naming (types, members, constants, files, packages)
- File and project layout, import ordering
- Error and exception handling conventions
- Concurrency and immutability idioms
- Documentation comment format
- Testing naming and layout conventions
