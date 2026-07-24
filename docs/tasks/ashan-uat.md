# Ashan: extra task, User Acceptance Testing (UAT)

**Why this matters:** Task 3 of the brief explicitly asks for UAT covering
"end-to-end recruitment workflows including: job application submission,
candidate screening, interview scheduling, recruitment reporting." Your
Postman collection proves the API responds correctly. UAT is different, it
proves a real person can actually get through the workflow **in the browser**,
clicking real buttons, not sending JSON. Both are needed, neither replaces
the other.

This is on top of your Postman work, not instead of it.

## The four workflows, and one honest note upfront

The brief names four workflows. Three exist in the running app. One does
not:

| Workflow | Status |
|---|---|
| Job application submission | Built, test it |
| Candidate screening (ranking) | Built, test it |
| Recruitment reporting | Built, test it (this is the Analytics page) |
| Interview scheduling | **Not built.** No screen, no endpoint exists for it anywhere in the app. |

Don't go hunting for an interview scheduling screen, it isn't there. Note
that honestly in your write-up instead, under a short "Not covered" section.
An honest gap is fine, a fabricated pass is not.

## What to do

Work through each of the three real workflows **in the live site**
(`https://meridian-talent.pages.dev`), as an actual user would, no Postman.
Use the demo account buttons on the sign-in page.

### 1. Job application submission
Sign in as candidate → search for a job → open it → submit an application
with a cover letter → confirm it shows up under "My Applications" with
status Submitted.

### 2. Candidate screening
Sign in as recruiter → open a posting with applicants → open Ranked
Applicants → click "Why this score" on at least one candidate → confirm the
score breakdown makes sense → change an application's status.

### 3. Recruitment reporting
Sign in as recruiter or admin → open Analytics → confirm the funnel, the
posting performance table, and the skills breakdown all show real numbers
that match what you'd expect from the data you just touched in steps 1-2.

## What to capture, per workflow

- A screenshot at each meaningful step (5-8 screenshots per workflow is
  plenty, don't screenshot every click)
- Whether it completed without any error or dead end
- Anything that didn't behave the way a real recruiter or candidate would
  expect, even if it's not technically a bug

Save screenshots into `docs/testing/uat/`, named like
`uat-application-01.png`, `uat-screening-01.png`, etc.

## Write it up

Create `docs/testing/uat.md` with one section per workflow:

- What you did, step by step (short, this is a log not an essay)
- Screenshot references
- Pass / Fail
- The honest "Not covered: interview scheduling" note at the end, one
  sentence on why (not built in this version, domain model exists but no
  API or UI surface).

## Commit it

```bash
git checkout main
git pull origin main
git checkout -b work/ashan-uat
git add docs/testing/uat.md docs/testing/uat/
git commit -m "Add end-to-end UAT covering application, screening and reporting workflows"
git push -u origin work/ashan-uat
```

Then tell Hasitha it's pushed, same as always. Don't merge it yourself.
