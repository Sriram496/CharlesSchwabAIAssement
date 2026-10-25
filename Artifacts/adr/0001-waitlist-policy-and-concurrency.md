# 0001: Waiting-List Promotion Policy and Concurrency Mechanism

- Status: Accepted
- Date: 2026-10-25
- Deciders: Project owner, with the architecture agent

## Context and Problem Statement

Learners enroll in one course offering and wait in a first-come, first-served line when it is full. Two decisions shape the design: what to do with a waiting learner who is not eligible when a seat opens, and how to keep seat counts and the waiting list correct when several requests arrive at once. Source: [SRS v1.0](../Requirements/SRS.md), POL-WL-001, POL-PRE-001, REQ-NFR-001 to REQ-NFR-007.

## Decision Drivers

- POL-WL-001 and REQ-FUN-014: an ineligible learner at the front is skipped and keeps their place.
- REQ-FUN-013, REQ-FUN-015: eligibility is re-checked at promotion; if nobody is eligible the seat stays open.
- REQ-NFR-001, REQ-NFR-002: seats are never over-filled and nobody is promoted twice, even with simultaneous requests.
- REQ-NFR-003: a drop and its promotion appear as one step.
- REQ-NFR-004, REQ-NFR-007: reported numbers match the real state, and a failed request leaves no partial change.
- Scope: one offering, in memory, no new database or deployment (SRS out-of-scope list).

## Decision A: Waiting-List Promotion Policy

### Considered Options
- Skip & Retain Ineligible Learner
- Remove Ineligible Learner
- Block at the front (stop promoting until the front learner is eligible)

### Pros and Cons of the Options

#### Skip & Retain Ineligible Learner
- Good, because eligible learners are never blocked by an ineligible one.
- Good, because a skipped learner keeps their place and queue age if they become eligible later.
- Bad, because the promotion scan may read several entries (a small cost for one offering).

#### Remove Ineligible Learner
- Good, because the waiting list stays short and every entry is eligible when scanned.
- Bad, because a learner loses their place if the prerequisite status is briefly wrong or changes shortly after.
- Bad, because it contradicts POL-WL-001 and REQ-FUN-014.

#### Block at the front
- Good, because it is the strictest first-come, first-served reading.
- Bad, because one ineligible learner can leave a seat vacant while eligible learners wait.
- Bad, because it contradicts REQ-FUN-014.

### Decision Outcome

Chosen option: **Skip & Retain Ineligible Learner**, because it is required by POL-WL-001 and REQ-FUN-014 and prevents ineligible learners from blocking eligible ones while preserving queue age.

## Decision B: Concurrency and Thread-Safety Mechanism

### Considered Options
1. One exclusive in-process lock around each whole request, including status reads
2. Separate locks for different pieces of state (store, queue)
3. Optimistic or lock-free retry
4. Single-writer worker that handles requests from a queue
5. External single-threaded store such as Redis with atomic scripts

### Pros and Cons of the Options

#### 1. One exclusive in-process lock around each whole request
- Good, because the check, decision and change happen as one step, which meets REQ-NFR-001 to REQ-NFR-003 directly.
- Good, because blocking reads means nobody ever sees a half-finished state (REQ-NFR-004).
- Good, because there is only one lock, so deadlock between locks is impossible.
- Good, because it needs no infrastructure and is simple to test.
- Bad, because requests run one at a time. This is acceptable for one in-memory offering with no I/O.
- Bad, because it protects one process only, and arrival order under heavy load is not guaranteed fair.

#### 2. Separate locks for different pieces of state
- Good, because it allows finer-grained access.
- Bad, because half-finished states can appear between locks, which breaks REQ-NFR-003.

#### 3. Optimistic or lock-free retry
- Good, because of higher throughput.
- Bad, because it is complex, offers no benefit at this scale, and the promotion rules are hard to make lock-free.

#### 4. Single-writer worker
- Good, because it gives strict arrival order and the same "one at a time" guarantee.
- Bad, because it adds a worker lifecycle, error passing and a deadlock risk when called from the worker itself.

#### 5. External single-threaded store such as Redis
- Good, because it survives restarts and could share state across several running copies.
- Bad, because it adds a new datastore and deployment, which the SRS excludes.
- Bad, because it adds network failure modes that strain REQ-NFR-007, and eligibility data would have to move into it or the check-then-act race returns.

### Decision Outcome

Chosen option: **1. One exclusive in-process lock around each whole request**, because it meets the concurrency requirements with the least machinery and stays within the SRS scope.

Rules that follow from the decision:
1. **Scope:** `CourseOfferingService` takes the lock for the whole of `Enroll`, `Drop`, and `GetStatus`. The lock covers the offering (the "row"); there is one offering, so one lock. A drop and its promotion run under a single lock.
2. **Decide, then apply:** inside the lock, read and decide first with no changes; only then apply all changes together. For a drop, choose the promotion candidate before removing the enrollment. A failure during deciding leaves nothing changed (REQ-NFR-007).
3. **Release on every exit:** use the language's lock block so the lock is released if an exception occurs.
4. **No lock calls in lock:** no component calls back into `CourseOfferingService` while the lock is held.
5. **Single owner of change:** `WaitlistQueueManager` and the store are not safe for simultaneous use on their own and are reached only through the service.
6. **Eligibility:** the check and the seat change happen in the same locked step. `EligibilityProvider` stays a separate component and is safe for simultaneous updates on its own; its data may change outside the lock (see LIM-003).
7. **Store seam:** components depend on `IOfferingStore` so a different store can replace `InMemoryStore` later without changing the rules.

## Consequences

- Good: seat counts, the waiting list and reported status are always consistent.
- Good: a clear, testable concurrency guarantee that maps to each REQ-NFR.
- Bad: requests are serialized, so throughput is limited to one request at a time (acceptable at this scale).
- Bad: state is lost on restart and only one running copy is supported ([LIM-002](../limitations.md)).
- Bad: eligibility is a point-in-time answer ([LIM-003](../limitations.md)).
- Follow-ups: if state must survive restarts or be shared across copies, revisit option 5 behind `IOfferingStore` with a new SRS version.
