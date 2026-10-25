# Software Requirements Specification: <System Name>

## 1. Document Identification & Metadata

| Field | Value |
|-------|-------|
| Title | <System Name> Software Requirements Specification |
| Version | 0.1 |
| Status | Draft |
| Date | <YYYY-MM-DD> |

## 2. System Purpose & Scope Boundaries

**Purpose:** <one or two sentences>

### IN-SCOPE
- <capability>

### OUT-OF-SCOPE
- <explicitly excluded capability>

## 3. Domain Assumptions & Policy Definitions

### Assumptions
- **ASM-001:** <assumption>

### Policies
- **POL-PRE-001 (Prerequisite Rule):** <formal rule: what counts as satisfied, when it is evaluated, what happens on failure>
- **POL-WL-001 (Waitlist Promotion):** <FIFO ordering key, when promotion triggers, what happens if promoted student no longer eligible>
- **POL-DUP-001 (Deduplication):** <what identifies a duplicate request/enrollment, and the resulting behavior>

## 4. Functional Requirements

### 4.1 Enrollment

#### REQ-FUN-001: <Short title>
- **Statement:** The system shall <action> when <condition>.
- **Traceability ID:** REQ-FUN-001
- **Policy refs:** POL-PRE-001
- **Inputs:** <fields>
- **Expected Outputs:** <result and state change>
- **Acceptance Criteria:**
  - Given <context>, when <action>, then <observable result>.
  - Given <negative context>, when <action>, then <rejection/error>.

### 4.2 Dropping

### 4.3 FIFO Promotion

### 4.4 State Guardrails

## 5. Non-Functional Requirements

### 5.1 Concurrency

#### REQ-NFR-001: <Short title>
- **Statement:** The system shall <measurable behavior> when <condition>.
- **Acceptance Criteria:** <measurable threshold>

### 5.2 State Consistency

### 5.3 Error Handling

## 6. Requirements Traceability Matrix

| Requirement ID | Feature Set | Verification Method | Test Level | Test Case Reference |
|----------------|-------------|---------------------|------------|---------------------|
| REQ-FUN-001 | Enrollment | Unit Test | Unit | TC-001 |
