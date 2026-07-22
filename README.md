# SA Project Plan: TalentX Recruitment Platform

**Author:** Dinuja Ransara  
**Institution:** NSBM Green University, Faculty of Computing  
**Date:** July 2026  

---

## 1. System Architecture (SA) Overview
The platform utilizes a modern Client-Server architecture based on the **MERN** stack (MongoDB, Express.js, React.js, Node.js). The system is completely decoupled, meaning the frontend and backend operate independently and communicate via a RESTful API.

* **Presentation Layer (Frontend):** Built with React.js using Vite for fast bundling. It is styled with Tailwind CSS to ensure a fully responsive, component-based user interface.
* **Application Layer (Backend):** Built with Node.js and Express.js to handle all business logic, user authentication, and API routing.
* **Data Layer (Database):** MongoDB (NoSQL) accessed via the Mongoose Object Data Modeling (ODM) library for flexible, document-based data storage.

---

## 2. Software Engineering (SE) Methodology
The project follows an **Iterative/Agile Methodology**. This allows for the continuous testing and refinement of individual system modules (e.g., building and validating the authentication module before moving to the dashboard interfaces).

### 2.1 Core Requirements Analysis
**Functional Requirements (FRs):** 
1. The system must allow users to register distinct profiles as either a "Candidate" or a "Recruiter."
2. Recruiters must be able to post job listings and view applicant profiles via a secure dashboard.
3. Candidates must be able to view active job listings and submit applications.

**Non-Functional Requirements (NFRs):**
1. **Security:** Passwords must be hashed using bcrypt prior to database insertion.
2. **Performance:** API responses should resolve in under 500ms to ensure a seamless user experience.
3. **Usability:** The UI must be fully responsive across mobile, tablet, and desktop interfaces.

---

## 3. Application Sitemap (Frontend Page Structure)
Based on the routing and navigation hierarchy, the frontend consists of the following primary views:

| Page / Route | React Component | Purpose |
| :--- | :--- | :--- |
| **`/`** | `LandingPage.jsx` | The main entry point featuring the value proposition, "Home," "Jobs," "News," and "About" sections. |
| **`/login`** | `Login.jsx` | Dual-purpose authentication portal allowing users to toggle between Candidate and Recruiter sign-in. |
| **`/register/candidate`** | `CandidateRegister.jsx` | Registration form capturing candidate details (Name, Email, Skills). |
| **`/register/recruiter`** | `RecruiterRegister.jsx` | Business registration form capturing company legal name, industry, and contact details. |
| **`/candidate/dashboard`** | `CandidatePortal.jsx` | Private view for job seekers to track applications and update resumes. |
| **`/recruiter/dashboard`** | `RecruiterPortal.jsx` | Private view for hiring managers to post jobs and review candidates. |
| **`/admin`** | `AdminDashboard.jsx` | System administrator view for moderating users and platform content. |

---

## 4. Backend Connection & Data Flow
The integration between the React frontend and the Node.js backend follows a strict Request-Response lifecycle. 

**Step-by-Step Data Flow (Registration Module Example):**
1. **Client-Side Capture:** The React component (`CandidateRegister.jsx`) captures user input in a state object.
2. **API Invocation:** An asynchronous `fetch` request is triggered upon form submission, sending the payload as a JSON string to the backend endpoint (`http://localhost:5000/api/register/candidate`).
3. **Backend Processing:** The Express.js router intercepts the `POST` request. Controller logic validates the incoming data format.
4. **Database Transaction:** Mongoose maps the data to the defined `Candidate` schema and saves a new document into the MongoDB database.
5. **Response:** The backend returns an HTTP status code (e.g., `201 Created`). The React frontend listens for this response and triggers the appropriate UI alert.

---

## 5. API Endpoint Architecture
To fulfill the platform requirements, the backend utilizes the following RESTful routes:

* `POST /api/register/candidate` - Creates a new candidate user entity.
* `POST /api/register/recruiter` - Creates a new recruiter/business entity.
* `POST /api/auth/login` - Authenticates user credentials and returns a secure token for session management.
* `GET /api/jobs` - Fetches active job listings for the public and candidate views.
