# Dinuja: architectural diagrams and report Chapters 1 and 2

**Your branch:** `work/dinuja`
**Your files:** `docs/diagrams/` and `docs/report/`. Nothing else, so nothing you
do can conflict with anyone.

**Why this matters:** the brief marks three diagrams as **mandatory**, and the
class diagram is called out twice. Without them marks are lost outright, and
right now none of them exist.

Read [CONTRIBUTING.md](../../CONTRIBUTING.md) first for the git workflow.

## Tooling

Write the diagrams as PlantUML text, not by dragging boxes. Text is versionable,
reviewable and editable later.

- Render at <https://www.plantuml.com/plantuml> (paste, download PNG), or
- Use the **PlantUML** extension in VS Code and press `Alt+D` to preview.

Commit both the `.puml` source and the exported `.png`.

## Where to get the facts

Do not invent the model. It is all in the repository:

| What | Where |
|---|---|
| Entities and their fields | `src/Meridian.Domain/Entities/` |
| Relationships, keys, indexes | `src/Meridian.Infrastructure/Persistence/Configurations/` |
| Endpoints and roles | `src/Meridian.Api/Controllers/` |
| Project layout | `README.md` |

---

## Commit 1: Use case diagram

Four actors: Candidate, Recruiter, Hiring Manager, Administrator.

Cover at minimum: register, sign in, search jobs, apply, track application,
withdraw, create posting, publish posting, duplicate posting, view ranked
applicants, change application status, review shortlist, record decision, manage
organisations, monitor the system.

Show which actor can do what. A stub is already in
`docs/diagrams/use-case.puml` to start from.

```bash
git add docs/diagrams/use-case.puml docs/diagrams/use-case.png
git commit -m "Add the use case diagram covering all four actors"
```

## Commit 2: Class diagram (this one is mandatory and marked twice)

Model the domain entities and their relationships. Read the files in
`src/Meridian.Domain/Entities/` and include:

`User`, `Role`, `UserRole`, `Organization`, `Department`, `CandidateProfile`,
`Resume`, `Skill`, `SkillAlias`, `ResumeSkill`, `JobPosting`,
`JobRequiredSkill`, `JobApplication`, `ApplicationEvent`, `Interview`,
`InterviewFeedback`, `Evaluation`, `AuditLog`, `Notification`.

Get the multiplicities right, they are stated in the configuration files:

- `User` to `Role` is many-to-many through `UserRole`
- `User` to `CandidateProfile` is one-to-one
- `CandidateProfile` to `Resume` is one-to-many
- `JobPosting` to `JobApplication` is one-to-many
- `JobApplication` to `ApplicationEvent` is one-to-many

Include the `Clone()` method on `JobPosting`. It is the Prototype pattern and
Chapter 4 refers to it.

```bash
git add docs/diagrams/class-diagram.puml docs/diagrams/class-diagram.png
git commit -m "Add the class diagram for the domain model"
```

## Commit 3: Deployment diagram

Show where each piece actually runs. This is not hypothetical, it is deployed:

```
Browser
  └─ HTTPS ─> Cloudflare Pages   (React client, and a Pages Function at /api/*)
                  └─ HTTP ─> MonsterASP.NET   (ASP.NET Core 8 Web API)
                                └─ TCP 1433 ─> SQL Server 2025 (db60830)
```

Label the protocols. Note in the diagram that the Pages Function exists because
the API host cannot issue a TLS certificate, so the edge terminates HTTPS.

```bash
git add docs/diagrams/deployment.puml docs/diagrams/deployment.png
git commit -m "Add the deployment diagram showing the edge, API and database tiers"
```

## Commit 4: Chapter 1

Write `docs/report/chapter-01-introduction.md`, roughly **600 words**, covering:

1. **Introduction.** What the platform is and who it serves.
2. **Existing systems and problem definition.** Compare against real tools:
   Workday, Greenhouse, LinkedIn Recruiter. The genuine problem is that automated
   screening is a black box, candidates are rejected with no reason given and
   recruiters cannot defend a shortlist. You also have an honest local example:
   this project began as an Express and MongoDB prototype and was replaced,
   preserved on the `archive/express-prototype` branch.
3. **Aims and objectives.**
4. **Scope**, including what is deliberately out of scope.

**Style rules from the guideline, these are marked:** third person, past tense,
passive voice. No "I", "we", or "you". Write "the system was designed", never "we
designed the system".

```bash
git add docs/report/chapter-01-introduction.md
git commit -m "Draft Chapter 1: introduction, problem definition, aims and scope"
```

## Commit 5: Chapter 2

Write `docs/report/chapter-02-requirements.md`, roughly **550 words**:

- **Functional requirements**, numbered FR1, FR2 and so on. Derive them from the
  controllers in `src/Meridian.Api/Controllers/`, do not invent them.
- **Non-functional requirements**, numbered NFR1 onwards. Real ones you can point
  at in the code: passwords hashed with BCrypt at work factor 12, role-based
  access control on every endpoint, audit logging of security events, responsive
  down to 360px, scoring that is deterministic and reproducible.
- **Features of the application**, grouped by the four portals.

```bash
git add docs/report/chapter-02-requirements.md
git commit -m "Draft Chapter 2: functional and non-functional requirements"
```

## Commit 6: Figure list

Create `docs/report/figures.md` listing every diagram with its number and
caption, in the form the final report needs (Figure 1, Figure 2, Figure 3). The
guideline requires numbered figures.

```bash
git add docs/report/figures.md
git commit -m "Add the numbered figure list for the report"
```

## When you are done

```bash
git push -u origin work/dinuja
```

Then tell Hasitha. **Do not merge it yourself.**

## Your minute in the presentation

Have the diagrams on screen and walk through:

- The class diagram, and one relationship that was not obvious, for example why
  `User` and `Role` are many-to-many rather than a single role column: a hiring
  manager at a client is frequently also a recruiter
- The deployment diagram, and why the edge tier exists at all
- The risk you managed: the first architecture was MERN, which did not meet the
  brief's requirement for ASP.NET and a relational database, and it had to be
  replaced
