# Chapter 2: Requirements

## 2.1 Functional Requirements
The functional requirements for the Meridian platform were mapped directly from the controller endpoints implemented within the system architecture. The following core requirements were established and met:

* **FR1 (Authentication):** Secure user registration and session management were provided. Users were authenticated and authorized before accessing protected resources.
* **FR2 (Job Management):** Job postings were created, duplicated, and published by recruiters. 
* **FR3 (Application Submission):** Candidate profiles were managed, and applications were submitted against active job postings, including the parsing of structured resume data.
* **FR4 (Applicant Tracking):** The status of job applications was updated and tracked throughout the recruitment lifecycle.
* **FR5 (Candidate Ranking):** Submitted applications were evaluated, and a ranked list of candidates was generated for recruiters and hiring managers based on explicit job requirements.
* **FR6 (Organization Management):** Client organizations were registered and monitored within the system by administrative users.

## 2.2 Non-Functional Requirements
To ensure the system was secure, reliable, and accessible, several strict non-functional requirements were enforced at the code level:

* **Security & Cryptography:** Passwords were never stored in plaintext. BCrypt password hashing was implemented at a work factor of 12 to ensure robust resistance against brute-force attacks.
* **Access Control:** Strict role-based access control (RBAC) was enforced on every API endpoint. Actions were restricted based on the user's assigned role (Candidate, Recruiter, Hiring Manager, or Administrator) to prevent privilege escalation.
* **Auditability:** Comprehensive audit logging was integrated into the system. Critical state changes and administrative actions were recorded to maintain a verifiable history of system events.
* **Accessibility & Responsiveness:** The user interface was designed to be fully responsive. Usability and layout integrity were maintained on screens as narrow as 360px, ensuring access across diverse mobile devices.
* **Algorithm Predictability:** Deterministic scoring was utilized for the candidate ranking engine. The evaluation logic was designed so that identical resume inputs matched against identical job requirements would consistently produce the exact same score breakdown, completely eliminating the unpredictability of black-box AI screening.