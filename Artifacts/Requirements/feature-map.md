---
markmap:
  colorFreezeLevel: 2
  initialExpandLevel: 3
---

# Course Offering Enrollment and Waiting List

## Scope
### In-Scope
- Enroll one learner in the one offering
- Waiting list when the offering is full
- One place per learner
- Drop and seat promotion
- Clear answer for every request
- Status check
### Out-of-Scope
- Multiple offerings or a full registration product
- Screens, catalog, payments, sign-in
- Deployment or a new database
- Configurable rules
- AI-assistance recording

## Policies
### Prerequisites
- POL-PRE-001 Eligibility checked at request and again before promotion
### Waiting-List Promotion
- POL-WL-001 First come, first served; skip ineligible and keep their place
### One Place per Learner
- POL-DUP-001 Repeats change nothing; returning learner joins at the back

## Functional Requirements
### Enrollment
- REQ-FUN-001 Enroll when a seat is free
- REQ-FUN-002 Join the waiting list when full
- REQ-FUN-003 Reject learner who has not met the prerequisite
- REQ-FUN-004 Reject invalid learner on enroll
- REQ-FUN-005 Report existing place on repeated request
- REQ-FUN-006 Treat request after dropping as new
- REQ-FUN-007 Report learner status
### Dropping
- REQ-FUN-008 Drop an enrolled learner
- REQ-FUN-009 Leave the waiting list
- REQ-FUN-010 Reject drop from learner with no place
- REQ-FUN-011 Reject drop from invalid learner
### FIFO Promotion
- REQ-FUN-012 Promote earliest eligible waiting learner
- REQ-FUN-013 Re-check prerequisite before promoting
- REQ-FUN-014 Skip ineligible learner and keep their place
- REQ-FUN-015 Leave seat open when nobody is eligible
- REQ-FUN-016 Keep waiting list in joining order
### State Guardrails
- REQ-FUN-017 Never exceed capacity
- REQ-FUN-018 One place per learner
- REQ-FUN-019 Rejected requests change nothing
- REQ-FUN-020 Capacity must be a positive number

## Non-Functional Requirements
### Concurrency
- REQ-NFR-001 Never over-fill seats under simultaneous requests
- REQ-NFR-002 Never promote twice under simultaneous requests
- REQ-NFR-003 A drop and its promotion happen as one step
### State Consistency
- REQ-NFR-004 Reported numbers always match the real state
- REQ-NFR-005 Same requests, same result
### Error Handling
- REQ-NFR-006 Every request gets a clear outcome
- REQ-NFR-007 A failed request leaves no partial changes

## Confirmed Assumptions
- ASM-006 Enrolled learners keep their seat if prerequisite status changes
- ASM-007 No limit on waiting-list length
- ASM-008 Waiting learners may drop and leave the list

## Baseline
- SRS v1.0 Frozen on 2026-10-25
