# 7. Individual Contributions

> **Note for completion before submission.** Responsibilities, features and
> testing below are filled in factually from the commit history and task
> records. Challenges encountered and lessons learned are left as `[...]`
> for each member to write in their own words, one or two sentences each,
> the word budget below does not allow more and the report needs to sound
> like four different people. Index numbers must be added. The word budget
> is tight, see `README.md` in this folder before writing.

## 7.1 Hasitha Bandara — `[index number]`

**Responsibilities undertaken.** Architecture, the backend API, the scoring
engine, the client application, and deployment.

**Features implemented.** The layered domain model, Repository and Unit of
Work, authentication and RBAC, the Prototype-based posting lifecycle, and the
four-strategy scoring engine.

**Testing contributions.** Thirty unit tests: eighteen covering ranking
determinism and the mandatory-requirement cap, twelve covering authentication
and job-ownership business logic against a real database.

**Challenges encountered.** IIS rejects a body-less POST that Kestrel accepts,
and an empty strategy collection failed silently far from its cause.

**Lessons learned.** Behaviour differing between development and the deployed
host is only found by deploying early, not by careful local testing.

## 7.2 Ashan `[surname]` — `[index number]`

**Responsibilities undertaken.** API testing, access-control verification, and
end-to-end UAT of the application, screening and reporting workflows.

**Features implemented.** The Postman collection's authentication, jobs,
applications and security folders, proving every 401 and 403 case holds.

**Testing contributions.** `[...]`

**Challenges encountered.** `[...]`

**Lessons learned.** `[...]`

## 7.3 Dinuja Ransara — `[index number]`

> **Authorship note.** `docs/testing/usability.md` was written by Dinuja, who
> ran the usability session and recorded the findings. It was committed
> directly rather than through his own branch for workflow reasons on
> submission day, so it does not appear under his name in Section 7.5.

**Responsibilities undertaken.** The three mandatory architecture diagrams,
report Chapters 1 and 2, and usability testing.

**Features implemented.** The class diagram modelling all nineteen domain
entities, and an informal usability session on the live site.

**Testing contributions.** Identified friction in the candidate sign-in flow
and the ranked table's mobile layout.

**Challenges encountered.** `[...]`

**Lessons learned.** `[...]`

## 7.4 Sewmin `[surname]` — `[index number]`

**Responsibilities undertaken.** Demonstration data, accessibility, and
responsive verification.

**Features implemented.** Six additional candidate profiles and four postings
with genuine CV text, so the TF-IDF engine scores them meaningfully.

**Testing contributions.** Verified the ranked-applicant screen and keyboard
navigation at 360, 768 and 1280 pixels.

**Challenges encountered.** `[...]`

**Lessons learned.** `[...]`

## 7.5 Repository and commit history

**Repository:** `https://github.com/Dinuja-Ransara/Recruitment-Platform`

> **Figure 22.** Commit history, showing contributions by author.

The commit history may be reproduced for this section using:

```
git log --pretty=format:"%h  %ad  %an  %s" --date=short
```

A summary by author may be produced using:

```
git shortlog -sn --all
```

## 7.6 Source code

**Source code (OneDrive):** `[paste the NSBM OneDrive link here]`

> **Required.** The submission guideline states that omission of the source code
> link results in a mark of zero for the project. The link must be shared such
> that all evaluators can open it. This must be verified by opening the link in a
> private browsing window while signed out.

## 7.7 Demonstration video

**Video (OneDrive):** `[paste the NSBM OneDrive link here]`

> **Required.** Sharing permissions must likewise be verified while signed out.
