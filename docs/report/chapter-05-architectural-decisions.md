# 5. Architectural Decisions

## 5.1 System architecture selection

A layered architecture was selected, comprising domain, application,
infrastructure and presentation layers with dependencies directed inward. The
domain project references nothing; the web interface references infrastructure
only at its composition root.

The consequence that mattered most concerns the scoring engine. It accepts plain
records rather than persistence entities and therefore carries no reference to
Entity Framework, so it can be tested with literal values instead of a database
and eighteen tests exercise it in under a second. Had it consumed entities
directly, every test would have required a database context and the determinism
claim in Section 5.4 would have been impractical to assert.

The alternative considered was the MERN stack of the initial prototype, rejected
because the specification required a C# ASP.NET Web API over a relational
database, and independently because referential integrity between applications,
postings and organisations is a correctness requirement rather than a
convenience.

## 5.2 Database design decisions

Nineteen tables were implemented code-first, with configuration in
`IEntityTypeConfiguration` classes discovered by assembly scanning so the context
remains stable as the model grows.

Constraints were placed in the database rather than in application checks alone. A
unique index on electronic mail address prevents the race in which two
simultaneous registrations both pass an existence check. A composite unique index
across posting and candidate prevents duplicate applications. Skill names are
unique, which with a case-insensitive collation ensures two spellings of one skill
cannot score differently.

Deletion behaviour was set explicitly per relationship, because SQL Server rejects
a schema containing multiple cascade paths to one table; cascade was retained only
where a child has no meaning without its parent.

The score breakdown is stored as JSON in one column rather than normalised. It is
written once, read whole, never queried by its parts, and its shape is expected to
evolve. Deserialisation failure is caught, since a breakdown written by an earlier
shape must not prevent a response; the numeric score is stored separately.

## 5.3 Security mechanisms

Passwords are hashed using BCrypt at work factor twelve, chosen over a
general-purpose hash because it is deliberately slow and salts automatically,
defeating both precomputed tables and the parallel brute force that makes fast
hashes unsuitable for passwords.

Authentication issues a signed JSON Web Token carrying identity and every role
held, so authorisation is evaluated without a database round trip. It is signed
rather than encrypted, so no confidential value is placed within it. Clock skew
tolerance was set to zero, the customary five-minute allowance being misleading in
a demonstration.

Authorisation is expressed as named policies rather than repeated role strings,
and enforcement is layered: the interface establishes that a caller holds the
recruiter role, then the service verifies the posting belongs to the caller's own
organisation, so one client's recruiter cannot modify another client's vacancy.

Two disclosure decisions were deliberate. Authentication failure returns an
identical message whether the account is absent or the password wrong, so the
endpoint cannot enumerate addresses. An unpublished posting, or another user's
application, returns "not found" rather than "forbidden", so identifiers cannot be
probed for existence.

Authentication attempts, registrations and entity modifications are written to an
append-only audit trail retaining the acting user and originating address.

## 5.4 Artificial intelligence integration approach

The scoring engine was implemented within the solution rather than delegated to an
external service, which gave it three properties otherwise unobtainable. It is
deterministic, so identical inputs produce identical scores and the behaviour can
be asserted by test, where a hosted language model cannot. It needs no network
access or credential, so the instance cannot be disabled by an expired key. And it
is explicable.

Term frequency and inverse document frequency were implemented directly, with a
recruitment-specific stop word list so terms such as "experience" and
"responsible", present in nearly every document, cannot dominate a comparison.
Term frequency is normalised by document length, so a lengthy curriculum vitae
cannot outscore a concise one by repetition, and cosine similarity compares
subject matter while disregarding length.

Five factors are measured: weighted skill coverage with partial credit for shallow
experience, resume relevance, experience fit with a taper penalising
overqualification, education fit and location fit. Each returns a value between
zero and one, multiplied by the weight the strategy assigns.

One rule was non-negotiable. A candidate lacking a skill the posting marks
mandatory has the score **capped** rather than reduced, because strength elsewhere
must not offset a hard requirement failure. The cap is reported in the breakdown
rather than applied silently, and a test asserts that the reported contributions
reconstruct the headline score, so the explanation is evidence of the calculation
rather than a description accompanying it.

## 5.5 Application programming interface design principles

Resource-oriented routes were adopted, with state transitions expressed as
subordinate actions such as `POST /api/jobs/{id}/publish`, because publication is
a transition subject to rules rather than a field update. Permitted transitions
are declared once as a table, so an application cannot move directly from
submitted to hired. Expected failures are returned as values rather than raised as
exceptions, so conditions such as "already applied" travel through ordinary
control flow.

One interaction between framework features required correction. The context
enables retry on transient failure, and the SQL Server retry strategy refuses to
participate in a transaction opened outside its control, since on a retry it could
not roll back the previous attempt. Rather than abandoning retry, the Unit of Work
requests the provider's execution strategy and opens the transaction within it, so
the retry boundary encloses the transaction rather than the reverse.

## 5.6 Scalability considerations

Read paths use no-tracking queries with bounded paging, so a request cannot
materialise an entire table, and indexes were placed on the columns actually
filtered.

Ranking recomputes scores across the pool on each request rather than returning
stored values, because inverse document frequency is measured relative to a
corpus and a score computed against one applicant carries less information than
one computed against the full pool. The values are then persisted so the candidate
and recruiter views agree.

The stateless interface, with authorisation carried in the token, permits
horizontal scaling without shared session storage.

## 5.7 Assumptions and constraints

The hosting plan cannot issue a transport layer security certificate. Since a
browser blocks an HTTPS page from calling an HTTP endpoint, an edge function was
introduced on the client's own origin, terminating encryption at the content
delivery network and forwarding from the server, where the restriction does not
apply. Siting the proxy on the client origin further means no cross-origin request
occurs. The edge-to-origin hop is unencrypted, recorded here as a constraint of
the plan rather than a production posture; the instance carries seeded
demonstration data only.

Electronic mail, messaging, calendar and storage integrations were implemented
behind provider interfaces with local implementations rather than paid accounts.

Two platform differences appeared only after deployment. Internet Information
Services rejects a body-less POST carrying no content length with status 411,
where the development server accepts it. And the hosting login, though holding full rights within its
own database, cannot create a database on the server; consequently the routine
rebuilding demonstration data deletes rows in dependency order and never drops the
database, since a dropped database could not be recreated by the application.
