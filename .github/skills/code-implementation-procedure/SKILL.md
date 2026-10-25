---
name: code-implementation-procedure
description: 'Implement application code that follows the finalized architecture (C4, sequence flows, ADRs) and the chosen tech stack, applying the language or framework official coding standards, formatting, OOP/SOLID principles, reusability, and maintainability. Use when asked to implement, code, build, or write the application, services, controllers, or tests after Artifacts/Architecture and ADR 0002 exist; also use to look up and apply the coding standard for the selected language.'
argument-hint: 'Optional: component or requirement IDs to implement (default: all)'
---

# Code Implementation Procedure

Implements the system in `Code/` strictly according to the approved architecture and tech stack.

## Inputs (read first, in this order)
1. `Artifacts/adr/0002-technology-stack-selection.md` - language, framework, versions, test framework, build tooling.
2. `Artifacts/adr/0001-waitlist-policy-and-concurrency.md` - policy and concurrency mechanism to implement.
3. `Artifacts/Architecture/c4-architecture.md` - components, responsibilities, invariants, `Technology Stack` section.
4. `Artifacts/Architecture/sequence-flows.md` - call order, branches, lock boundaries, error paths.
5. `Artifacts/Requirements/SRS.md` - `REQ-*` statements, acceptance criteria, `POL-*` policies, traceability matrix.

## Gate
If ADR 0002 or the C4 `Technology Stack` section is missing, or the stack is still undecided, stop. Tell the user to run the `architecture-agent` first. Do not pick a stack here. If the stack conflicts with a requirement, report it rather than switching stacks; changing the stack requires a superseding ADR.

## Procedure

1. **Load the coding standard.** Identify the language/framework from ADR 0002, then follow [the standards reference](./references/coding-standards.md): fetch the official style guide(s) listed for that language and apply them. If the language is not listed, search for its official or community-canonical style guide and use that. Honor existing config in `Code/` (`.editorconfig`, linter, formatter) over defaults.
2. **Plan the structure.** Map each C4 component to a class/module/package in the idiomatic project layout of the stack. Keep component names from the C4 diagram so the code is traceable to the design. Track work with a todo list: one item per component plus tests.
3. **Scaffold** the project with the stack's standard tooling and the versions from ADR 0002 (project files, formatter/linter config, test project).
4. **Implement bottom-up:** domain models and errors, then `InMemoryStore`, `WaitlistQueueManager`, `CourseOfferingService`, `EnrollmentController`. Implement exactly the flows in the sequence diagrams, including every `alt`/`opt` branch (e.g. Skip & Retain Ineligible, duplicate and guardrail errors) and the locking from ADR 0001.
5. **Add tests** for each requirement using the stack's test framework from ADR 0002, covering the acceptance criteria and the SRS traceability matrix. Include concurrency tests for the NFRs. Name tests so the `REQ-*` ID is discoverable.
6. **Format, lint, build, test.** Run the stack's formatter, linter/analyzers, build, and tests. Fix all failures and warnings you introduced.
7. **Verify conformance** with the checklist below and report: files created, requirements covered, deviations or open questions.

## Code Quality Rules

### Standards and formatting
- Follow the language's official style guide for naming, layout, imports, documentation comments, and idioms. Let the formatter decide whitespace; do not hand-format against it.
- Names reveal intent and match domain terms in the SRS. No abbreviations, magic numbers, or magic strings; use named constants or enums.

### OOP and SOLID (when classes are used)
- **S**: one reason to change per class; controller handles I/O only, service handles rules, queue manager handles ordering, store handles state.
- **O**: extend behavior through new types or strategies (e.g. a promotion policy interface), not by editing stable code with growing conditionals.
- **L**: subtypes honor the contracts of their base types or interfaces.
- **I**: small, role-focused interfaces; no class forced to implement unused members.
- **D**: depend on abstractions and inject dependencies (e.g. store, queue manager, clock/ID generators); no hidden globals or `new` of collaborators inside business logic.
- Prefer composition over inheritance. Keep fields private; expose behavior, not mutable internals. Use immutable value objects where practical.
- In non-OOP or functional stacks, apply the equivalent: small single-purpose functions/modules, explicit dependencies, pure logic separated from side effects.

### Reusability and maintainability
- DRY: extract shared validation, error construction, and locking helpers once; do not copy-paste branches.
- Small functions, shallow nesting, early returns for guard clauses; one level of abstraction per function.
- Centralize error types and error-to-response mapping per the SRS error requirements; never swallow exceptions.
- Keep policy rules (prerequisites, promotion, deduplication) in one place each, referenced by `POL-*` IDs in a one-line comment only where the code cannot show it.
- Make behavior testable: pure rule logic separate from locking and storage.
- Do not over-engineer: no speculative layers, frameworks, or features beyond the architecture and SRS.

## Constraints
- DO NOT change files under `Artifacts/`; report design problems instead.
- DO NOT introduce languages, frameworks, or libraries not in ADR 0002 without flagging it; justify any extra dependency and prefer the standard library.
- DO NOT add features outside the SRS.

## Conformance Checklist
- [ ] Language, framework, and versions match ADR 0002.
- [ ] Each C4 component exists as a distinct unit with the documented responsibility; no cross-layer leaks.
- [ ] Every sequence-flow branch is implemented; locking matches ADR 0001.
- [ ] Every `REQ-FUN-*` and `REQ-NFR-*` has at least one test; all tests pass.
- [ ] Formatter, linter/analyzers, and build run clean.
- [ ] No duplicated logic, magic literals, or unused code; SOLID violations resolved.
- [ ] Any language standard added beyond the reference table is noted in the report with its source.
