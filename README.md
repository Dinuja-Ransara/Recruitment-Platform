<p align="center">
  <img src="docs/brand/mark-180.png" alt="Meridian" width="120" />
</p>

<h1 align="center">Meridian Talent Platform</h1>

<p align="center">
  AI-powered recruitment and talent management for a multinational HR consultancy.<br />
  <strong>SE205.3 Software Architecture</strong> &middot; Group 4 &middot; NSBM Green University
</p>

---

## What this is

A recruitment platform covering the full hiring lifecycle for four roles:
candidates, recruiters, hiring managers and administrators. Candidates build a
profile, upload a CV and apply. The platform parses the CV, extracts skills,
matches candidates to postings and ranks applicants. Recruiters screen and
schedule. Hiring managers evaluate and decide. Administrators manage users,
roles and organisations, and monitor the system.

**The scoring engine is not a wrapper around a paid AI API.** It is implemented
in C# inside this repository: a skill taxonomy with alias resolution, TF-IDF
vectorisation with cosine similarity, and structured signals for experience,
education and location fit. It runs offline, needs no API key, costs nothing,
and because it is deterministic it is covered by unit tests. Every score it
returns carries a per-factor breakdown explaining how the number was reached.

## Architecture

Layered, with dependencies pointing inward. No layer references anything
outside its own arrow.

```
src/
  Meridian.Domain           entities, enums, domain rules. Zero dependencies.
  Meridian.Application      interfaces, DTOs, result types.
  Meridian.Infrastructure   EF Core, repositories, unit of work, security, providers.
  Meridian.Ai               matching engine, skill taxonomy, parsers, ranking strategies.
  Meridian.Api              controllers, JWT, authorisation policies, Swagger.
  meridian.web              React 19 + TypeScript + Vite + Tailwind client.
tests/
  Meridian.Ai.Tests         deterministic unit tests over the scoring engine.
  Meridian.Api.Tests        integration tests through WebApplicationFactory.
docs/
  adr/                      architecture decision records.
  diagrams/                 use case, class and deployment diagrams.
```

| Concern | Choice |
|---|---|
| Backend | ASP.NET Core 8 Web API, C# |
| Database | SQL Server (LocalDB for development), EF Core 8 code-first |
| Authentication | JWT bearer, BCrypt password hashing at work factor 12 |
| Authorisation | Role-based, four named policies |
| Frontend | React 19, TypeScript, Vite, Tailwind CSS v4 |
| API documentation | Swagger / OpenAPI with bearer auth support |

## Design patterns

Each pattern earns its place against a real problem rather than being added for
the sake of the list. Full discussion is in the report, Chapter 4.

| Pattern | Where | Problem it solves |
|---|---|---|
| Factory Method | `ResumeParserFactory` | Selecting a parser for PDF, DOCX, TXT, JSON or XML without a switch leaking into services |
| Abstract Factory | `IIntegrationProviderFactory` | Swapping email, SMS, calendar and storage as one consistent family |
| Singleton | `SkillTaxonomyRegistry` | An expensive, immutable taxonomy loaded once and shared safely |
| Builder | `AnalyticsQueryBuilder` | Analytics queries with eight optional filters |
| Prototype | `JobPosting.Clone()` | Reposting a role across Colombo, Singapore and London |
| Repository | `IRepository<T>` | Isolating EF Core so services can be tested against fakes |
| Unit of Work | `IUnitOfWork` | Committing application, event, notification and audit writes as one transaction |
| Dependency Injection | `Program.cs`, `DependencyInjection.cs` | Every dependency is an interface |
| Strategy | `IRankingStrategy` | Recruiter chooses the ranking algorithm per posting |
| Observer | `IDomainEventDispatcher` | Status changes fan out without the service knowing its subscribers |

## Running it

Requires the .NET 8 SDK, Node 20 or later, and SQL Server LocalDB.

```bash
# Backend. Applies migrations and seeds automatically on first run.
dotnet run --project src/Meridian.Api --launch-profile http
# API on http://localhost:5138, Swagger at http://localhost:5138/swagger

# Frontend, in a second terminal.
cd src/meridian.web
npm install
npm run dev
# Client on http://localhost:5173
```

### Demonstration accounts

All share the password `Meridian#2026`.

| Role | Email |
|---|---|
| Administrator | admin@meridian.example.com |
| Recruiter | recruiter@meridian.example.com |
| Hiring Manager | manager@meridian.example.com |
| Candidate | candidate@meridian.example.com |

The seeder is idempotent, so restarting never duplicates data. It creates three
client organisations across Colombo, Singapore and London, seven departments and
a skill taxonomy of 25 skills with 32 aliases.

## Testing

```bash
dotnet test                      # unit and integration tests
newman run postman/meridian.json # API collection, requires the API running
```

## Project history

This repository began as an Express and MongoDB prototype. The coursework brief
mandates a C# ASP.NET Web API over a relational database, so that prototype was
replaced by the layered architecture described above. The original work is
preserved on the `archive/express-prototype` branch and is discussed in the
report under existing systems and problem definition.
