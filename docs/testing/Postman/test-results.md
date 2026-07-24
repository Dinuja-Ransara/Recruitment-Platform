# Meridian API — Test Results

**Collection:** Meridian API (Postman)
**Environment:** Meridian (`baseUrl = http://meridiantalent.runasp.net/api`)
**Tester:** Ashan Iduranga
**Date:** 2026-07-24

## Summary

All endpoints in the collection were executed end-to-end using the Postman Collection Runner and manual requests, covering authentication, job posting management, the application pipeline, AI-based ranking, and role-based access control (security) checks. All requests returned their expected status codes.

| Folder | Requests | Result |
|---|---|---|
| Auth | 6 | ✅ Pass |
| Jobs | 6 | ✅ Pass |
| Applications | 7 | ✅ Pass |
| Security | 5 | ✅ Pass |

---

## 1. Auth

| # | Request | Method | Endpoint | Expected | Actual | Result |
|---|---|---|---|---|---|---|
| 1 | Login - Admin | POST | `/auth/login` | 200 | 200 | ✅ Pass |
| 2 | register | POST | `/auth/register/candidate` | 200/201 | 200 | ✅ Pass |
| 3 | Login - Manager | POST | `/auth/login` | 200 | 200 | ✅ Pass |
| 4 | Login - Recruiter | POST | `/auth/login` | 200 | 200 | ✅ Pass |
| 5 | Login - Candidate | POST | `/auth/login` | 200 | 200 | ✅ Pass |
| 6 | /me | GET | `/auth/me` | 200 | 200 | ✅ Pass |

**Notes:**
- `Login - Recruiter` and `Login - Candidate` each store their JWT into `recruiterToken` and `candidateToken` collection variables via test scripts, used by all downstream requests.
- `/me` confirms the candidate's bearer token resolves to the correct authenticated profile.

---

## 2. Jobs

| # | Request | Method | Endpoint | Expected | Actual | Result |
|---|---|---|---|---|---|---|
| 1 | Search Jobs | GET | `/jobs` | 200 | 200 | ✅ Pass |
| 2 | My Postings | GET | `/jobs/mine` | 200 | 200 | ✅ Pass |
| 3 | List Organizations (recruiter) | GET | `/organizations` | 200 | 200 | ✅ Pass |
| 4 | Create Job | POST | `/jobs` | 200/201 | 201 | ✅ Pass |
| 5 | Get Job | GET | `/jobs/{{jobId}}` | 200 | 200 | ✅ Pass |
| 6 | Publish Job | POST | `/jobs/{{jobId}}/publish` | 200 | 200 | ✅ Pass |
| 7 | Duplicate Job | POST | `/jobs/{{jobId}}/duplicate` | 200/201 | 201 | ✅ Pass |

**Notes:**
- `Create Job` uses a valid `organizationId` retrieved from `List Organizations (recruiter)` and sets the `jobId` collection variable from the response for use by all subsequent job/application requests.
- `Get Job` and `Publish Job` were verified against the same `jobId` created in this run, confirming the job exists and transitions to a published state successfully.

---

## 3. Applications

| # | Request | Method | Endpoint | Expected | Actual | Result |
|---|---|---|---|---|---|---|
| 1 | Apply | POST | `/applications` | 200/201 | 201 | ✅ Pass |
| 2 | My Applications | GET | `/applications/mine` | 200 | 200 | ✅ Pass |
| 3 | Get Application | GET | `/applications/{{applicationId}}` | 200 | 200 | ✅ Pass |
| 4 | Ranked Applicants | GET | `/applications/job/{{jobId}}/ranked` | 200 | 200 | ✅ Pass |
| 5 | Change Status | POST | `/applications/{{applicationId}}/status` | 200 | 200 | ✅ Pass |
| 6 | Withdraw Application | POST | `/applications/{{applicationId}}/withdraw` | 200/204 | 200 | ✅ Pass |
| 7 | Recommendations | GET | `/applications/recommendations` | 200 | 200 | ✅ Pass |

**Notes:**
- `Apply` submits the candidate's application to the job created in the Jobs folder, and its test script stores the returned `id` into the `applicationId` collection variable.
- `Ranked Applicants` — called with the recruiter token — returned the AI-generated ranking with a full score breakdown per candidate. This is the key evidence endpoint for the application pipeline (see screenshot).
- `Change Status` moves the application to "In Review" (status code `2`) as the recruiter.
- `Withdraw Application` was executed by the candidate to confirm self-service withdrawal works as expected.

---

## 4. Security

| # | Request | Method | Endpoint | Expected | Actual | Result |
|---|---|---|---|---|---|---|
| 1 | organizations | GET | `/organizations` | 200 | 200 | ✅ Pass |
| 2 | organizations as candidate | GET | `/organizations` | 200 | 200 | ✅ Pass |
| 3 | system-health as recruiter | GET | `/admin/system-health` | 403 | 403 | ✅ Pass |
| 4 | Login wrong password | POST | `/auth/login` | 401 | 401 | ✅ Pass |
| 5 | jobs as candidate | POST | `/jobs` | 403 | 403 | ✅ Pass |

**Notes:**
- `system-health as recruiter` confirms recruiters are correctly blocked from admin-only routes (role-based access control).
- `Login wrong password` confirms invalid credentials are rejected with `401 Unauthorized`.
- `jobs as candidate` confirms candidates cannot create job postings, correctly returning `403 Forbidden`.

---

## Evidence

Screenshots of the Postman Collection Runner results are attached alongside this file:

- `1.png` — Auth folder run results (Login - Admin, register, Login - Manager, Login - Recruiter, Login - Candidate, /me)
- `2.png` — Jobs folder run results (Search Jobs, My Postings, List Organizations, Create Job, Get Job, Publish Job, Duplicate Job)
- `3.png` — Applications folder run results — Apply, My Applications, Get Application
- `4.png` — Applications folder run results — **Ranked Applicants response with full AI score breakdown** (primary evidence), Change Status, Withdraw Application, Recommendations
- `5.png` — Security folder run results (organizations, organizations as candidate, system-health as recruiter, Login wrong password, jobs as candidate)

## Conclusion

All 24 requests across the four collection folders (Auth, Jobs, Applications, Security) executed successfully with their expected HTTP status codes. The API correctly enforces authentication, role-based authorization, and the full application lifecycle from job creation through AI-based candidate ranking to status updates and withdrawal.
