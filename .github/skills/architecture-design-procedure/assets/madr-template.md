# 0001: Waitlist Promotion Policy and Concurrency Mechanism

- Status: Proposed
- Date: <YYYY-MM-DD>
- Deciders: <names>

## Context and Problem Statement

<Why these two decisions are needed; reference SRS sections and REQ/POL IDs.>

## Decision Drivers

- <driver, e.g. REQ-NFR-001 exactly one of N concurrent requests gets the last seat>
- <driver>

## Decision A: Waitlist Promotion Policy

### Considered Options

- Skip & Retain Ineligible Learner
- Remove Ineligible Learner

### Pros and Cons of the Options

#### Skip & Retain Ineligible Learner
- Good, because <reason>
- Bad, because <reason>

#### Remove Ineligible Learner
- Good, because <reason>
- Bad, because <reason>

### Decision Outcome

Chosen option: "<option>", because <justification>.

## Decision B: Concurrency and Thread-Safety Mechanism

### Considered Options

- Mutex lock per course offering
- Synchronized transaction / single global lock

### Pros and Cons of the Options

#### Mutex lock per course offering
- Good, because <reason>
- Bad, because <reason>

#### Synchronized transaction / single global lock
- Good, because <reason>
- Bad, because <reason>

### Decision Outcome

Chosen option: "<option>", because <justification>.

## Consequences

- Good: <consequence>
- Bad: <accepted trade-off>
- Follow-ups: <what to revisit, e.g. when moving beyond InMemoryStore>
