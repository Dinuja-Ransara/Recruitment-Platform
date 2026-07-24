# 3. Architectural Design

## 3.1 Use case diagram

Figure 1 presents the use cases available to each of the four actors. Candidate
use cases concern profile management, search and application. Recruiter use cases
concern posting lifecycle and screening. Hiring manager use cases are confined to
evaluation and decision, deliberately excluding the raw applicant pool, for the
reasons given in Section 5.3. Administrator use cases concern user, role and
organisation management together with system monitoring.

> **Figure 1.** Use case diagram, showing the four actors and their permitted
> operations. Source: `docs/diagrams/use-case.puml`.

## 3.2 Class diagram

Figure 2 presents the domain model. Nineteen entities were implemented, all
inheriting identity and audit timestamps from a common `BaseEntity`.

Three relationships merit particular attention.

The association between `User` and `Role` was modelled as many-to-many through an
explicit `UserRole` join entity rather than a single role column. Within a
consultancy a hiring manager at a client is frequently also a recruiter, and a
single-role model would have required duplicate accounts. The join entity was
declared explicitly so the assignment itself carries a timestamp and can be
audited.

`JobPosting` exposes a `Clone` operation returning a deep copy of the posting and
its skill requirements. This realises the Prototype pattern and is discussed in
Section 4.5.

`JobApplication` carries a `RowVersion` concurrency token. A recruiter shortlisting
an applicant and a hiring manager rejecting the same applicant are plausible
concurrent operations, and the token causes the second write to fail rather than
silently overwrite the first.

> **Figure 2.** Class diagram of the domain model, showing entities, key
> attributes and multiplicities. Source: `docs/diagrams/class-diagram.puml`.

## 3.3 Deployment diagram

Figure 3 presents the deployed topology, which comprises three tiers.

The client tier consists of a compiled single-page application served as static
assets from a content delivery network, together with an edge function handling
requests to the `/api` path. The application tier is an ASP.NET Core 8 Web API
hosted on a Windows platform. The data tier is a SQL Server 2025 instance
reachable only from within the hosting provider's private network.

The edge function exists for a specific reason, set out in Section 5.6: the
application host cannot issue a transport layer security certificate on the plan
used, and a browser refuses to permit a page served over HTTPS to call an endpoint
served over HTTP. Terminating encryption at the edge and forwarding to the origin
from the server resolves this, because the restriction applies only to the
browser.

> **Figure 3.** Deployment diagram, showing the edge, application and data tiers
> with the protocol used between each. Source: `docs/diagrams/deployment.puml`.
