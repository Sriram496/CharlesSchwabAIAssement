---
name: srs-featuremap-generator
description: 'Transform a feature scope into a structured, testable Software Requirements Specification (SRS.md) and a Markmap mind map (feature-map.md). Use when asked to write requirements, create an SRS, derive REQ-FUN/REQ-NFR requirements, build a traceability matrix, or generate a feature map / mind map for enrollment, dropping, waitlist FIFO promotion, prerequisites, deduplication, or state-guardrail features.'
argument-hint: 'Feature scope or path to discovery notes (e.g. Discovery/scope.md)'
---

# SRS and Feature Map Generator

Turns a feature scope into two artifacts:

| Artifact | Path |
|----------|------|
| SRS | `Artifacts/Requirements/SRS.md` |
| Markmap feature map | `Artifacts/Requirements/feature-map.md` |

Always write to `Artifacts/Requirements/` (create the folder if missing). Do not write to `docs/requirements/`.

## When to Use
- A feature scope, user story set, or discovery notes must become formal, testable requirements.
- The user asks for an SRS, requirement IDs, a traceability matrix, or a mind map of features.

## Procedure

1. **Gather scope.** Read the user's scope and any files in `Discovery/` and `Artifacts/Architecture/`. If scope, policies, or capacity/prerequisite rules are missing, ask the user; otherwise record the gap as an explicit assumption in Section 3.
2. **Identify feature sets.** Default groups: Enrollment, Dropping, FIFO Promotion, State Guardrails. Add groups only if the scope requires them.
3. **Draft `SRS.md`** using [the SRS template](./assets/srs-template.md) and the schema rules below.
4. **Draft `feature-map.md`** using [the feature-map template](./assets/feature-map-template.md). Every `REQ-FUN-XXX` and `REQ-NFR-XXX` ID must appear in the map.
5. **Validate** with the checklist below, then fix any failure before reporting.
6. **Report** briefly: files written, requirement counts, open assumptions/questions.

## SRS Schema Rules

`SRS.md` must contain these six sections, in this order, as `##` headings:

1. **Document Identification & Metadata** - Title, Version, Status, Date.
2. **System Purpose & Scope Boundaries** - explicit `IN-SCOPE` and `OUT-OF-SCOPE` lists.
3. **Domain Assumptions & Policy Definitions** - formally define prerequisite rules, waitlist promotion policy, and deduplication logic. Give each policy an ID (`POL-XXX`) so requirements can cite it.
4. **Functional Requirements (`REQ-FUN-XXX`)** - grouped by feature set (Enrollment, Dropping, FIFO Promotion, State Guardrails).
5. **Non-Functional Requirements (`REQ-NFR-XXX`)** - at minimum: Concurrency, State Consistency, Error Handling.
6. **Requirements Traceability Matrix** - table mapping every requirement ID to a test verification method.

### Functional requirement rules
- Statement syntax is exactly: `The system shall [action] when [condition].`
- Each requirement has: Traceability ID, Statement, Inputs, Expected Outputs, Acceptance Criteria.
- Acceptance criteria use Given/When/Then, are objectively verifiable, and cover at least one positive and one negative/edge case.
- One behavior per requirement; no "and/or" compound actions. No vague terms ("fast", "appropriate", "user-friendly").
- IDs are sequential and unique (`REQ-FUN-001`, `REQ-FUN-002`, ...), never reused or renumbered after publication.
- Reference policies (`POL-XXX`) rather than restating policy text.

### Non-functional requirement rules
- Same `shall ... when ...` syntax, with measurable thresholds (e.g. "exactly one of N concurrent requests succeeds for the last seat").
- Concurrency: simultaneous enroll/drop on the same resource, no double-booking, no lost promotion.
- State Consistency: enrolled count, waitlist order, and capacity invariants hold after every operation.
- Error Handling: defined error codes/messages for each rejected operation; failed operations leave state unchanged.

### Traceability matrix rules
- Columns: Requirement ID | Feature Set | Verification Method | Test Level | Test Case Reference.
- Verification Method is one of: Unit Test, Integration Test, Concurrency/Stress Test, Inspection, Demonstration.
- No orphan rows: every requirement ID in Sections 4-5 appears exactly once as a row, and every row maps to an existing ID.

## Feature Map Rules (Markmap)

- Plain Markdown using nested headers (`#`, `##`, `###`) and `- bullets` only, so it renders in VS Code via the `gera2ld.markmap-vscode` extension.
- Single `#` root (the system name); `##` per feature set; `###` per requirement group or policy; bullets for `REQ-*` IDs with a short title.
- Optional YAML frontmatter with `markmap:` options is allowed (see template). No tables, code blocks, or HTML.
- Mirror SRS structure: Scope, Policies, the four feature sets, NFRs.

## Validation Checklist
- [ ] Both files exist at the exact paths above.
- [ ] All six SRS sections present, in order, with metadata filled (no placeholders left).
- [ ] IN-SCOPE and OUT-OF-SCOPE lists both non-empty.
- [ ] Every functional requirement follows `The system shall ... when ...` and has Inputs, Outputs, Acceptance Criteria.
- [ ] Policies for prerequisites, FIFO waitlist promotion, and deduplication are defined and referenced by requirements.
- [ ] Requirement IDs are unique, sequential, and match across SRS, traceability matrix, and feature map.
- [ ] State guardrails cover invalid transitions (e.g. dropping when not enrolled, enrolling twice, enrolling in a full section without waitlist).
- [ ] Feature map contains only headers and bullets.
