# Software Requirements Specification: Course Offering Enrollment and Waiting List

## 1. Document Identification & Metadata

| Field | Value |
|-------|-------|
| Title | Course Offering Enrollment and Waiting List Software Requirements Specification |
| Version | 1.0 |
| Status | Frozen (baselined) |
| Date | 2026-10-25 |
| Source | `Discovery/PROBLEM_STATEMENT.md` |

### Change Control
- This version is the approved requirements baseline. Requirement IDs, statements, and acceptance criteria shall not be edited in place.
- Any change requires a new version (v1.1 or later) with a change log entry stating what changed and why.
- Requirement IDs are never reused or renumbered; a withdrawn requirement is marked "Withdrawn" and keeps its ID.
- Downstream work (architecture, code, tests) shall trace to the IDs in this baseline.

| Version | Date | Change |
|---------|------|--------|
| 0.9 | 2026-10-25 | Initial draft |
| 1.0 | 2026-10-25 | Reviewed and approved; open items closed; baseline frozen |

## 2. System Purpose & Scope Boundaries

**Purpose:** Let learners enroll in a single course offering, drop from it, and wait in a fair first-come, first-served line when the offering is full. When a seat opens, the earliest-waiting learner who meets the prerequisite receives it.

### IN-SCOPE
- Enrolling one learner in the one course offering when a seat is free and the learner meets the prerequisite.
- Placing a learner who meets the prerequisite on the waiting list when the offering is full.
- Preventing a learner from holding more than one place (enrolled or waiting) in the offering.
- Dropping a learner from the offering and giving the freed seat to the earliest waiting learner who meets the prerequisite.
- Giving a clear answer for every request, including rejected, repeated, and invalid requests.
- Keeping seat counts and waiting-list order correct at all times.
- Checking a learner's current status (optional capability named in the problem statement).

### OUT-OF-SCOPE
- A complete registration product or more than one course offering.
- Screens or other user-facing front ends, course catalogs, payments, sign-in or identity integrations, and multi-term registration.
- Deployment and any new database.
- Configurable prerequisite or waiting-list rules.
- Recording the AI assistance used during the work (decided out of scope for the SRS).

## 3. Domain Assumptions & Policy Definitions

### 3.1 Assumptions
- **ASM-001:** There is exactly one course offering. It has a fixed number of seats (its capacity), which is a whole number greater than zero.
- **ASM-002:** The system can tell which learners are known to it, and for each known learner, whether they have met the offering's prerequisite.
- **ASM-003:** A learner is "invalid" when no learner identifier is given or the identifier does not belong to a known learner.
- **ASM-004:** A seat opens only when an enrolled learner drops.
- **ASM-005:** One freed seat is given to at most one learner.
- **ASM-006:** A learner who is already enrolled keeps their seat even if their prerequisite status later changes. Prerequisite status only affects entering the offering or being promoted.
- **ASM-007:** The waiting list has no maximum length.
- **ASM-008 (approved decision):** A learner who is on the waiting list may drop, which removes them from the list. The problem statement says a drop by "a learner who is not enrolled" fails; this assumption deliberately refines that wording so waiting learners can leave the list. A learner who is neither enrolled nor waiting still gets a failure.

### 3.2 Policy Definitions
- **POL-PRE-001 (Prerequisite Rule):** A learner is eligible only if they are a known learner and have met the offering's prerequisite. Eligibility is checked when a learner asks to enroll or join the waiting list, and checked again immediately before a waiting learner would be promoted. A learner who fails the check is not enrolled, not added to the waiting list, and not promoted.
- **POL-WL-001 (Waiting-List Promotion):** The waiting list is first come, first served, ordered by the moment each learner first joined. When a seat opens, the system looks at waiting learners in that order and promotes the first one who is eligible at that moment. A waiting learner who is not eligible is skipped for that seat but stays on the list in their original place. If no waiting learner is eligible, the seat stays open and the list is checked again the next time a seat opens.
- **POL-DUP-001 (One Place per Learner):** A learner may hold at most one place in the offering: either one enrollment or one waiting-list entry. Repeating a request for a learner who already holds a place creates nothing new and does not change their place in line. A learner who dropped may ask again later; that request is treated as brand new, and if the offering is full they join at the end of the waiting list.

### 3.3 Review Outcomes
- **OPEN-001 (Closed, confirmed):** ASM-008 stands: waiting learners may drop and leave the list. This refines the problem statement wording.
- **OPEN-002 (Closed, confirmed):** ASM-006 stands: enrolled learners keep their seat if their prerequisite status later changes.
- **OPEN-003 (Closed, confirmed):** ASM-007 stands: the waiting list has no maximum length.

## 4. Functional Requirements

### 4.1 Enrollment

#### REQ-FUN-001: Enroll when a seat is free
- **Statement:** The system shall enroll a learner in the offering when the learner is eligible, holds no place in the offering, and at least one seat is free.
- **Traceability ID:** REQ-FUN-001
- **Policy refs:** POL-PRE-001, POL-DUP-001
- **Inputs:** Learner identifier.
- **Expected Outputs:** Outcome "enrolled"; the number of enrolled learners goes up by one.
- **Acceptance Criteria:**
  - Given an eligible learner and an offering with a free seat, when the learner asks to enroll, then the learner is enrolled and the enrolled count rises by one.
  - Given an offering with exactly one free seat, when an eligible learner enrolls, then the enrolled count equals the capacity and no more.

#### REQ-FUN-002: Join the waiting list when full
- **Statement:** The system shall add a learner to the end of the waiting list when the offering is full, the learner is eligible, and the learner holds no place in the offering.
- **Traceability ID:** REQ-FUN-002
- **Policy refs:** POL-PRE-001, POL-WL-001, POL-DUP-001
- **Inputs:** Learner identifier.
- **Expected Outputs:** Outcome "waitlisted" with the learner's position in line; the enrolled count is unchanged.
- **Acceptance Criteria:**
  - Given a full offering and an eligible learner, when the learner asks to enroll, then the learner appears once on the waiting list and the enrolled count stays at capacity.
  - Given a waiting list with three learners, when a new eligible learner asks to enroll, then they are placed fourth.

#### REQ-FUN-003: Reject a learner who has not met the prerequisite
- **Statement:** The system shall reject an enrollment request when the learner has not met the prerequisite.
- **Traceability ID:** REQ-FUN-003
- **Policy refs:** POL-PRE-001
- **Inputs:** Learner identifier of a known learner who has not met the prerequisite.
- **Expected Outputs:** Outcome "rejected: prerequisite not met"; nothing changes.
- **Acceptance Criteria:**
  - Given an offering with a free seat and an ineligible learner, when the learner asks to enroll, then the learner is not enrolled.
  - Given a full offering and an ineligible learner, when the learner asks to enroll, then the learner is not added to the waiting list.

#### REQ-FUN-004: Reject an enrollment request from an invalid learner
- **Statement:** The system shall reject an enrollment request when the learner identifier is missing or does not belong to a known learner.
- **Traceability ID:** REQ-FUN-004
- **Policy refs:** POL-PRE-001
- **Inputs:** A blank, missing, or unrecognized learner identifier.
- **Expected Outputs:** Outcome "rejected: unknown learner"; nothing changes.
- **Acceptance Criteria:**
  - Given a blank learner identifier, when enrollment is requested, then it is rejected and no record is created.
  - Given an identifier that is not a known learner, when enrollment is requested, then it is rejected and no record is created.

#### REQ-FUN-005: Report the existing place on a repeated request
- **Statement:** The system shall report the learner's existing status without creating anything new when an enrollment request comes from a learner who is already enrolled or already on the waiting list.
- **Traceability ID:** REQ-FUN-005
- **Policy refs:** POL-DUP-001
- **Inputs:** Learner identifier of a learner who already holds a place.
- **Expected Outputs:** Outcome "already enrolled" or "already waitlisted" with the current position; nothing changes.
- **Acceptance Criteria:**
  - Given an enrolled learner, when they ask to enroll again, then they receive "already enrolled" and the enrolled count is unchanged.
  - Given a learner who is second on the waiting list, when they ask to enroll again, then they receive "already waitlisted", are still second, and the list length is unchanged.

#### REQ-FUN-006: Treat a request after dropping as a new request
- **Statement:** The system shall treat an enrollment request as brand new when it comes from a learner who previously dropped, and shall place that learner at the end of the waiting list when the offering is full.
- **Traceability ID:** REQ-FUN-006
- **Policy refs:** POL-DUP-001, POL-WL-001
- **Inputs:** Learner identifier of a learner who previously dropped.
- **Expected Outputs:** Outcome "enrolled" if a seat is free; otherwise "waitlisted" at the last position.
- **Acceptance Criteria:**
  - Given a learner who dropped and a full offering with two waiting learners, when the learner asks to enroll, then they are placed third on the waiting list.
  - Given a learner who dropped and an offering with a free seat, when the learner asks to enroll, then they are enrolled.

#### REQ-FUN-007: Report a learner's status
- **Statement:** The system shall report whether a learner is enrolled, on the waiting list (with their position), or neither when a status request is made for a known learner.
- **Traceability ID:** REQ-FUN-007
- **Policy refs:** POL-DUP-001
- **Inputs:** Learner identifier.
- **Expected Outputs:** One of "enrolled", "waitlisted at position N", or "not enrolled or waitlisted"; nothing changes. For an invalid learner, "rejected: unknown learner".
- **Acceptance Criteria:**
  - Given an enrolled learner, when status is requested, then "enrolled" is reported.
  - Given a learner third on the waiting list, when status is requested, then "waitlisted at position 3" is reported.
  - Given a known learner with no place, when status is requested, then "not enrolled or waitlisted" is reported.

### 4.2 Dropping

#### REQ-FUN-008: Drop an enrolled learner
- **Statement:** The system shall remove a learner's enrollment and free their seat when an enrolled learner drops.
- **Traceability ID:** REQ-FUN-008
- **Policy refs:** POL-WL-001
- **Inputs:** Learner identifier of an enrolled learner.
- **Expected Outputs:** Outcome "dropped"; the enrolled count goes down by one before any promotion; if a learner is promoted, the outcome also names the promoted learner.
- **Acceptance Criteria:**
  - Given an enrolled learner and an empty waiting list, when the learner drops, then they are no longer enrolled and the enrolled count is one lower.
  - Given an enrolled learner, when they drop, then exactly one enrollment is removed.

#### REQ-FUN-009: Leave the waiting list
- **Statement:** The system shall remove a learner from the waiting list, without changing the number of enrolled learners and without promoting anyone, when a learner who is on the waiting list drops.
- **Traceability ID:** REQ-FUN-009
- **Policy refs:** POL-WL-001, POL-DUP-001
- **Inputs:** Learner identifier of a waiting learner.
- **Expected Outputs:** Outcome "removed from waiting list"; learners behind them each move up one place and keep their relative order.
- **Acceptance Criteria:**
  - Given a learner who is second of three on the waiting list, when they drop, then the list has two learners in their original relative order and the enrolled count is unchanged.
  - Given a waiting learner who drops, then no waiting learner is promoted.

#### REQ-FUN-010: Reject a drop from a learner with no place
- **Statement:** The system shall reject a drop request and change nothing when the learner is known but is neither enrolled nor on the waiting list.
- **Traceability ID:** REQ-FUN-010
- **Policy refs:** POL-DUP-001
- **Inputs:** Learner identifier of a known learner with no place, including a learner who already dropped.
- **Expected Outputs:** Outcome "rejected: not enrolled"; nothing changes.
- **Acceptance Criteria:**
  - Given a known learner who never enrolled, when they ask to drop, then the request is rejected and the enrolled count is unchanged.
  - Given a learner who has already dropped, when they ask to drop again, then the request is rejected and the enrolled count does not fall a second time.

#### REQ-FUN-011: Reject a drop from an invalid learner
- **Statement:** The system shall reject a drop request and change nothing when the learner identifier is missing or does not belong to a known learner.
- **Traceability ID:** REQ-FUN-011
- **Policy refs:** POL-PRE-001
- **Inputs:** A blank, missing, or unrecognized learner identifier.
- **Expected Outputs:** Outcome "rejected: unknown learner"; nothing changes.
- **Acceptance Criteria:**
  - Given an unrecognized identifier, when a drop is requested, then it is rejected and the enrolled count and waiting list are unchanged.

### 4.3 FIFO Promotion

#### REQ-FUN-012: Promote the earliest eligible waiting learner
- **Statement:** The system shall give a freed seat to the earliest-waiting learner who is eligible, and no other learner, when an enrolled learner drops and the waiting list is not empty.
- **Traceability ID:** REQ-FUN-012
- **Policy refs:** POL-WL-001, POL-PRE-001
- **Inputs:** A seat freed by a drop; the current waiting list.
- **Expected Outputs:** The promoted learner becomes enrolled and is removed from the waiting list; the enrolled count returns to its pre-drop value; at most one learner is promoted per freed seat.
- **Acceptance Criteria:**
  - Given a full offering and three eligible waiting learners A, B, C (in that order), when an enrolled learner drops, then A is enrolled, B and C remain waiting in that order, and the enrolled count equals capacity.
  - Given a full offering with several waiting learners, when one enrolled learner drops, then exactly one waiting learner is promoted.

#### REQ-FUN-013: Re-check the prerequisite before promoting
- **Statement:** The system shall confirm that a waiting learner meets the prerequisite at the moment of promotion, and shall not promote a learner who no longer meets it, when a seat opens.
- **Traceability ID:** REQ-FUN-013
- **Policy refs:** POL-PRE-001
- **Inputs:** The waiting learner under consideration and their current prerequisite status.
- **Expected Outputs:** A learner who fails the check is not enrolled.
- **Acceptance Criteria:**
  - Given a learner who met the prerequisite when joining the waiting list but no longer meets it when a seat opens, when the seat opens, then that learner is not promoted.

#### REQ-FUN-014: Skip an ineligible learner and keep their place
- **Statement:** The system shall skip a waiting learner who is not eligible, leave them on the waiting list in their original position, and continue to the next waiting learner, when a seat opens.
- **Traceability ID:** REQ-FUN-014
- **Policy refs:** POL-WL-001, POL-PRE-001
- **Inputs:** The waiting list in order, with one or more ineligible learners at the front.
- **Expected Outputs:** The earliest eligible learner behind them is promoted; skipped learners keep their original positions relative to each other and to learners not promoted.
- **Acceptance Criteria:**
  - Given waiting learners A (ineligible), B (eligible), C (eligible), when a seat opens, then B is enrolled, A stays ahead of C on the list, and A is not enrolled.
  - Given A later becomes eligible and another seat opens, then A is promoted before C because A kept their original place.

#### REQ-FUN-015: Leave the seat open when nobody is eligible
- **Statement:** The system shall leave a freed seat vacant and promote no one when a seat opens and no waiting learner is eligible.
- **Traceability ID:** REQ-FUN-015
- **Policy refs:** POL-WL-001, POL-PRE-001
- **Inputs:** A freed seat; a waiting list where every learner is ineligible.
- **Expected Outputs:** The seat stays open; all waiting learners remain in their positions; the list is checked again the next time a seat opens.
- **Acceptance Criteria:**
  - Given a waiting list in which every learner is ineligible, when a seat opens, then no one is enrolled and the enrolled count is one below capacity.
  - Given the seat is still open and a later seat opens, then the waiting list is checked again and the earliest eligible learner at that time is promoted.

#### REQ-FUN-016: Keep the waiting list in joining order
- **Statement:** The system shall keep waiting learners in the order in which they first joined, unaffected by repeated requests, skipped promotions, or other learners leaving, when the waiting list changes.
- **Traceability ID:** REQ-FUN-016
- **Policy refs:** POL-WL-001, POL-DUP-001
- **Inputs:** Any request that changes or reads the waiting list.
- **Expected Outputs:** The relative order of learners who stay on the list never changes.
- **Acceptance Criteria:**
  - Given learners A, B, C who joined in that order, when B repeats an enrollment request, then the order stays A, B, C.
  - Given A, B, C in order, when A is skipped during a promotion and B is promoted, then the remaining order is A, C.

### 4.4 State Guardrails

#### REQ-FUN-017: Never exceed capacity
- **Statement:** The system shall refuse to enroll any learner when doing so would put the number of enrolled learners above the offering's capacity.
- **Traceability ID:** REQ-FUN-017
- **Policy refs:** POL-WL-001
- **Inputs:** Any request that could add an enrollment (enroll, promotion).
- **Expected Outputs:** The enrolled count is never greater than capacity.
- **Acceptance Criteria:**
  - Given a full offering, when eligible learners keep asking to enroll, then the enrolled count stays equal to capacity and the extra learners wait.
  - Given any sequence of enroll and drop requests, then the enrolled count never exceeds capacity.

#### REQ-FUN-018: One place per learner
- **Statement:** The system shall ensure a learner is never both enrolled and on the waiting list, and never appears more than once in either, when any request is processed.
- **Traceability ID:** REQ-FUN-018
- **Policy refs:** POL-DUP-001
- **Inputs:** Any request.
- **Expected Outputs:** Each learner holds zero or one place.
- **Acceptance Criteria:**
  - Given any sequence of enroll, drop, and promotion requests, then no learner appears twice across the enrolled group and the waiting list combined.
  - Given a waiting learner who is promoted, then they no longer appear on the waiting list.

#### REQ-FUN-019: Rejected requests change nothing
- **Statement:** The system shall leave the enrolled learners, the capacity, and the waiting list exactly as they were when a request is rejected.
- **Traceability ID:** REQ-FUN-019
- **Policy refs:** POL-PRE-001, POL-DUP-001
- **Inputs:** Any rejected request (not eligible, unknown learner, repeated request, drop with no place).
- **Expected Outputs:** The state before and after the request is identical.
- **Acceptance Criteria:**
  - Given any rejected request, when the enrolled learners and waiting list are compared before and after, then they are identical.

#### REQ-FUN-020: Capacity must be a positive number
- **Statement:** The system shall refuse to set up the offering when the capacity is not a whole number greater than zero.
- **Traceability ID:** REQ-FUN-020
- **Policy refs:** none
- **Inputs:** The capacity value supplied when the offering is set up.
- **Expected Outputs:** A clear rejection; no offering is created with an invalid capacity.
- **Acceptance Criteria:**
  - Given a capacity of zero or a negative number, when the offering is set up, then the setup is rejected.
  - Given a capacity of 1 or more, when the offering is set up, then it is accepted.

## 5. Non-Functional Requirements

### 5.1 Concurrency

#### REQ-NFR-001: Never over-fill seats under simultaneous requests
- **Statement:** The system shall never let the number of enrolled learners exceed capacity when several enrollment requests arrive at the same time.
- **Acceptance Criteria:** Given one free seat and many eligible learners asking at the same moment, exactly one is enrolled and the rest are placed on the waiting list.

#### REQ-NFR-002: Never promote twice under simultaneous requests
- **Statement:** The system shall promote a waiting learner at most once, and give each freed seat to at most one learner, when several drops or enrollments happen at the same time.
- **Acceptance Criteria:** Given several simultaneous drops, the number of promotions equals the number of freed seats, and no learner is promoted twice.

#### REQ-NFR-003: A drop and its promotion happen as one step
- **Statement:** The system shall make a seat opening and the resulting promotion appear as one complete step to every other request when other requests arrive at the same time.
- **Acceptance Criteria:** Given an enrollment request arriving during a drop, the request never takes the freed seat ahead of the earliest eligible waiting learner, and no request sees a half-finished result.

### 5.2 State Consistency

#### REQ-NFR-004: Reported numbers always match the real state
- **Statement:** The system shall keep the reported enrolled count and waiting-list positions equal to the actual enrolled learners and waiting list after every request, whether it succeeded or was rejected.
- **Acceptance Criteria:** After any series of requests, the enrolled count equals the number of enrolled learners, and waiting-list positions run 1, 2, 3, … with no gaps or repeats.

#### REQ-NFR-005: Same requests, same result
- **Statement:** The system shall produce the same enrollment and waiting-list results when the same series of requests is made in the same order from the same starting state.
- **Acceptance Criteria:** Running an identical series of requests twice from the same starting state gives identical final enrolled learners and waiting-list order.

### 5.3 Error Handling

#### REQ-NFR-006: Every request gets a clear outcome
- **Statement:** The system shall give every request a clear outcome that states what happened and why when the request is handled.
- **Acceptance Criteria:** Each request ends in exactly one of: enrolled, waitlisted, already enrolled, already waitlisted, rejected (prerequisite not met), rejected (unknown learner), dropped, removed from waiting list, rejected (not enrolled).

#### REQ-NFR-007: A failed request leaves no partial changes
- **Statement:** The system shall leave no partial changes behind when a request cannot be completed for any reason.
- **Acceptance Criteria:** Given a request that fails part-way, the enrolled learners and waiting list are the same as before the request.

## 6. Requirements Traceability Matrix

### 6.1 Requirement to Verification

| Requirement ID | Feature Set | Verification Method | Test Level | Test Case Reference |
|----------------|-------------|---------------------|------------|---------------------|
| REQ-FUN-001 | Enrollment | Unit Test | Unit | TC-FUN-001 |
| REQ-FUN-002 | Enrollment | Unit Test | Unit | TC-FUN-002 |
| REQ-FUN-003 | Enrollment | Unit Test | Unit | TC-FUN-003 |
| REQ-FUN-004 | Enrollment | Unit Test | Unit | TC-FUN-004 |
| REQ-FUN-005 | Enrollment | Unit Test | Unit | TC-FUN-005 |
| REQ-FUN-006 | Enrollment | Unit Test | Unit | TC-FUN-006 |
| REQ-FUN-007 | Enrollment | Unit Test | Unit | TC-FUN-007 |
| REQ-FUN-008 | Dropping | Unit Test | Unit | TC-FUN-008 |
| REQ-FUN-009 | Dropping | Unit Test | Unit | TC-FUN-009 |
| REQ-FUN-010 | Dropping | Unit Test | Unit | TC-FUN-010 |
| REQ-FUN-011 | Dropping | Unit Test | Unit | TC-FUN-011 |
| REQ-FUN-012 | FIFO Promotion | Unit Test | Unit | TC-FUN-012 |
| REQ-FUN-013 | FIFO Promotion | Unit Test | Unit | TC-FUN-013 |
| REQ-FUN-014 | FIFO Promotion | Unit Test | Unit | TC-FUN-014 |
| REQ-FUN-015 | FIFO Promotion | Unit Test | Unit | TC-FUN-015 |
| REQ-FUN-016 | FIFO Promotion | Unit Test | Unit | TC-FUN-016 |
| REQ-FUN-017 | State Guardrails | Integration Test | Integration | TC-FUN-017 |
| REQ-FUN-018 | State Guardrails | Integration Test | Integration | TC-FUN-018 |
| REQ-FUN-019 | State Guardrails | Unit Test | Unit | TC-FUN-019 |
| REQ-FUN-020 | State Guardrails | Unit Test | Unit | TC-FUN-020 |
| REQ-NFR-001 | Concurrency | Concurrency/Stress Test | Integration | TC-NFR-001 |
| REQ-NFR-002 | Concurrency | Concurrency/Stress Test | Integration | TC-NFR-002 |
| REQ-NFR-003 | Concurrency | Concurrency/Stress Test | Integration | TC-NFR-003 |
| REQ-NFR-004 | State Consistency | Integration Test | Integration | TC-NFR-004 |
| REQ-NFR-005 | State Consistency | Integration Test | Integration | TC-NFR-005 |
| REQ-NFR-006 | Error Handling | Inspection | Review | TC-NFR-006 |
| REQ-NFR-007 | Error Handling | Integration Test | Integration | TC-NFR-007 |

### 6.2 Problem Statement Coverage

| Source Item | Covered By |
|-------------|------------|
| AC1 Available capacity | REQ-FUN-001, REQ-FUN-017 |
| AC2 Full capacity | REQ-FUN-002, REQ-FUN-017 |
| AC3 Duplicate requests | REQ-FUN-005, REQ-FUN-016, REQ-FUN-018 |
| AC4 FIFO promotion | REQ-FUN-012, REQ-FUN-016 |
| AC5 Ineligible learner at the front | REQ-FUN-014, REQ-FUN-015 |
| AC6 Prerequisite failure | REQ-FUN-003, REQ-FUN-013 |
| AC7 Invalid and repeated drops | REQ-FUN-010, REQ-FUN-011, REQ-FUN-019 |
| AC8 Tests for the listed cases | REQ-FUN-001 to REQ-FUN-015 (each verified by its Test Case Reference) |
| AC9 Concurrency | REQ-NFR-001, REQ-NFR-002, REQ-NFR-003 |
| Invariant: count never exceeds capacity | REQ-FUN-017, REQ-NFR-001 |
| Invariant: one place per learner | REQ-FUN-018, REQ-FUN-005 |
| Invariant: only eligible learners enter or are promoted | REQ-FUN-003, REQ-FUN-013 |
| Invariant: a drop removes exactly one enrollment | REQ-FUN-008, REQ-FUN-010 |
| Invariant: no eligible waiting learner leaves the seat open | REQ-FUN-015 |
| Invariant: rejected operations change nothing | REQ-FUN-019, REQ-NFR-007 |
