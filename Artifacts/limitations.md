# Limitations Log

Single place to record limitations, gaps, and known trade-offs found in the requirements, architecture, or code. Review them with the requirements later; nothing here changes the frozen SRS until a new SRS version is approved.

## How to Use
- Add every new limitation as a new entry at the end. Never delete entries; change their status instead.
- IDs are sequential (`LIM-001`, `LIM-002`, ...) and never reused.
- Statuses: `Open`, `Under Review`, `Accepted` (kept as a known limitation), `Resolved` (fixed by a new SRS version or design change; note which).
- Each entry states the affected requirement or policy IDs, the plain-language scenario, the impact, how the design currently handles it, and the decision needed.

## Summary

| ID | Title | Affects | Found During | Status |
|----|-------|---------|--------------|--------|
| LIM-001 | New learner can take an open seat ahead of a waiting learner who has become eligible | REQ-FUN-001, REQ-FUN-012, REQ-FUN-015, POL-WL-001 | Architecture Gate 2 (LLD) | Accepted (Option A) |
| LIM-002 | Single process only; requests run one at a time; state lost on restart | REQ-NFR-001 to REQ-NFR-005 | Architecture Gate 3 (ADR 0001, ADR 0002) | Accepted |
| LIM-003 | Eligibility is a point-in-time answer and can change outside the offering lock | REQ-FUN-013, POL-PRE-001 | Architecture Gate 3 (ADR 0001) | Accepted |

## Entries

### LIM-001: New learner can take an open seat ahead of a waiting learner who has become eligible
- **Date logged:** 2026-10-25
- **Status:** Accepted (Option A), decided 2026-10-25
- **Found during:** Architecture Gate 2 (low-level design review of the enrollment flow)
- **Affects:** REQ-FUN-001 (enroll when a seat is free), REQ-FUN-012 (promote earliest eligible), REQ-FUN-015 (leave seat open when nobody is eligible), POL-WL-001
- **Scenario:**
  1. A seat opens, but every waiting learner is ineligible, so the seat stays open (REQ-FUN-015).
  2. One of the waiting learners then meets the prerequisite. No promotion runs, because promotion only happens when a seat opens.
  3. A new, eligible learner asks to enroll. A seat is free, so REQ-FUN-001 enrolls them.
  4. The waiting learner, who joined earlier and is now eligible, is passed over until the next seat opens.
- **Impact:** Low likelihood, but it can break the first-come, first-served expectation for a learner who has waited longer. No invariant is violated: capacity is respected, and nobody is enrolled twice.
- **Current handling:** The design follows the frozen SRS literally. A new eligible learner is enrolled when a seat is free.
- **Options:**
  - **A. Accept as a known limitation** (current handling). No SRS change.
  - **B. Run a promotion pass first.** Before enrolling a new learner into an open seat, offer the seat to eligible waiting learners in order. Needs a new SRS version amending REQ-FUN-001 and REQ-FUN-015.
- **Decision:** Option A accepted on 2026-10-25. The SRS stays at v1.0 and the design follows REQ-FUN-001 literally. Revisit with the requirements review if fairness for this case becomes a concern.

### LIM-002: Single process only; requests run one at a time; state lost on restart
- **Date logged:** 2026-10-25
- **Status:** Accepted
- **Found during:** Architecture Gate 3 (ADR 0001 concurrency decision, ADR 0002 stack)
- **Affects:** REQ-NFR-001 to REQ-NFR-005
- **Scenario:** The offering lock protects one running copy of the application. Two copies would each hold their own seats and waiting list. All state is held in memory, so a restart loses all enrollments and the waiting list. Requests run one at a time, and under heavy simultaneous load the order in which waiting requests are served is not guaranteed to match their arrival order.
- **Impact:** Acceptable for one offering in a console application with no persistence. Not suitable for several running copies or for data that must survive a restart.
- **Current handling:** Accepted by the project owner in ADR 0001 (option 1) and ADR 0002. `IOfferingStore` keeps the door open to a different store later.
- **Decision:** Accepted on 2026-10-25. Revisit with a new SRS version if persistence or several running copies become requirements.

### LIM-003: Eligibility is a point-in-time answer and can change outside the offering lock
- **Date logged:** 2026-10-25
- **Status:** Accepted
- **Found during:** Architecture Gate 3 (ADR 0001, eligibility handling)
- **Affects:** REQ-FUN-013, POL-PRE-001
- **Scenario:** A learner's prerequisite status can be updated at any time by whatever supplies that data. The check made inside the locked step is correct for that moment; a change a moment later is treated as a later event and is not held back by the offering lock.
- **Impact:** A learner could be promoted or enrolled just before their status changes, or just after it changes in their favor. No invariant is broken.
- **Current handling:** `EligibilityProvider` is safe for simultaneous updates on its own. Updates to eligibility data are not routed through the offering lock.
- **Decision:** Accepted on 2026-10-25. A change to a learner's prerequisite status made outside a request is treated as a later event and is not held back by the offering lock.
