# 2. Requirements

## 2.1 Functional requirements

The requirements below were derived from the specified scenario. Each is realised
by one or more endpoints in the delivered interface.

**Table 1.** Functional requirements.

| Ref | Requirement |
|---|---|
| FR1 | Register a candidate account and issue an access token. |
| FR2 | Authenticate by electronic mail and password, rejecting invalid credentials without disclosing whether the account exists. |
| FR3 | Maintain a candidate profile: headline, location, experience, education. |
| FR4 | Store an uploaded curriculum vitae and extract the skills evidenced within it. |
| FR5 | Search published postings by keyword, country, city, work mode, employment type and seniority. |
| FR6 | Submit an application to a published posting, rejecting a duplicate application. |
| FR7 | Score each application against its posting on submission and present the breakdown to the candidate. |
| FR8 | Recommend published postings to a candidate in descending order of fit. |
| FR9 | Track and withdraw an application. |
| FR10 | Create a posting as a draft with weighted skill requirements and a ranking strategy. |
| FR11 | Publish, close and duplicate a posting. |
| FR12 | Present the applicant pool ranked by fit, each entry carrying its score breakdown. |
| FR13 | Progress an application through the pipeline, rejecting transitions that are not permitted. |
| FR14 | Record every state change as a timeline entry attributable to the acting user. |
| FR15 | Present a hiring manager with shortlisted candidates only. |
| FR16 | Present an administrator with organisations, departments and record counts. |
| FR17 | Record authentication attempts and entity modifications in an audit trail. |

## 2.2 Non-functional requirements

**Table 2.** Non-functional requirements.

| Ref | Requirement |
|---|---|
| NFR1 | Passwords stored using BCrypt at work factor twelve, never reversibly. |
| NFR2 | Authorisation enforced on the server for every protected endpoint, independently of the client. |
| NFR3 | Scoring deterministic: identical input produces identical output. |
| NFR4 | Scoring operates with no external network call. |
| NFR5 | Concurrent modification of an application detected and rejected, not silently overwritten. |
| NFR6 | Security events recorded in an append-only trail retaining acting user and network address. |
| NFR7 | The interface remains usable from a viewport width of 360 pixels. |
| NFR8 | Interactive elements reachable by keyboard and carrying accessible labels. |
| NFR9 | The interface documented using OpenAPI and exercisable from a browser. |
| NFR10 | Database unavailability does not prevent startup; the condition is reported through a health endpoint. |

## 2.3 Features of the application

**Candidate portal.** Registration and authentication, profile management,
curriculum vitae storage, filtered job search, application submission with an
optional covering letter, an application list, a detail view presenting the score
breakdown and a status timeline, withdrawal, and ranked recommendations.

**Recruiter portal.** Posting creation with weighted skill requirements and a
selectable ranking strategy, publication, closure and duplication, the ranked
applicant pool with an expandable factor breakdown per applicant, and pipeline
progression.

**Hiring manager portal.** A view restricted to shortlisted candidates, each
carrying the same reasoning shown to the recruiter, together with decision
actions.

**Administration portal.** Organisation and department directory, record counts
across the core tables, and the recent security event feed drawn from the audit
trail.
