# Chapter 1: Introduction

## 1.1 Introduction
The recruitment landscape has increasingly relied on digital platforms to manage the influx of applications for modern job postings. As organizations scaled, the need for automated Applicant Tracking Systems (ATS) became critical to parse, evaluate, and rank candidates efficiently. The Meridian Recruitment Platform was conceptualized and developed as a response to the growing demand for transparency and accuracy within these digital recruitment workflows. The system was designed to serve four primary actors: Candidates, Recruiters, Hiring Managers, and Administrators, providing a unified ecosystem for the entire job application lifecycle.

## 1.2 Existing Systems and Problem Definition
While the market was dominated by established platforms such as Workday, Greenhouse, and LinkedIn Recruiter, significant systemic flaws were identified during the analysis phase. These existing solutions successfully managed data at scale, but their automated screening mechanisms frequently functioned as a complete black box. 

When candidates were processed through these legacy systems, the scoring algorithms were hidden. Consequently, candidates were routinely rejected without being provided any specific reason for their rejection, leading to a frustrating user experience. Furthermore, because the evaluation metrics were obfuscated, recruiters were unable to adequately defend a generated shortlist when questioned by hiring managers. The lack of deterministic, explainable scoring meant that placement decisions could not be audited or justified using concrete data. 

## 1.3 Aims and Objectives
To address these critical shortcomings, the Meridian platform was designed with transparency as its foundational objective. The primary aim was to replace opaque screening algorithms with a deterministic, explainable scoring engine. 

The specific objectives of the project were defined as follows:
* To implement a transparent ranking system where every applicant score was accompanied by a detailed, accessible breakdown of how that score was calculated.
* To ensure that candidates who were rejected were provided with automated, clear, and objective reasons based strictly on missing mandatory skills or qualifications.
* To equip recruiters with the necessary data to defend and justify shortlists to hiring managers confidently.
* To construct a robust, secure architecture utilizing ASP.NET Core 8 Web API and a React client, protected by strict role-based access control.

## 1.4 Scope
The scope of the Meridian project was strictly limited to the core recruitment lifecycle. It encompassed the creation and publishing of job postings, applicant registration and profile management, automated application scoring, and the final recording of hiring decisions. 

The system was designed to handle the parsing of structured resume data and the matching of that data against explicitly defined job requirements. However, the scope deliberately excluded post-hiring onboarding processes, payroll integration, and internal employee performance tracking. The focus was maintained exclusively on creating a transparent, defensible bridge between the initial job posting and the final candidate selection.