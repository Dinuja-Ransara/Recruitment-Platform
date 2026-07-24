# User Acceptance Testing (UAT)

**Site:** https://meridian-talent.pages.dev
**Tester:** Ashan Iduranga
**Date:** 2026-07-24
**Method:** Manual, end-to-end testing in the live browser app using the demo account buttons on the sign-in page (no Postman/API calls).

## Summary

| Workflow | Result |
|---|---|
| Job application submission | ✅ Pass |
| Candidate screening | ✅ Pass |
| Recruitment reporting | ✅ Pass |
| Interview scheduling | Not covered (see note below) |

---

## 1. Job Application Submission

**Goal:** Confirm a candidate can find a job, submit an application with a cover letter, and see it reflected in "My Applications."

**Steps:**
1. Opened the sign-in page and signed in using the candidate demo account button. — `uat-application-01.png`
2. Searched for a job using the search bar. — `uat-application-02.png`
3. Opened the job listing to view its details. — `uat-application-03.png`
4. Clicked Apply, wrote a cover letter, and submitted the application. — `uat-application-04.png`
5. Opened "My Applications" and confirmed the new application appears with status **Submitted**. — `uat-application-05.png`

**Result:** ✅ Pass — completed without errors or dead ends. The application flow behaved as expected from a candidate's perspective.

---

## 2. Candidate Screening

**Goal:** Confirm a recruiter can view ranked applicants for a job, inspect an individual score breakdown, and update an application's status.

**Steps:**
1. Signed out and signed back in using the recruiter demo account button. — `uat-screening-01.png`
2. Opened a job posting that had applicants. — `uat-screening-02.png`
3. Opened Ranked Applicants and viewed the ranked candidate list. — `uat-screening-03.png`
4. Clicked "Why this score" on a candidate and reviewed the score breakdown, which made sense relative to the candidate's profile. — `uat-screening-04.png`
5. Changed the application's status (moved to "In Review") and confirmed the update. — `uat-screening-05.png`

**Result:** ✅ Pass — ranking, score breakdown, and status change all worked correctly with no errors.

---

## 3. Recruitment Reporting

**Goal:** Confirm the Analytics page reflects real data — including the funnel, posting performance, and skills breakdown — consistent with the activity performed in Workflows 1 and 2.

**Steps:**
1. Opened Analytics as recruiter (admin access also available). — `uat-reporting-01.png`
2. Reviewed the recruitment funnel view. — `uat-reporting-02.png`
3. Reviewed the posting performance table. — `uat-reporting-03.png`
4. Reviewed the skills breakdown section.
5. Confirmed the numbers matched expectations given the new application submitted in Workflow 1 and the status change made in Workflow 2.

**Result:** ✅ Pass — all three report sections (funnel, posting performance, skills breakdown) displayed real, consistent numbers with no placeholder or broken data.

---

## Not Covered

**Interview scheduling** was not tested because it does not exist in this version of the application — there is no screen and no API endpoint for it anywhere in the app. The domain model may support the concept, but no UI or API surface has been built yet, so this workflow could not be exercised.

## Conclusion

All three workflows that exist in the live application — job application submission, candidate screening, and recruitment reporting — were tested end-to-end as a real user and passed without errors or dead ends. Interview scheduling remains unbuilt and is explicitly excluded from this round of UAT.
