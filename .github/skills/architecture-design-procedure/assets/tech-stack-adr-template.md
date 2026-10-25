# 0002: Technology Stack Selection

- Status: Proposed
- Date: <YYYY-MM-DD>
- Deciders: <names>

## Context and Problem Statement

<Which language, framework, and tooling to use for <System Name>; cite SRS sections and constraints from the user or Discovery notes.>

## Decision Drivers

- <driver, e.g. REQ-NFR-001 thread-safe concurrent enrollment>
- <driver, e.g. in-memory store, no external database>
- <driver, e.g. testability of FIFO and guardrail rules>
- <driver, e.g. team familiarity or mandated constraint>

## Considered Options

- <Stack A: language + framework>
- <Stack B: language + framework>

## Pros and Cons of the Options

### <Stack A>
- Good, because <reason>
- Bad, because <reason>

### <Stack B>
- Good, because <reason>
- Bad, because <reason>

## Decision Outcome

Chosen option: "<Stack>", because <justification>.

| Area | Choice | Version |
|------|--------|---------|
| Language | <language> | <version> |
| Framework | <framework or none> | <version> |
| Concurrency primitives | <primitives implementing ADR 0001> | n/a |
| Test framework | <framework> | <version> |
| Build / package tooling | <tool> | <version> |

## Consequences

- Good: <consequence>
- Bad: <accepted trade-off>
- Implementation under `Code/` must use this stack; changing it requires a superseding ADR.
