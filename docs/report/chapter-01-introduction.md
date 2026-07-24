# 1. Introduction

## 1.1 Overview

Meridian is an AI-powered recruitment and talent management platform developed for
a multinational human resource consultancy. The platform covers the full
recruitment lifecycle for four distinct roles. Candidates maintain a professional
profile, upload a curriculum vitae and apply to advertised positions. Recruiters
create postings, screen applicants and progress them through a hiring pipeline.
Hiring managers evaluate shortlisted candidates and record decisions.
Administrators manage users, roles and client organisations, and monitor the
system.

The distinguishing characteristic of the platform is that its automated screening
explains itself. Every candidate score is returned together with the factors that
produced it, the skills that were evidenced, the skills that were absent, and the
contribution each factor made to the final figure.

## 1.2 Existing systems and problem definition

Established platforms address this domain. Workday Recruiting and SAP
SuccessFactors provide applicant tracking within larger suites, Greenhouse and
Lever concentrate on hiring workflow, and LinkedIn Recruiter supplies sourcing at
scale. None was displaced on grounds of features.

The problem addressed is narrower. Automated screening in these systems operates
as a black box: a score is produced, applicants are ordered by it, and the
reasoning is not exposed. Rejected candidates therefore receive no substantive
explanation, recruiters cannot defend a shortlist beyond citing the tool, and
organisations cannot audit the process for bias because the basis of each decision
is not recorded.

Regulation has made this concrete. New York City Local Law 144 requires bias
audits of automated employment decision tools, and the European Union Artificial
Intelligence Act classifies recruitment systems as high risk and imposes
transparency obligations (European Parliament, 2024). A tool that cannot state its
reasoning satisfies neither.

A second problem was encountered locally. The project began as an Express and
MongoDB prototype implementing one registration endpoint, whose four role portals
were placeholder files containing no implementation. It did not satisfy the
specified requirement for a C# ASP.NET Web API over a relational database and was
replaced rather than extended. That work is preserved on the
`archive/express-prototype` branch and discussed in Section 5.1.

## 1.3 Aims and objectives

The aim was to deliver a working recruitment platform in which automated
screening is transparent, reproducible and defensible. The objectives were:

1. To design a layered architecture in which the domain model carries no
   dependency on persistence, presentation or infrastructure.
2. To implement a scoring engine operating deterministically and without any
   external service, so its behaviour can be covered by automated tests.
3. To return, with every score, a breakdown of the factors producing it.
4. To enforce role-based access control at the interface boundary rather than the
   client, and to record security events in an audit trail.
5. To apply the design patterns studied where each solves a genuine problem.
6. To deliver a responsive client interface supporting the four roles.

## 1.4 Scope

The delivered scope comprises candidate registration and profile management,
curriculum vitae storage with skill extraction, posting creation and lifecycle
management, application submission and tracking, explainable ranking, pipeline
progression, organisation and department management, and system monitoring with an
audit trail.

Elements deliberately excluded are justified in Section 5.7. Electronic mail,
messaging and calendar integration was implemented through provider interfaces
with local implementations rather than paid third-party accounts. Video interview
analysis was not attempted. The deployed instance carries seeded demonstration
data only and holds no personal data of real individuals.
