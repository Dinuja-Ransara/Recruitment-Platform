# 7. Individual Contributions

> **Note for completion before submission.** Each paragraph below must describe
> what that member actually did, and must be consistent with the commit history
> printed in Section 7.5 and with what that member demonstrates during the
> recorded presentation. Placeholders marked `[...]` are to be completed by the
> member concerned. Index numbers must be added.

## 7.1 Hasitha Bandara — `[index number]`

**Responsibilities undertaken.** Overall system architecture, the backend
application programming interface, the candidate scoring engine, the client
application, and deployment.

**Features implemented.** The layered solution and its dependency direction. The
nineteen-entity domain model, its configuration and migration. Repository and Unit
of Work, including the retriable transaction scope. Authentication, password
hashing, authorisation policies and the audit trail. The posting lifecycle with
organisation-scoped ownership and Prototype-based duplication. The application
pipeline and its permitted transitions. The scoring engine: term-frequency
implementation, five factors, four strategies and the explanation model. The four
role portals. Deployment of the interface, database and edge proxy.

**Testing contributions.** Eighteen unit tests covering determinism, ranking
order, stability under reordered input, the mandatory-requirement cap, weight
totals, divergence between strategies and the dependency injection regression.

**Challenges encountered.** The SQL Server retry strategy refuses to participate
in a transaction it did not open, so the Unit of Work was restructured to place
the retry boundary outside the transaction. The container supplied the strategy
factory with an empty collection because the strategies had not been registered
individually, a fault that surfaced far from its cause. Deployment revealed two
behaviours absent in development: Internet Information Services rejects a
body-less POST with status 411, and the hosting login cannot create a database,
so a routine that dropped the database left one nothing could restore.

**Lessons learned.** Behaviour differing between the development server and the
deployment target is not discoverable locally, so an early deployment is worth
more than a careful late one. A container that satisfies a collection dependency
with an empty collection converts a registration mistake into a distant failure,
so a component requiring collaborators should assert their presence at
construction.

## 7.2 Ashan `[surname]` — `[index number]`

**Responsibilities undertaken.** `[...]`

**Features implemented.** `[...]`

**Testing contributions.** `[...]`

**Challenges encountered.** `[...]`

**Lessons learned.** `[...]`

## 7.3 Dinuja Ransara — `[index number]`

> **Authorship note.** `docs/testing/usability.md` was written by Dinuja, who
> ran the usability session and recorded the findings. It was committed to
> the repository directly rather than through his own branch for workflow
> reasons on submission day, so it does not appear under his name in the
> commit history in Section 7.5. This note exists so the two records agree.

**Responsibilities undertaken.** `[...]`

**Features implemented.** `[...]`

**Testing contributions.** `[...]`

**Challenges encountered.** `[...]`

**Lessons learned.** `[...]`

## 7.4 Sewmin `[surname]` — `[index number]`

**Responsibilities undertaken.** `[...]`

**Features implemented.** `[...]`

**Testing contributions.** `[...]`

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
