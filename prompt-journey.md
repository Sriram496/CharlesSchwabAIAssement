# My prompt journey: skills, agents, requirements, architecture, code

Durations are estimates based on message timing gaps, including assistant response time and my review time. Where a "prompting technique" is described, it is based on what the history shows.

## Phase timeline

| # | Phase | Duration |
|---|-------|----------|
| 1 | Skills and agents | ~57 min |
| 2 | Requirements (plan, draft, freeze) | ~22 min |
| 3 | Architecture and design | ~2 h 15 min |
| 4 | Coding, features 1 to 5, README | ~28 min |
| 5 | Post-build changes | ~4 min |

Active work was about 3 hours 35 minutes.

## Phase 1: Build the tooling first

I created reusable skills and agents before touching the project, so every later step followed a repeatable procedure.

| # | My prompt | Intent | Result |
|---|-----------|--------|--------|
| 1 | `/create-skill` to turn feature scopes into structured, testable requirements, with an SRS and a Markmap feature map | A requirements procedure with traceable IDs (REQ-FUN, REQ-NFR) | `srs-featuremap-generator` skill |
| 2 | `/create-skill` for an architectural design procedure | C4 models, sequence diagrams, and ADRs from the SRS | `architecture-design-procedure` skill |
| 3 | `/create-agent` named `architecture-agent`, with a name and description | A specialized sub-agent for architecture | Agent created |
| 4 | `/create-agent` named `requirements-agent` | A second sub-agent for the requirements stage | Agent created |
| 5 | **Correction:** add tech stack choice (language, framework, tooling) to the architecture agent | I saw a gap: nothing decided the technology before coding | Agent now decides and records the stack before any code |
| 6 | `/create-skill` to implement code that follows the finalized architecture and stack | Code must follow the design, with the language's coding standards | `code-implementation-procedure` skill |
| 7 | `/create-agent` for the code agent, working from the architecture artifacts | The last role in the chain | `code-agent`, created but not run |

**Technique:** I created tools and roles first, and I split the work into requirements, architecture, and code. Step 5 was my first course correction.

## Phase 2: Requirements analysis and freeze

| # | My prompt | Result |
|---|-----------|--------|
| 8 | `/plan` to understand the problem statement in the discovery docs, analyze requirements, and freeze them | The assistant drafted a plan |
| 9 | **Correction:** I told the assistant the starter project is an empty .NET 10 console app, with no prerequisite data, no persistence, and no concurrency. I also asked for plain language. | The assistant revised the plan and rewrote it in plain language |
| 10 | "Go ahead with requirement artifacts draft generation" | v0.9 SRS and feature map written |
| 11 | "I am good with the results, let's freeze the requirement" | SRS v1.0 frozen, with change control |

**Technique:** I corrected the assistant's assumptions with project facts before any document was written. I reviewed the draft before freezing, so requirements could not drift later.

## Phase 3: Architecture with validation gates

I asked for the design in steps, validated one at a time, in the order HLD then LLD.

| # | My prompt | Result |
|---|-----------|--------|
| 12 | Plan the architecture and design, giving me each draft to validate one at a time | Gate 1 started. The assistant asked two questions in chat. |
| 13 | "Five component approach is better and keep it. Looks good for the HLD, let's go to the next" | HLD committed to `c4-architecture.md` |
| 14 | **Correction:** add the gap found in the SRS as a limitation in a separate file that I can go through later, and do this for any future gaps | [limitations.md](limitations.md) created, with LIM-001 |
| 15 | "Pick option A for case 1 and finalize the design" | LLD committed, LIM-001 accepted |
| 16 | ADR 0001: Decision A is skip and retain ineligible. Decision B is a lock, with one lock around each request. | The assistant wrote a concurrency brainstorm before committing |
| 17 | "Go with option A, single in-process lock." Add an offering store interface (in memory for now). Use a console app. | Gates 1 to 3 committed |
| 18 | "Confirm with xUnit for test case implementation" | Recorded in ADR 0002 |
| 19 | "What about data models (DB schema)? We should finalize" | The assistant drafted a model, noting there is no database |
| 20 | **Correction:** "Can you explain clearly on topics which you require confirmation from me" | The assistant listed three confirmations plus one open item |
| 21 | My four answers: calculated values are not stored, add timestamps, the console host sets data in code, and treat promotion as a later event | Data model committed as section 3.5 |

**Technique:** I used decision gates and picked named options (A or B). I asked for a limitations log so open gaps are tracked and not forgotten. In step 20 I made the assistant state exactly what it needed from me.

## Phase 4: One feature at a time with approval gates

My main instruction: "implement the code to build the console based on the architecture design plan and do one feature at a time and create test cases for each completed feature and validate the test runs."

| # | My prompt | Assistant response |
|---|-----------|--------------------|
| 22 | The instruction above | Surveyed `Code/` and drafted the Feature 1 plan, without editing files |
| 23 | **Correction:** approved, asked to delete the default unit test file, and said yes to item 4 of the plan | Feature 1 implemented and passing (~9 min) |
| 24 | "Approve" | Drafted the detailed Feature 2 plan |
| 25 | "Yes approved for all" | Feature 2 done, 28 tests pass (~6 min) |
| 26 | **Correction:** "Approve" with no clear scope | The assistant said it did not cover Feature 3 and waited for a clear approval |
| 27 | "Approve" | Feature 3 done: 41 tests, 13 new (~3.5 min) |
| 28 | "Approve" | Drafted the Feature 4 plan with two open questions |
| 29 | "Yes for both" | Feature 4 done: 59 tests. The assistant found and fixed a bug in its own test. (~2.5 min) |
| 30 | "Approve" | Feature 4 closed. Drafted the Feature 5 plan. |
| 31 | **Correction:** "Yes to the plan, OK with the command syntax, OK to item 3" | Feature 5 done: 84 tests. One deviation reported: a missing ID is "rejected: unknown learner", per REQ-FUN-004. (~7 min) |
| 32 | "Create a README file that has all instructions to run the application" | [Code/README.md](../Code/README.md) created |

**Technique:** A plan first, then my approval, then implementation. Every feature had its own tests plus format, build, and test checks. The checks and the approval gates made the assistant stop and report deviations. When my "Approve" was ambiguous in step 26, the assistant did not guess.

## Phase 5: Refinements and regression check

| # | My prompt | Result |
|---|-----------|--------|
| 33 | "Increase the capacity to 5, and how many learners are available in the system and what their prerequisite status" | Capacity is 5. The banner now reads capacity from the offering. |
| 34 | "Increase the learners available" | L5 to L10 added. L4 and L9 do not meet the prerequisite. |
| 35 | "Check whether the changes broke any test case or not" | Format, build, and all 84 tests pass |

**Technique:** Small requests, each followed by a regression check.

## Prompting skills this history shows

- **Set up the process before the work.** I created skills and agents first, so each stage had rules.
- **Gate every decision.** I validated HLD, LLD, ADRs, and the data model one at a time.
- **Correct the assistant with facts.** The empty-project correction (step 9) and the clarification request (step 20) are examples.
- **Track gaps instead of losing them.** The limitations log and the "do this for any future gaps" instruction made that standing.
- **Control scope with approvals.** One feature at a time, with a plan first, and no code before approval.
- **Verify every change.** Tests, format, and build ran after each feature and after each tweak.
