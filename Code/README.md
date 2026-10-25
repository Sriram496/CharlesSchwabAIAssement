# Course Offering Enrollment and Waiting List

A console application that lets learners enroll in one course offering, drop from it, and wait in a first-come, first-served line when it is full.

## Prerequisites

- .NET 10 SDK. Check with `dotnet --version`. Install from [dotnet.microsoft.com](https://dotnet.microsoft.com/download) if needed.
- macOS, Linux, or Windows terminal.

## Project Layout

- `CourseRegistrationApplication.slnx`: solution file.
- `CourseRegistrationApplication/`: console application.
- `CourseRegistrationApplication.Test/`: xUnit test project.

All commands below run from the `Code/` folder.

## Run the Application

```bash
cd Code
dotnet run --project CourseRegistrationApplication/CourseRegistrationApplication.csproj
```

On start, the application prints the offering and the command list, then shows a `>` prompt.

## Commands

| Command | Meaning |
|---------|---------|
| `enroll <learnerId>` | Enroll the learner, or place them on the waiting list when the offering is full. |
| `drop <learnerId>` | Drop the learner from the enrollment or the waiting list. A freed seat goes to the earliest eligible waiting learner. |
| `status <learnerId>` | Show whether the learner is enrolled, waiting (with position), or neither. |
| `quit` | Exit the application. |

Command words are not case-sensitive. A missing or unknown learner ID is rejected and nothing changes.

## Sample Data

The sample offering and learners are set in [CourseRegistrationApplication/Program.cs](CourseRegistrationApplication/Program.cs).

- Offering: `CS101`, capacity 5.
- Learners `L1`, `L2`, `L3`, `L5`, `L6`, `L7`, `L8`, `L10`: known, prerequisite met.
- Learners `L4` and `L9`: known, prerequisite not met (cannot enroll or wait).
- With 5 seats and 8 eligible learners, the waiting list is reachable.
- Any other ID is an unknown learner.

To change them, edit the `new Offering(...)` and `eligibility.AddLearner(...)` lines in `Program.cs`.

## Example Session

```text
Course offering CS101 (capacity 5). Commands: enroll <id>, drop <id>, status <id>, quit
> enroll L1
L1 is enrolled.
> enroll L4
Rejected: L4 has not met the prerequisite.
> status L1
L1 is enrolled.
> register X
Unknown command. Use: enroll <learnerId>, drop <learnerId>, status <learnerId>, quit.
> quit
```

## Run the Tests

```bash
cd Code
dotnet test CourseRegistrationApplication.slnx
```

## Build and Format Check

```bash
cd Code
dotnet format CourseRegistrationApplication.slnx --verify-no-changes
dotnet build CourseRegistrationApplication.slnx
```

## Important Notes

- All data is held in memory. Restarting the application clears all enrollments and the waiting list.
- The application runs as a single process. Running two copies gives each its own seats and waiting list.
