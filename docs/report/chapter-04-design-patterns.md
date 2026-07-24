# 4. Design Patterns

Patterns were applied where each addressed a problem present in the system.
Table 1 traces every pattern used to the module lecture that introduced it and to
the class realising it.

**Table 1.** Design pattern traceability.

| Pattern | Lecture | Realised by | Problem solved |
|---|---|---|---|
| Repository | Coursework requirement | `Repository<T>` | Isolating the application layer from Entity Framework |
| Unit of Work | Coursework requirement | `UnitOfWork` | Committing several related writes as one transaction |
| Dependency Injection | Coursework requirement | `DependencyInjection.AddInfrastructure` | Depending on interfaces rather than implementations |
| Factory Method | L3 | `RankingStrategyFactory` | Resolving a scoring strategy without a switch at each call site |
| Prototype | L7 | `JobPosting.Clone` | Reposting a role across offices without retyping it |
| Strategy | Coursework requirement | `IRankingStrategy` and four implementations | Scoring different classes of vacancy differently |
| Template Method | Derived | `WeightedStrategyBase` | Fixing the scoring algorithm while varying its weights |
| Singleton | L5 | Container-managed lifetimes | Sharing immutable, expensive collaborators safely |

## 4.1 Repository

`IRepository<T>` exposes retrieval, addition, update and removal over any entity
deriving from `BaseEntity`, wrapping an Entity Framework `DbSet`.

No repository method calls `SaveChanges`. Persisting belongs to the Unit of Work,
and that separation is what permits several repositories to participate in one
transaction. A deliberately narrow `Query` method returns `IQueryable` for read
paths needing projection, while all writes pass through the typed methods.

## 4.2 Unit of Work

Submitting an application writes to `JobApplications`, `ApplicationEvents`,
`Notifications` and `AuditLogs`. These constitute one logical event and were
required to commit together or not at all.

`IUnitOfWork` therefore owns the transaction boundary and hands out repositories
sharing a single context, cached per entity type. It exposes
`ExecuteInTransactionAsync` rather than only the conventional begin, commit and
rollback triple, for the reason given in Section 5.5: the database retries
transient failures, and a retry policy cannot resume a transaction it did not
open.

## 4.3 Dependency Injection

Every dependency is expressed as an interface and supplied by the ASP.NET Core
container. Lifetimes were chosen deliberately: the Unit of Work is scoped, giving
one context per request, while the password hasher, token service, strategies and
matching engine are singletons holding no mutable state.

Because the engine depends on `IRankingStrategyFactory` rather than constructing
strategies itself, it could be exercised against test doubles, which is how the
regression in Section 4.4 was captured by test.

## 4.4 Factory Method

`IRankingStrategyFactory` resolves the strategy a posting requested, in place of a
switch statement wherever ranking occurs that would need amendment whenever a
strategy is added. An unrecognised stored value returns the balanced default
rather than raising, so an unexpected enumeration value cannot take ranking
offline.

A defect found during integration testing illustrates a real hazard of the
pattern. The factory declares a constructor accepting
`IEnumerable<IRankingStrategy>` for injection. The container selected it and
supplied an **empty** collection, because the factory had been registered while
the strategies had not. Containers satisfy `IEnumerable<T>` with an empty sequence
rather than failing, so the fault surfaced much later, inside scoring, as a
missing dictionary key. The strategies were then registered individually, and the
factory now rejects an empty collection at construction so the failure names its
own cause. Two regression tests cover it.

## 4.5 Prototype

A multinational consultancy commonly advertises the same role in several offices.
`JobPosting.Clone` returns a deep copy of the posting and its skill requirements,
deliberately resetting the copy to draft and discarding the publication date,
identity and applications of the original.

The pattern was preferred to a data transfer approach because deep copy semantics
belong with the entity that understands its own invariants. The recruiter
interface exposes this as a duplicate action.

## 4.6 Strategy

A single scoring formula is inappropriate across all vacancies. A backend
position is properly decided on evidenced skills, a practice lead on depth of
experience, and encoding one formula would have obliged the recruiter to argue
with the tool.

Four strategies were implemented and selected per posting: skill weighted, resume
similarity, hybrid and experience first. They differ only in the weights applied
to the five measured factors, never in how a factor is measured, so two strategies
may disagree about priorities while agreeing about facts. That they genuinely
diverge is asserted by test: a candidate weak in skills but strong in experience
scores higher under the experience-first strategy than the skill-weighted one.

## 4.7 Template Method

`WeightedStrategyBase` fixes the algorithm: it measures the five factors, applies
the subclass weights, applies the mandatory-requirement cap described in Section
5.4 and assembles the explanation. A concrete strategy supplies only its weights
and identity, which makes each factor testable once rather than once per strategy
and reduces a new strategy to declaring five numbers.

## 4.8 Singleton

The module presented Singleton in its classical form, a class enforcing its own
single instance through a private constructor and static accessor. That form was
considered and not adopted.

The objects requiring single-instance semantics here are the matching engine and
the four strategies, all immutable and stateless. Registering them with singleton
lifetime achieves the same sharing while leaving the classes ordinary,
constructible and testable. The classical form makes substitution in a test
difficult, since the instance is reached through a static member rather than
supplied as a dependency. The pattern was therefore applied in intent and
delegated in mechanism, the prevailing practice in dependency-injected
applications (Microsoft, 2024).

## 4.9 Patterns considered and not used

Abstract Factory and Builder were evaluated. The former would have suited the
external provider families of Section 5.7 had those integrations targeted live
services, the latter a filtered analytics query had reporting been in scope.
Neither was introduced, because a pattern applied where no problem exists adds
indirection without benefit.
