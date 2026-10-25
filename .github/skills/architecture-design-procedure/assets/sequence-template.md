# Sequence Flows: <System Name>

## 1. Successful Enrollment & Full-Capacity Waitlist Redirect
Covers: REQ-FUN-XXX

```mermaid
sequenceDiagram
    actor L as Learner
    participant EC as EnrollmentController
    participant COS as CourseOfferingService
    participant WQM as WaitlistQueueManager
    participant IMS as InMemoryStore

    L->>EC: enroll(learnerId, offeringId)
    EC->>COS: enroll(learnerId, offeringId)
    Note over COS,IMS: acquire lock(offeringId)
    COS->>IMS: load offering, enrollments
    alt seat available
        COS->>IMS: save enrollment
        COS-->>EC: ENROLLED
    else offering full
        COS->>WQM: enqueue(learnerId, offeringId)
        WQM->>IMS: append to waitlist
        WQM-->>COS: position
        COS-->>EC: WAITLISTED(position)
    end
    Note over COS,IMS: release lock(offeringId)
    EC-->>L: response
```

## 2. Learner Drop Triggering FIFO Waitlist Promotion
Covers: REQ-FUN-XXX

```mermaid
sequenceDiagram
    actor L as Learner
    participant EC as EnrollmentController
    participant COS as CourseOfferingService
    participant WQM as WaitlistQueueManager
    participant IMS as InMemoryStore

    L->>EC: drop(learnerId, offeringId)
    EC->>COS: drop(learnerId, offeringId)
    Note over COS,IMS: acquire lock(offeringId)
    COS->>IMS: remove enrollment
    COS->>WQM: promoteNext(offeringId)
    loop until seat filled or queue exhausted
        WQM->>IMS: peek next waitlisted learner (FIFO)
        alt learner eligible
            WQM->>IMS: move learner to enrolled
        else ineligible (Skip & Retain)
            Note over WQM: skip learner, keep original queue position
        end
    end
    Note over COS,IMS: release lock(offeringId)
    COS-->>EC: DROPPED (+ promoted learner, if any)
    EC-->>L: response
```

## 3. Duplicate Request & Error Guardrails
Covers: REQ-FUN-XXX

```mermaid
sequenceDiagram
    actor L as Learner
    participant EC as EnrollmentController
    participant COS as CourseOfferingService
    participant IMS as InMemoryStore

    L->>EC: enroll(learnerId, offeringId)
    EC->>COS: enroll(learnerId, offeringId)
    COS->>IMS: load state
    alt duplicate enrollment or waitlist entry
        COS-->>EC: error DUPLICATE
    else prerequisite not met
        COS-->>EC: error PREREQUISITE_NOT_MET
    else offering not found
        COS-->>EC: error NOT_FOUND
    end
    Note over COS,IMS: state unchanged on error
    EC-->>L: error response
```
