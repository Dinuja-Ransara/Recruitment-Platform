# 6. System Demonstration

The screenshots referenced below were captured from the deployed instance at
`https://meridian-talent.pages.dev`, operating against the SQL Server database
described in Chapter 3.

## 6.1 User interfaces

> **Figure 4.** Public landing page, presenting the ranking of a live applicant
> pool rather than a description of the capability.

> **Figure 5.** Authentication screen, showing the four demonstration accounts.

> **Figure 6.** Candidate overview, showing scored posting recommendations and
> current application activity.

> **Figure 7.** Job search with keyword, country and work mode filters applied.

> **Figure 8.** Recruiter posting list, showing published and draft states with
> the publish, close and duplicate actions.

> **Figure 9.** Hiring manager shortlist, restricted to candidates advanced past
> screening.

> **Figure 10.** Administration overview, showing record counts and the recent
> security event feed.

## 6.2 Artificial intelligence functionality

Figure 11 is the central demonstration. It presents the ranked applicant pool for
the senior backend engineering position, ten applicants ordered by fit.

The ordering is defensible rather than merely plausible. The most experienced
applicant, a principal engineer of twelve years, is ranked fifth because the
mandatory ASP.NET Core requirement is not evidenced in his curriculum vitae. A
ranking driven by seniority language alone would have placed him near the top.

> **Figure 11.** Ranked applicant pool, showing scores, pipeline stage and
> mandatory requirement warnings.

> **Figure 12.** Expanded score breakdown for the leading applicant, showing each
> factor's measured value, the weight applied and the resulting contribution,
> summing to the headline score, together with evidenced and absent skills.

> **Figure 13.** The same breakdown as presented to the candidate within their own
> application, demonstrating that both parties are shown identical reasoning.

## 6.3 Authentication workflows

> **Figure 14.** Successful authentication returning a signed token together with
> the roles held.

> **Figure 15.** Rejected authentication, returning an identical message whether
> the account is absent or the password incorrect.

> **Figure 16.** A candidate token refused on a recruiter endpoint with status 403,
> demonstrating that authorisation is enforced by the interface and not by the
> client.

## 6.4 Application programming interface testing

The interface was exercised using Postman. Table 2 summarises the access control
verification, each case asserting the status code returned.

**Table 2.** Access control verification results.

| Request | Authenticated as | Expected | Observed |
|---|---|---|---|
| `GET /api/organizations` | none | 401 | 401 |
| `GET /api/organizations` | candidate | 403 | 403 |
| `GET /api/organizations` | recruiter | 200 | 200 |
| `GET /api/admin/system-health` | recruiter | 403 | 403 |
| `GET /api/admin/system-health` | administrator | 200 | 200 |
| `POST /api/auth/login` (wrong password) | none | 401 | 401 |
| `POST /api/jobs` | candidate | 403 | 403 |

> **Figure 17.** Postman collection run summary.

> **Figure 18.** Response body of the ranked applicants endpoint, showing the
> serialised score breakdown.

## 6.5 Automated testing

Thirty unit tests across two suites execute in under ten seconds. Eighteen cover
the scoring engine: determinism under repeated scoring of identical inputs,
ranking stability under reversed input order, the mandatory-requirement cap,
divergence between strategies, summation of factor weights, and the regression
described in Section 4.4. Twelve more, run against a real SQLite-backed
database rather than mocks, cover authentication and business logic directly:
login succeeding and failing correctly, the wrong-password and unknown-email
cases returning an identical message, deactivated accounts being blocked,
candidate registration, and the job-posting ownership rule from Section 6.4,
that creation does not check organisation ownership but every later action does.

> **Figure 19.** Test run output showing all thirty tests passing.

## 6.6 Database implementation

> **Figure 20.** Database schema as created by the migration, showing the nineteen
> tables and their relationships.

> **Figure 21.** Health endpoint reporting the deployed instance as healthy with
> the database reachable.
