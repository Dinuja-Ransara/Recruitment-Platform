# List of Figures

* **Figure 1:** Use Case Diagram detailing the system actions across the four primary actors (Candidate, Recruiter, Hiring Manager, Administrator).
* **Figure 2:** Class Diagram of the domain model illustrating entity relationships, multiplicities, and the Prototype design pattern.
* **Figure 3:** Deployment Diagram outlining the physical architecture, including the browser, Cloudflare Pages edge proxy, ASP.NET Core API, and SQL Server database tiers.
* **Figure 4:** Public landing page, presenting the ranking of a live applicant pool rather than a description of the capability.
* **Figure 5:** Authentication screen, showing the four demonstration accounts.
* **Figure 6:** Candidate overview, showing scored posting recommendations and current application activity.
* **Figure 7:** Job search with keyword, country and work mode filters applied.
* **Figure 8:** Recruiter posting list, showing published and draft states with the publish, close and duplicate actions.
* **Figure 9:** Hiring manager shortlist, restricted to candidates advanced past screening.
* **Figure 10:** Administration overview, showing record counts and the recent security event feed.
* **Figure 11:** Ranked applicant pool, showing scores, pipeline stage and mandatory requirement warnings.
* **Figure 12:** Expanded score breakdown for the leading applicant, showing each factor's measured value, the weight applied and the resulting contribution, together with evidenced and absent skills.
* **Figure 13:** The same breakdown as presented to the candidate within their own application, demonstrating that both parties are shown identical reasoning.
* **Figure 14:** Successful authentication returning a signed token together with the roles held.
* **Figure 15:** Rejected authentication, returning an identical message whether the account is absent or the password incorrect.
* **Figure 16:** A candidate token refused on a recruiter endpoint with status 403, demonstrating that authorisation is enforced by the interface and not by the client.
* **Figure 17:** Postman collection run summary.
* **Figure 18:** Response body of the ranked applicants endpoint, showing the serialised score breakdown.
* **Figure 19:** Test run output showing all thirty tests passing.
* **Figure 20:** Database schema as created by the migration, showing the nineteen tables and their relationships.
* **Figure 21:** Health endpoint reporting the deployed instance as healthy with the database reachable.
* **Figure 22:** Commit history, showing contributions by author.
