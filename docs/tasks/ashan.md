# Ashan: API testing and evidence

**Your branch:** `work/ashan`
**Your files:** `postman/` and `docs/testing/`. Nothing else, so nothing you do
can conflict with anyone.

**Why this matters:** the coursework marks the prototype partly on "testing
results", and the report needs screenshots of API testing in Chapter 6. Right now
there is no evidence of either. This is the whole of that.

Read [CONTRIBUTING.md](../../CONTRIBUTING.md) first for the git workflow.

## Before you start

Get the API running so you have something to test against. Two options:

- **Live, no setup:** use `https://meridian-talent.pages.dev/api` as your base
  URL. It is deployed and working.
- **Local:** `dotnet run --project src/Meridian.Api --launch-profile http`, then
  the base URL is `http://localhost:5138`.

Every account uses the password `Meridian#2026`:

| Role | Email |
|---|---|
| Administrator | admin@meridian.example.com |
| Recruiter | recruiter@meridian.example.com |
| Hiring Manager | manager@meridian.example.com |
| Candidate | candidate@meridian.example.com |

Swagger lists every endpoint: `https://meridian-talent.pages.dev/api/../swagger`
or `http://localhost:5138/swagger` locally.

---

## Commit 1: Postman environment

Create a Postman environment with two variables, `baseUrl` and `token`. Export it
to `postman/meridian.environment.json`.

```bash
git add postman/meridian.environment.json
git commit -m "Add Postman environment with base URL and token variables"
```

## Commit 2: Authentication requests

Create a collection called **Meridian API** with an `Auth` folder containing:

- `POST {{baseUrl}}/api/auth/login` for each of the four roles
- `POST {{baseUrl}}/api/auth/register/candidate`
- `GET {{baseUrl}}/api/auth/me`

On the login request, add this to the **Tests** tab so the token is captured
automatically and every later request is authenticated:

```javascript
pm.test("Login succeeds", () => pm.response.to.have.status(200));
pm.environment.set("token", pm.response.json().accessToken);
```

Export the collection to `postman/meridian.collection.json`.

```bash
git add postman/meridian.collection.json
git commit -m "Add authentication requests to the Postman collection"
```

## Commit 3: Job posting requests

Add a `Jobs` folder:

- `GET {{baseUrl}}/api/jobs` (public search)
- `GET {{baseUrl}}/api/jobs/mine` (recruiter's own, needs the recruiter token)
- `GET {{baseUrl}}/api/jobs/1`
- `POST {{baseUrl}}/api/jobs` (create; copy a body from Swagger)
- `POST {{baseUrl}}/api/jobs/1/publish`
- `POST {{baseUrl}}/api/jobs/1/duplicate`

**Important:** on the publish and duplicate requests, set the body to `{}` with
content type JSON. They take no body, but the live server rejects a body-less
POST with `411 Length Required`.

```bash
git add postman/meridian.collection.json
git commit -m "Add job posting lifecycle requests to the collection"
```

## Commit 4: Application requests

Add an `Applications` folder:

- `POST {{baseUrl}}/api/applications` with `{"jobPostingId": 1}` (candidate token)
- `GET {{baseUrl}}/api/applications/mine`
- `GET {{baseUrl}}/api/applications/job/1/ranked` (recruiter token, this is the
  AI ranking, the most important request in the collection)
- `POST {{baseUrl}}/api/applications/1/status` with `{"status": 1}`
- `GET {{baseUrl}}/api/applications/recommendations`

```bash
git add postman/meridian.collection.json
git commit -m "Add application pipeline and ranking requests to the collection"
```

## Commit 5: Access control tests

This is the part that impresses. Add a `Security` folder proving the API refuses
what it should:

| Request | Token | Expected |
|---|---|---|
| `GET /api/organizations` | none | **401** |
| `GET /api/organizations` | candidate | **403** |
| `GET /api/admin/system-health` | recruiter | **403** |
| `POST /api/auth/login` with a wrong password | none | **401** |
| `POST /api/jobs` | candidate | **403** |

Add a test script to each, for example:

```javascript
pm.test("Candidate is refused the recruiter endpoint", () =>
  pm.response.to.have.status(403));
```

```bash
git add postman/meridian.collection.json
git commit -m "Add role-based access control tests proving 401 and 403 responses"
```

## Commit 6: Run it and record the evidence

Run the whole collection with Postman's Collection Runner. Screenshot the
results, especially the green pass summary and the ranked applicants response
showing the score breakdown.

Save screenshots into `docs/testing/screenshots/` and write
`docs/testing/test-results.md` covering:

- When it was run, against which environment, how many requests, how many passed
- A table of every access control test and its result
- Anything that failed and why

```bash
git add docs/testing/
git commit -m "Add API test run results and evidence screenshots"
```

## When you are done

```bash
git push -u origin work/ashan
```

Then tell Hasitha in the group chat. **Do not merge it yourself.**

## Your minute in the presentation

You will be asked to explain your contribution while demonstrating it in the
running system. Have Postman open, run the collection live, and talk through:

- What you tested and why the access control tests matter most
- That the API refuses a candidate token on recruiter endpoints with 403, and
  refuses an absent token with 401
- The risk you managed: the deployed server rejects body-less POSTs with 411
  where the local one accepts them, so the collection sets an explicit `{}` body
