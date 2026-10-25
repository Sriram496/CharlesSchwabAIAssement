# Problem Statement: Course Offering Enrollment and Waitlist

## Purpose

Implement the enrollment, dropping, and FIFO waitlist-promotion slice for a single course offering in the provided starter project. Learners may enroll only once in the offering. When capacity is reached, eligible learners may join the waitlist. When an enrolled learner drops, the system should fill the open seat with the earliest eligible learner waiting.


## Scope

### In scope

- Enroll one learner in one offering when capacity is available and the prerequisite is satisfied.
- Add an eligible learner to that offering's waitlist when it is full.
- Prevent duplicate enrollment and duplicate waitlist entries for the same learner and offering.
- Drop an enrolled learner and promote the earliest eligible waitlisted learner when a seat opens.
- Return clear results for invalid requests, failed prerequisite checks, duplicate requests, and drops of learners who are not enrolled.
- Preserve enrollment, capacity, and waitlist invariants across operations.
- Add tests for the specified enrollment, drop, waitlist, prerequisite, and invalid-request cases.


### Out of scope

- A complete registration product or multiple course offerings.
- Frontend, course catalog, payments, identity integrations, or multi-term registration.
- Deployment or a new database
- A configurable prerequisite or waitlist policy engine.

## Assumptions and Policies

### Prerequisite eligibility

- **Assumption:** The starter project supplies the prerequisite data or a way to determine whether a learner has satisfied the offering's prerequisite. If it does not, represent this with a small eligibility check or an in-memory value.
- A learner is eligible only when that check reports that the prerequisite is satisfied.
- Check eligibility before initial enrollment or waitlist entry, and check it again immediately before promotion. A failed check must not enroll or promote the learner.

### Waitlist ordering and promotion

- Maintain a FIFO order for each offering using the time or monotonically increasing sequence at which a learner first joined the waitlist.
- When a seat opens, inspect the queue in FIFO order and promote the first learner who is eligible at that time.
- **If an ineligible learner reaches the front, skip that learner for the current promotion and leave them on the waitlist in their original position.** Continue scanning for the earliest eligible learner. This preserves their queue age if they later satisfy the prerequisite, while preventing them from blocking eligible learners.
- If no waitlisted learner is eligible, leave the seat open. Re-evaluate the queue on a later seat opening; do not promote an ineligible learner.

### Duplicate handling

- A learner may have at most one active enrollment-or-waitlist record per offering.
- Repeating an enrollment request for a learner already enrolled or waitlisted must not create another record or change their queue position. Return a clear result indicating the existing state.
- A dropped learner may submit a new enrollment request later. Treat it as a new request and, if the offering is full, assign a new position at the end of the waitlist.

## Service Boundary and Data Design

Expose a small service boundary around the one offering, with operations equivalent to:

- `Enroll(learnerId)`: enroll if eligible and a seat is available; otherwise waitlist if eligible and full.
- `Drop(learnerId)`: drop an enrolled learner and trigger promotion if a seat opens.
- Optionally, `GetStatus(learnerId)`: report whether the learner is enrolled, waitlisted, or neither.

Use the starter's existing API shape and naming if present. The service should own the state transitions and return explicit outcomes rather than exposing partially updated state.

The minimum state is:

- Offering identifier and positive capacity.
- The set of enrolled learner identifiers.
- An ordered waitlist containing learner identifiers and a stable queue timestamp or sequence.
- Access to the prerequisite eligibility check.

An in-memory implementation is acceptable only when the starter has no persistence requirement. Do not add a new database solely for this feature.

## Invariants and State Transitions

- The enrolled learner count never exceeds the offering capacity.
- A learner cannot be enrolled and waitlisted at the same time, or appear more than once in either state.
- Only eligible learners may enter the waitlist or become enrolled.
- A successful drop removes exactly one existing enrollment. A repeated or invalid drop changes no state.
- Promotion consumes the newly available seat only for an eligible learner; if none is eligible, the seat remains vacant.
- Invalid or rejected operations leave enrollment, capacity, and waitlist state unchanged.

If the starter project supports concurrent requests, retain its transaction or locking approach. The seat-opening and promotion transition must be atomic with respect to other enrollment and drop operations for the offering. For a prototype with no persistence or concurrency mechanism, serialize service operations in memory and document that limitation.

## Acceptance Criteria

1. **Available capacity:** Given an eligible learner and an open seat, enrollment succeeds and the enrolled count increases by one.
2. **Full capacity:** Given a full offering and an eligible learner, enrollment adds that learner to the waitlist once without exceeding capacity.
3. **Duplicate requests:** Repeated enrollment requests for an enrolled or waitlisted learner return a clear existing-state result and do not add records or change waitlist position.
4. **FIFO promotion:** When an enrolled learner drops, the earliest eligible learner in the waitlist is promoted.
5. **Ineligible learner at the front:** If the earliest waiting learner is ineligible at promotion time, that learner remains in the same queue position, is not enrolled, and the system continues to the earliest eligible learner. If no one is eligible, the seat remains vacant.
6. **Prerequisite failure:** An ineligible learner is neither enrolled nor added to the waitlist. A learner who becomes ineligible while waiting is not promoted.
7. **Invalid and repeated drops:** Dropping an invalid learner identifier or a learner who is not enrolled returns a clear failure result and leaves all state unchanged. Repeating a successful drop does not reduce enrollment count again.
8. **Tests:** Tests cover available capacity, full capacity, duplicate requests, FIFO promotion, prerequisite failure, ineligible-at-front handling, and invalid or repeated drops.
9. **Concurrency:** If the starter supports concurrent requests, include a focused test where practical showing that simultaneous requests cannot exceed capacity or cause duplicate promotion.

