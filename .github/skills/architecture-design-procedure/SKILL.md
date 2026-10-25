---
name: architecture-design-procedure
description: 'Transform requirements (SRS.md) into C4 architecture models, Mermaid sequence diagrams, and MADR Architecture Decision Records. Use when asked to design the architecture, create C4 context/container/component diagrams, model sequence flows or lifelines, write an ADR, choose and document the programming language / framework / tech stack, or decide waitlist promotion policy (Skip & Retain vs Remove Ineligible) and concurrency mechanism (mutex vs synchronized transactions) for an enrollment/waitlist system.'
argument-hint: 'Path to requirements (default: Artifacts/Requirements/SRS.md)'
---

# Architectural Design Procedure

Turns requirements into three artifacts:

| Artifact | Path |
|----------|------|
| C4 architecture | `Artifacts/Architecture/c4-architecture.md` |
| Sequence flows | `Artifacts/Architecture/sequence-flows.md` |
| ADR (policy and concurrency) | `Artifacts/adr/0001-waitlist-policy-and-concurrency.md` |
| ADR (technology stack) | `Artifacts/adr/0002-technology-stack-selection.md` |

The technology stack choice is also committed into `c4-architecture.md`, so it is recorded in two Markdown artifacts.

Write only to these paths (create folders if missing). Do not write to `docs/architecture/` or lowercase `Artifacts/architecture/`.

## When to Use
- Requirements exist (or are described) and need a system design before coding.
- The user asks for C4 views, sequence diagrams, or ADRs.

## Procedure

1. **Read inputs.** Load `Artifacts/Requirements/SRS.md` (and `feature-map.md` if present). Extract policies (`POL-*`), functional requirements, and NFRs, especially concurrency and state consistency. If the SRS is missing, ask the user or run the `srs-featuremap-generator` skill first.
2. **Draft `c4-architecture.md`** from [the C4 template](./assets/c4-template.md).
3. **Draft `sequence-flows.md`** from [the sequence template](./assets/sequence-template.md).
4. **Draft the ADR** from [the MADR template](./assets/madr-template.md).
5. **Select the tech stack** before any implementation code exists, following "Technology Stack Selection" below. Write `0002-technology-stack-selection.md` from [the stack ADR template](./assets/tech-stack-adr-template.md) and add the "Technology Stack" section to `c4-architecture.md`.
6. **Cross-check** with the checklist below and fix failures.
7. **Report** briefly: files written, key decisions (including the chosen stack), open questions.

## Artifact 1: `c4-architecture.md`

- **Context View (C4 Level 1):** embedded Mermaid `graph TB` showing the Learner, the System Boundary (subgraph), and the Course Service inside it. Label edges with the interaction (e.g. "enroll / drop").
- **Container / Component View (C4 Level 2/3):** Mermaid `graph TB` with these components and their responsibilities:
  - `EnrollmentController` - request entry, input validation, error mapping.
  - `CourseOfferingService` - enrollment/drop rules, prerequisite and duplicate checks, capacity.
  - `WaitlistQueueManager` - FIFO queue, promotion, skip/retain handling.
  - `InMemoryStore` - state for offerings, enrollments, waitlists, learner records.
- Follow with a component responsibility table (Component | Responsibility | Requirements covered) citing `REQ-*` IDs.
- Include a short "Invariants" list (capacity never exceeded, no duplicate enrollment, waitlist order preserved).

## Artifact 2: `sequence-flows.md`

Three `sequenceDiagram` blocks, each under its own `##` heading with a one-line description and the `REQ-*` IDs covered. Participants use the C4 component names, with Learner as the actor.

1. **Successful Enrollment & Full-Capacity Waitlist Redirect** - use `alt` for seat available (enrolled) vs. offering full (added to waitlist, position returned).
2. **Learner Drop triggering FIFO Waitlist Promotion** - drop frees a seat; `WaitlistQueueManager` takes head of queue; `alt` eligible (promote) vs. ineligible (**Skip & Retain Ineligible Learner**: skip, keep in queue at original position, try next).
3. **Duplicate Request & Error Guardrails** - `alt`/`opt` branches for duplicate enroll, drop when not enrolled, unmet prerequisite, unknown offering; each returns the error defined in the SRS and leaves state unchanged.

Show lock acquire/release (`Note over`) around state mutations in flows 1 and 2, consistent with the ADR decision.

## Artifact 3: ADR (MADR)

One ADR covering two decisions, using the MADR sections (Status, Context and Problem Statement, Decision Drivers, Considered Options, Decision Outcome, Consequences, Pros and Cons of the Options):

- **Decision A - Waitlist promotion policy:** *Skip & Retain* vs. *Remove Ineligible*.
- **Decision B - Concurrency and thread-safety:** mutex/lock per offering vs. synchronized transaction (single global lock or atomic block); add other options only if relevant to the stack.

Rules:
- Decision drivers come from SRS requirements and NFRs; cite the `REQ-*`/`POL-*` IDs.
- List at least two options per decision, each with pros and cons; state the chosen option and why, plus trade-offs accepted.
- Status is `Proposed` unless the user confirms acceptance. Date in `YYYY-MM-DD`.
- The chosen options must match the policy text in the SRS and the lock notes in the sequence diagrams. If the SRS and the decision conflict, flag it rather than silently diverging.

## Technology Stack Selection

Decide and document language, framework, and supporting stack choices before implementation.

1. **Honor explicit constraints.** If the user, SRS, `Discovery/`, or existing files in `Code/` already fix a language or framework, adopt it and record it as a constraint driver; still document alternatives briefly.
2. **Otherwise decide.** Evaluate at least two candidate stacks against drivers from the SRS (in-memory state, thread-safety/concurrency NFRs, testability, error handling) and team context. Pick one and justify it. Do not defer the choice.
3. **Cover these areas:** language and version, application framework (or none), concurrency primitives in that stack, test framework, build/package tooling, and API style if the SRS implies one.
4. **Commit to two artifacts, consistently:**
   - `Artifacts/adr/0002-technology-stack-selection.md` - MADR with Status, Context, Decision Drivers (cite `REQ-NFR-*`), Considered Options with pros/cons, Decision Outcome, Consequences. Status is `Proposed` unless the user confirms.
   - `c4-architecture.md` - add a `## 3. Technology Stack` section after the component view: a table (Layer / Component | Technology | Version | Rationale) covering the four components and test/build tooling, linking to ADR 0002. Mention the technology in Mermaid node labels (e.g. `EnrollmentController<br/>[ASP.NET Core Controller]`).
5. **Align with ADR 0001.** The concurrency mechanism must be implementable with primitives of the chosen stack; otherwise revise one or flag the conflict.
6. **Gate implementation.** ADR 0002 states that implementation under `Code/` must follow this stack. Never create code or project files in `Code/`.

## Validation Checklist
- [ ] Four files exist at the exact paths above (c4, sequence flows, ADR 0001, ADR 0002).
- [ ] The same language, framework, and versions appear in ADR 0002 and the C4 `Technology Stack` section.
- [ ] ADR 0001's concurrency mechanism is supported by the chosen stack.
- [ ] Mermaid blocks use correct fences and valid syntax (`graph TB`, `sequenceDiagram`).
- [ ] All four named components appear in the C4 component view and as sequence participants.
- [ ] All three sequence flows present, including the Skip & Retain branch.
- [ ] ADR covers both decisions with options, drivers, outcome, and consequences.
- [ ] Every flow and component cites requirement IDs that exist in the SRS.
- [ ] Concurrency mechanism is consistent across C4 invariants, sequence lock notes, and ADR.
