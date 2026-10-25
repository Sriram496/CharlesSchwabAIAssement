# Course Offering Enrollment and Waiting List

This repository contains the requirements, architecture, implementation, and tests for a single-offering course enrollment console application. Use this README as a map to the project; the detailed run guide is in [`Code/README.md`](Code/README.md).

## Quick Start

**Prerequisite:** .NET 10 SDK.

From the repository root, run:

```bash
cd Code
dotnet run --project CourseRegistrationApplication/CourseRegistrationApplication.csproj
```

For console commands, sample learner data, and additional run details, see [`Code/README.md`](Code/README.md).

To build and run the test suite:

```bash
cd Code
dotnet build CourseRegistrationApplication.slnx
dotnet test CourseRegistrationApplication.slnx
```

## Repository Structure

```text
.
├── .github/
│   ├── agents/       # Specialized requirements, architecture, and coding agents
│   └── skills/       # Reusable procedures and templates for project work
├── Artifacts/
│   ├── Architecture/ # C4 architecture and sequence flows
│   ├── Requirements/ # Frozen SRS and feature map
│   ├── adr/          # Architecture decision records
│   └── limitations.md
├── Code/
│   ├── CourseRegistrationApplication/      # Console app and domain implementation
│   ├── CourseRegistrationApplication.Test/  # xUnit tests
│   └── README.md                            # Run, command, and sample-data guide
├── Discovery/        # Original problem statement
├── prompt-journey.md # Prompting and project-work history
└── README.md         # This project and reviewer guide
```

## Where to Look

| If you want to... | Start here |
|---|---|
| Understand the original problem | [`Discovery/PROBLEM_STATEMENT.md`](Discovery/PROBLEM_STATEMENT.md) |
| Review the agreed requirements and acceptance criteria | [`Artifacts/Requirements/SRS.md`](Artifacts/Requirements/SRS.md) |
| Get a visual feature overview | [`Artifacts/Requirements/feature-map.md`](Artifacts/Requirements/feature-map.md) |
| Understand the components and their responsibilities | [`Artifacts/Architecture/c4-architecture.md`](Artifacts/Architecture/c4-architecture.md) |
| Follow enrollment, drop, and promotion interactions | [`Artifacts/Architecture/sequence-flows.md`](Artifacts/Architecture/sequence-flows.md) |
| See key design decisions and why they were chosen | [`Artifacts/adr/0001-waitlist-policy-and-concurrency.md`](Artifacts/adr/0001-waitlist-policy-and-concurrency.md), [`Artifacts/adr/0002-technology-stack-selection.md`](Artifacts/adr/0002-technology-stack-selection.md) |
| Review accepted limitations and known constraints | [`Artifacts/limitations.md`](Artifacts/limitations.md) |
| Run the app or learn the CLI commands and sample learners | [`Code/README.md`](Code/README.md) |
| Inspect application behavior | [`Code/CourseRegistrationApplication/`](Code/CourseRegistrationApplication/) |
| Inspect automated tests | [`Code/CourseRegistrationApplication.Test/`](Code/CourseRegistrationApplication.Test/) |
| See how the project was prompted and built | [`prompt-journey.md`](prompt-journey.md) |
| Understand the custom AI workflow used in this project | [`.github/agents/`](.github/agents/), [`.github/skills/`](.github/skills/) |

## Implementation Notes

- The app targets .NET 10 and uses an in-memory store; it does not persist data between runs.
- The console host configures one offering (`CS101`) and sample learners in code. Their prerequisite status is shown at startup.
- Tests use xUnit. The solution and test commands are listed above and in [`Code/README.md`](Code/README.md).
