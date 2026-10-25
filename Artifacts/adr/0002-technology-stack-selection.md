# 0002: Technology Stack Selection

- Status: Accepted
- Date: 2026-10-25
- Deciders: Project owner, with the architecture agent

## Context and Problem Statement

Choose the language, framework and tooling for the Course Offering Enrollment and Waiting List system before any code is written in `Code/`. The project owner has decided on a console application with an in-memory store for now. An existing .NET 10 console project is already in `Code/CourseRegistrationApplication`.

## Decision Drivers

- REQ-NFR-001 to REQ-NFR-003: reliable concurrency in one process (ADR 0001).
- SRS out-of-scope: no front end, no database, no deployment.
- Testability of the rules, including concurrency tests (SRS traceability matrix).
- Existing scaffold: .NET 10, nullable reference types enabled.
- Project owner's decision: console app and in-memory store.

## Considered Options
- **A.** C# on .NET 10, console host, in-memory store
- **B.** C# on .NET 10 with an ASP.NET Core web API, in-memory store
- **C.** A different language and runtime (for example Java or Python), console host

## Pros and Cons of the Options

### A. C# on .NET 10, console host
- Good, because it matches the existing project and the owner's decision.
- Good, because the standard library has what is needed: a lock type, thread-safe dictionary, and records.
- Good, because the test tooling and `dotnet test` workflow are built in.
- Bad, because there is no network interface for callers (not required by the SRS).

### B. ASP.NET Core web API
- Good, because it provides a ready-made request interface.
- Bad, because it adds a web surface, hosting and API design that the SRS excludes.

### C. A different language
- Good, because it could suit teams with other skills.
- Bad, because it discards the existing project with no requirement driving the change.

## Decision Outcome

Chosen option: **A. C# on .NET 10, console host, in-memory store**, because it fits the existing project, the owner's decision and the SRS scope, and supports the concurrency design in ADR 0001.

| Area | Choice | Version |
|------|--------|---------|
| Language and platform | C# on .NET | .NET 10 (`net10.0`), nullable reference types on |
| Host | Console application (thin host and composition root) | existing project |
| Application framework | None; plain classes | n/a |
| Concurrency primitive | `lock` on a private `System.Threading.Lock`, synchronous methods | .NET 10 |
| Eligibility data | `ConcurrentDictionary`-backed `EligibilityProvider` | .NET 10 |
| Store | `InMemoryStore` implementing `IOfferingStore` (`Dictionary<string, EnrollmentRecord>` and `List<WaitlistEntry>`) | .NET 10 |
| Learner identifiers | Case-sensitive strings compared exactly; blank is invalid | n/a |
| Outcomes | Immutable `record` types, no exceptions for business rejections | C# 14 |
| Clock | `System.TimeProvider`, injected; audit timestamps in UTC that never affect decisions or order | .NET 10 (built in) |
| Test framework | xUnit, in a new test project in the same solution | latest stable, pinned at implementation |
| Formatting and analysis | `.editorconfig`, `dotnet format`, built-in analyzers, Microsoft C# conventions | SDK built-in |
| Build tooling | `dotnet` CLI with the existing `.slnx` solution | .NET 10 SDK |
| Packages | Standard library plus test framework packages only | n/a |

xUnit was confirmed as the test framework by the project owner on 2026-10-25.

## Consequences

- Good: no new infrastructure, minimal dependencies and a small, testable codebase.
- Good: `IOfferingStore` lets a different store replace the in-memory one without changing the rules.
- Bad: no network interface; callers use the console host or call the classes directly.
- Bad: state is lost on restart ([LIM-002](../limitations.md)).
- **Implementation gate:** all implementation under `Code/` must follow this stack and ADR 0001. Changing either requires a new ADR that supersedes this one.
