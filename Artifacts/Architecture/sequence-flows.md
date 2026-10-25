# Sequence Flows: Course Offering Enrollment and Waiting List

Participants match the components in [c4-architecture.md](./c4-architecture.md). The lock notes mark the one indivisible step per request (REQ-NFR-003); the mechanism is decided in ADR 0001.

## 1. Successful Enrollment and Full-Capacity Waiting-List Redirect
Covers: REQ-FUN-001, REQ-FUN-002, REQ-FUN-005, REQ-FUN-006, REQ-FUN-017, REQ-FUN-018

```mermaid
sequenceDiagram
    actor L as Learner
    participant EC as EnrollmentController
    participant COS as CourseOfferingService
    participant EL as EligibilityProvider
    participant WQM as WaitlistQueueManager
    participant IMS as InMemoryStore

    L->>EC: enroll(learnerId)
    EC->>COS: Enroll(learnerId)
    Note over COS,IMS: one indivisible step begins
    COS->>EL: IsKnownLearner / HasMetPrerequisite
    COS->>IMS: load enrolled, capacity
    alt seat free
        COS->>IMS: add to enrolled
        COS-->>EC: Enrolled
    else offering full
        COS->>WQM: Join(learnerId, now)
        WQM->>IMS: append entry with next sequence
        WQM-->>COS: position
        COS-->>EC: Waitlisted(position)
    end
    Note over COS,IMS: step ends
    EC-->>L: outcome
```

## 2. Learner Drop Triggering FIFO Waiting-List Promotion
Covers: REQ-FUN-008, REQ-FUN-009, REQ-FUN-012, REQ-FUN-013, REQ-FUN-014, REQ-FUN-015, REQ-FUN-016

```mermaid
sequenceDiagram
    actor L as Learner
    participant EC as EnrollmentController
    participant COS as CourseOfferingService
    participant WQM as WaitlistQueueManager
    participant EL as EligibilityProvider
    participant IMS as InMemoryStore

    L->>EC: drop(learnerId)
    EC->>COS: Drop(learnerId)
    Note over COS,IMS: one indivisible step begins
    alt learner is enrolled
        COS->>IMS: remove enrollment
        COS->>WQM: PromoteNext(now)
        loop waiting entries in joining order
            WQM->>EL: HasMetPrerequisite(candidate) now
            alt eligible
                WQM->>IMS: remove entry, add to enrolled
                WQM-->>COS: promoted learner
            else ineligible (Skip and Retain)
                Note over WQM: skip, keep original place
            end
        end
        Note over WQM: no eligible learner leaves the seat open
        COS-->>EC: Dropped(promoted learner or none)
    else learner is waiting
        COS->>WQM: Remove(learnerId)
        COS-->>EC: RemovedFromWaitlist
    end
    Note over COS,IMS: step ends
    EC-->>L: outcome
```

## 3. Duplicate Requests and Error Guardrails
Covers: REQ-FUN-003, REQ-FUN-004, REQ-FUN-005, REQ-FUN-010, REQ-FUN-011, REQ-FUN-019, REQ-NFR-006, REQ-NFR-007

```mermaid
sequenceDiagram
    actor L as Learner
    participant EC as EnrollmentController
    participant COS as CourseOfferingService
    participant EL as EligibilityProvider
    participant IMS as InMemoryStore

    L->>EC: enroll or drop (learnerId)
    alt blank identifier
        EC-->>L: RejectedUnknownLearner
    else identifier present
        EC->>COS: request
        COS->>EL: IsKnownLearner
        alt unknown learner
            COS-->>EC: RejectedUnknownLearner
        else enroll and already holds a place
            COS->>IMS: read only
            COS-->>EC: AlreadyEnrolled or AlreadyWaitlisted
        else enroll and prerequisite not met
            COS-->>EC: RejectedPrerequisiteNotMet
        else drop and holds no place
            COS-->>EC: RejectedNotEnrolled
        end
        Note over COS,IMS: state unchanged on every rejection
        EC-->>L: outcome
    end
```
