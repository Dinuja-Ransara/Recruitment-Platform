using Meridian.Domain.Enums;

namespace Meridian.Infrastructure.Persistence;

/// <summary>
/// The demonstration dataset, kept apart from the seeding logic so the data can
/// be read and extended without touching the code that writes it.
///
/// Resume and description text is written as real prose on purpose. The matching
/// engine reads both with TF-IDF, so placeholder filler would produce meaningless
/// similarity scores and a ranking that proves nothing.
/// </summary>
internal static class DemoData
{
    internal sealed record CandidateSpec(
        string Email,
        string FullName,
        string Headline,
        string City,
        string Country,
        decimal Years,
        EducationLevel Education,
        bool OpenToRemote,
        string ResumeText,
        Dictionary<string, decimal> Skills);

    internal sealed record JobSpec(
        string OrganizationName,
        string DepartmentName,
        string Title,
        string Description,
        string Responsibilities,
        string City,
        string Country,
        WorkMode WorkMode,
        EmploymentType EmploymentType,
        SeniorityLevel Seniority,
        decimal MinYears,
        EducationLevel Education,
        decimal? SalaryMin,
        decimal? SalaryMax,
        string Currency,
        RankingStrategyType Strategy,
        (string Skill, bool Mandatory, int Weight, decimal MinYears)[] Skills);

    // -----------------------------------------------------------------------
    // Candidates
    // -----------------------------------------------------------------------

    internal static List<CandidateSpec> Candidates() =>
    [
        new("dilani.rathnayake@example.com", "Dilani Rathnayake",
            "Senior .NET engineer, payments and settlement", "Colombo", "Sri Lanka", 7,
            EducationLevel.Masters, true,
            "Seven years building transaction processing systems in C# on ASP.NET Core. Led the settlement "
            + "and clearing service for a regional card scheme, handling reconciliation across three markets. "
            + "Designed the relational model on SQL Server with careful indexing of the settlement tables. "
            + "Introduced Docker based deployment and a CI/CD pipeline, cutting release time from days to hours. "
            + "Mentored four engineers and ran the architecture review forum.",
            new() { ["C#"] = 7, ["ASP.NET Core"] = 6, ["SQL Server"] = 6, ["Docker"] = 4, ["Entity Framework Core"] = 5, ["CI/CD"] = 4, ["REST API Design"] = 6, ["Team Leadership"] = 3 }),

        new("kasun.silva@example.com", "Kasun Silva",
            "Backend engineer, C# and distributed services", "Kandy", "Sri Lanka", 5,
            EducationLevel.Bachelors, true,
            "Five years of backend development in C# and ASP.NET Core for logistics and freight tracking. "
            + "Built REST APIs consumed by mobile clients across two regions, with SQL Server as the primary store. "
            + "Familiar with containerised workloads although most deployment was handled by a platform team. "
            + "Comfortable with unit testing and code review.",
            new() { ["C#"] = 5, ["ASP.NET Core"] = 4, ["SQL Server"] = 4, ["REST API Design"] = 4, ["Unit Testing"] = 3 }),

        new("nadeesha.perera@example.com", "Nadeesha Perera",
            "Full stack engineer, React and Node", "Colombo", "Sri Lanka", 4,
            EducationLevel.Bachelors, true,
            "Four years building customer facing web applications with React and TypeScript on a Node.js backend. "
            + "Delivered a booking platform serving thirty thousand monthly users, with PostgreSQL behind it. "
            + "Some exposure to C# through an internal reporting tool. Strong on interface work, accessibility "
            + "and design systems.",
            new() { ["React"] = 4, ["TypeScript"] = 4, ["JavaScript"] = 5, ["Node.js"] = 4, ["PostgreSQL"] = 3, ["C#"] = 1 }),

        new("arjun.mehta@example.com", "Arjun Mehta",
            "Principal engineer, platform and reliability", "Singapore", "Singapore", 12,
            EducationLevel.Masters, false,
            "Twelve years across platform engineering and site reliability. Ran the payments platform for a "
            + "regional bank, owning availability targets and incident response. Deep experience with "
            + "microservices, Kubernetes and observability. Wrote services primarily in Java, with C# on two "
            + "earlier products. Led an engineering group of eighteen across three offices.",
            new() { ["Java"] = 12, ["Kubernetes"] = 6, ["Microservices"] = 8, ["Docker"] = 7, ["C#"] = 3, ["Team Leadership"] = 7, ["AWS"] = 5 }),

        new("fatima.hassan@example.com", "Fatima Hassan",
            "Backend engineer, C# and cloud", "London", "United Kingdom", 6,
            EducationLevel.Bachelors, true,
            "Six years of C# and ASP.NET Core work across insurance and healthcare. Built claims processing "
            + "APIs handling high volumes, deployed on Azure with Docker. Worked exclusively with PostgreSQL "
            + "and MongoDB for persistence, never with SQL Server. Strong on testing discipline and CI/CD.",
            new() { ["C#"] = 6, ["ASP.NET Core"] = 5, ["Docker"] = 4, ["Azure"] = 4, ["PostgreSQL"] = 5, ["MongoDB"] = 3, ["Unit Testing"] = 5, ["CI/CD"] = 4 }),

        new("thilina.bandara@example.com", "Thilina Bandara",
            "Graduate software engineer", "Galle", "Sri Lanka", 1,
            EducationLevel.Bachelors, true,
            "Recent computer science graduate with one year in a junior role. Built internal tools in C# and "
            + "learned ASP.NET Core on the job. University final year project was a library management system "
            + "with a SQL Server backend. Keen to work on larger systems.",
            new() { ["C#"] = 1, ["ASP.NET Core"] = 1, ["SQL Server"] = 1 }),

        new("marie.dubois@example.com", "Marie Dubois",
            "Hospitality operations supervisor", "Paris", "France", 8,
            EducationLevel.Diploma, false,
            "Eight years in hotel operations, supervising front of house teams of up to twenty. Responsible for "
            + "rota planning, guest relations, supplier negotiation and stock control. Introduced a new booking "
            + "process that reduced check in time significantly.",
            new() { ["Stakeholder Management"] = 6, ["Team Leadership"] = 8 }),

        new("priyanka.nair@example.com", "Priyanka Nair",
            "Cloud architect, Azure and .NET", "Singapore", "Singapore", 9,
            EducationLevel.Masters, true,
            "Nine years designing cloud native systems on Azure for financial services. Migrated a monolithic "
            + "policy administration system to microservices on ASP.NET Core, cutting deployment risk "
            + "substantially. Deep on Docker, Kubernetes and infrastructure as code. Regularly presents "
            + "architecture proposals to executive stakeholders.",
            new() { ["C#"] = 9, ["ASP.NET Core"] = 8, ["Azure"] = 8, ["Docker"] = 6, ["Kubernetes"] = 5, ["Microservices"] = 7, ["SQL Server"] = 6, ["Stakeholder Management"] = 5 }),

        new("daniel.oconnor@example.com", "Daniel O'Connor",
            "Frontend engineer, React and design systems", "London", "United Kingdom", 6,
            EducationLevel.Bachelors, true,
            "Six years of frontend engineering in React and TypeScript. Built and maintained a design system "
            + "used by nine product teams. Strong focus on accessibility, having taken two products through "
            + "WCAG AA audits. Comfortable consuming REST APIs and shaping the contracts with backend teams.",
            new() { ["React"] = 6, ["TypeScript"] = 6, ["JavaScript"] = 8, ["REST API Design"] = 4, ["Unit Testing"] = 4 }),

        new("chamodi.gunasekara@example.com", "Chamodi Gunasekara",
            "Data engineer, Python and SQL", "Colombo", "Sri Lanka", 5,
            EducationLevel.Bachelors, true,
            "Five years building data pipelines in Python against SQL Server and PostgreSQL warehouses. "
            + "Automated regulatory reporting for a telecommunications operator, replacing a manual monthly "
            + "process. Comfortable with orchestration, testing and CI/CD for data workloads.",
            new() { ["Python"] = 5, ["SQL"] = 5, ["SQL Server"] = 4, ["PostgreSQL"] = 4, ["CI/CD"] = 3, ["Unit Testing"] = 3 }),

        new("wei.lin.tan@example.com", "Wei Lin Tan",
            "Engineering manager, supply chain systems", "Singapore", "Singapore", 11,
            EducationLevel.Masters, false,
            "Eleven years in supply chain and logistics technology, the last four leading teams. Owned the "
            + "warehouse management platform for a regional operator, coordinating between engineering, "
            + "operations and external vendors. Hands on background in Java and C#. Focused on delivery "
            + "predictability and hiring.",
            new() { ["Java"] = 9, ["C#"] = 5, ["Team Leadership"] = 6, ["Stakeholder Management"] = 7, ["Agile"] = 6, ["Microservices"] = 4 }),

        new("ishara.wijeratne@example.com", "Ishara Wijeratne",
            "QA engineer, automation", "Colombo", "Sri Lanka", 4,
            EducationLevel.Bachelors, true,
            "Four years in quality assurance with a focus on automation. Built the regression suite for a "
            + "payments product, covering API and end to end flows. Works closely with developers on "
            + "testability and runs the release verification process. Writes automation in C# and JavaScript.",
            new() { ["Unit Testing"] = 4, ["C#"] = 3, ["JavaScript"] = 3, ["REST API Design"] = 3, ["CI/CD"] = 3, ["Agile"] = 4 }),

        new("omar.farouk@example.com", "Omar Farouk",
            "Solutions architect, banking", "London", "United Kingdom", 14,
            EducationLevel.Masters, false,
            "Fourteen years in banking technology, currently a solutions architect. Designed the integration "
            + "layer connecting core banking to digital channels, handling settlement and reconciliation. "
            + "Strong C# and SQL Server background, with recent work on event driven microservices. Chairs the "
            + "architecture review board and presents to risk and compliance regularly.",
            new() { ["C#"] = 12, ["ASP.NET Core"] = 7, ["SQL Server"] = 11, ["Microservices"] = 6, ["REST API Design"] = 9, ["Stakeholder Management"] = 8, ["Team Leadership"] = 6 }),

        new("sanduni.abeywickrama@example.com", "Sanduni Abeywickrama",
            "Junior full stack developer", "Negombo", "Sri Lanka", 2,
            EducationLevel.Bachelors, true,
            "Two years building internal web applications with React on an ASP.NET Core backend. Worked on a "
            + "human resources portal handling leave requests and appraisals, with SQL Server behind it. "
            + "Learning containerisation and automated testing.",
            new() { ["React"] = 2, ["C#"] = 2, ["ASP.NET Core"] = 2, ["SQL Server"] = 2, ["JavaScript"] = 3 }),

        new("james.whitfield@example.com", "James Whitfield",
            "DevOps engineer, containers and pipelines", "Manchester", "United Kingdom", 7,
            EducationLevel.Bachelors, true,
            "Seven years in DevOps and platform work. Owns the container platform for a retail group, running "
            + "Kubernetes across two regions. Built the deployment pipelines that every product team uses. "
            + "Scripts primarily in Python, with enough C# to debug the applications being deployed.",
            new() { ["Kubernetes"] = 6, ["Docker"] = 7, ["CI/CD"] = 7, ["Python"] = 5, ["AWS"] = 5, ["Azure"] = 3, ["C#"] = 2 }),

        new("aisha.rahman@example.com", "Aisha Rahman",
            "Product designer turned frontend developer", "London", "United Kingdom", 3,
            EducationLevel.Bachelors, true,
            "Three years as a product designer followed by a move into frontend engineering. Builds interfaces "
            + "in React and TypeScript with a strong grounding in usability and accessibility. Ran user testing "
            + "sessions for a recruitment product and rebuilt its application flow around the findings.",
            new() { ["React"] = 3, ["TypeScript"] = 3, ["JavaScript"] = 4 }),

        new("nuwan.dissanayake@example.com", "Nuwan Dissanayake",
            "Database administrator and SQL developer", "Colombo", "Sri Lanka", 10,
            EducationLevel.Bachelors, false,
            "Ten years administering SQL Server estates for banking and insurance clients. Responsible for "
            + "performance tuning, indexing strategy, backup and disaster recovery. Writes substantial T-SQL "
            + "and has optimised reporting queries running against tables of several hundred million rows.",
            new() { ["SQL Server"] = 10, ["SQL"] = 10, ["C#"] = 3 }),

        new("elena.petrova@example.com", "Elena Petrova",
            "Machine learning engineer", "London", "United Kingdom", 5,
            EducationLevel.Doctorate, true,
            "Five years applying machine learning to text problems, including resume parsing and job matching "
            + "at a recruitment technology company. Comfortable with Python and the usual modelling stack, and "
            + "with the engineering side of shipping models into production behind REST APIs.",
            new() { ["Python"] = 5, ["REST API Design"] = 3, ["Docker"] = 3, ["AWS"] = 3, ["SQL"] = 3 }),

        new("harith.senanayake@example.com", "Harith Senanayake",
            "Recruitment consultant moving into technology", "Colombo", "Sri Lanka", 6,
            EducationLevel.Bachelors, true,
            "Six years as a technical recruitment consultant, placing engineers across Sri Lanka and the "
            + "Maldives. Deep understanding of the hiring process, stakeholder expectations and candidate "
            + "experience. Currently studying software development in the evenings.",
            new() { ["Stakeholder Management"] = 6, ["Agile"] = 2 }),
    ];

    // -----------------------------------------------------------------------
    // Job postings
    // -----------------------------------------------------------------------

    internal static List<JobSpec> Jobs() =>
    [
        new("Meridian HR Consulting", "Technology Practice",
            "Senior Backend Engineer, Payments",
            "Own the settlement service that clears regional card payments across three markets. The role "
            + "covers the design of resilient distributed services, the relational model behind them, and the "
            + "operational discipline to run them. Deep C# and ASP.NET Core experience is expected, together "
            + "with confident relational modelling on SQL Server and containerised deployment.",
            "Design and operate the settlement pipeline. Lead architecture reviews. Mentor two engineers. "
            + "Own the on-call rotation for the service.",
            "Colombo", "Sri Lanka", WorkMode.Hybrid, EmploymentType.FullTime, SeniorityLevel.Senior,
            5, EducationLevel.Bachelors, 4_200_000, 6_000_000, "LKR", RankingStrategyType.Hybrid,
            [("C#", true, 5, 4), ("ASP.NET Core", true, 5, 3), ("SQL Server", true, 4, 3), ("Docker", false, 2, 1)]),

        new("Meridian HR Consulting", "Technology Practice",
            "Frontend Engineer, Client Platforms",
            "Build the interfaces our consultants and their clients use every day. The work is React and "
            + "TypeScript, with a strong emphasis on accessibility and on data-dense screens that stay "
            + "readable. You will shape the design system as much as consume it.",
            "Build and maintain client facing interfaces. Own the component library. Take screens through "
            + "accessibility review.",
            "Colombo", "Sri Lanka", WorkMode.Hybrid, EmploymentType.FullTime, SeniorityLevel.Mid,
            3, EducationLevel.Bachelors, 2_400_000, 3_600_000, "LKR", RankingStrategyType.SkillWeighted,
            [("React", true, 5, 3), ("TypeScript", true, 4, 2), ("JavaScript", false, 3, 3)]),

        new("Meridian HR Consulting", "Technology Practice",
            "Engineering Practice Lead",
            "Lead the technology practice across three offices. This is a leadership role first: hiring, "
            + "delivery predictability, and the technical direction of client engagements. A credible "
            + "engineering background matters more than any particular language.",
            "Lead a group of twelve across Colombo, Singapore and London. Own hiring and delivery. Represent "
            + "the practice to clients at executive level.",
            "Colombo", "Sri Lanka", WorkMode.OnSite, EmploymentType.FullTime, SeniorityLevel.Lead,
            8, EducationLevel.Bachelors, 7_000_000, 10_000_000, "LKR", RankingStrategyType.ExperienceFirst,
            [("Team Leadership", true, 5, 4), ("Stakeholder Management", true, 4, 4), ("Agile", false, 3, 3)]),

        new("Meridian HR Consulting", "Financial Services Practice",
            "Solutions Architect, Banking",
            "Design integration architecture for banking clients moving from core systems to digital "
            + "channels. The work is heavy on settlement, reconciliation and the constraints that come with "
            + "regulated environments. Expect to defend your decisions to risk and compliance.",
            "Own solution architecture on two client engagements. Chair design reviews. Produce architecture "
            + "decision records that survive audit.",
            "Singapore", "Singapore", WorkMode.Hybrid, EmploymentType.FullTime, SeniorityLevel.Lead,
            10, EducationLevel.Masters, 160_000, 220_000, "SGD", RankingStrategyType.ExperienceFirst,
            [("C#", true, 4, 6), ("SQL Server", true, 4, 5), ("Microservices", false, 4, 3), ("Stakeholder Management", true, 5, 5)]),

        new("Meridian HR Consulting", "Financial Services Practice",
            "Data Engineer, Regulatory Reporting",
            "Build the pipelines behind regulatory reporting for financial services clients. Accuracy and "
            + "auditability matter more than throughput here: every number produced has to be traceable back "
            + "to its source.",
            "Design and operate reporting pipelines. Automate reconciliation. Document lineage for audit.",
            "Colombo", "Sri Lanka", WorkMode.Remote, EmploymentType.FullTime, SeniorityLevel.Mid,
            4, EducationLevel.Bachelors, 3_000_000, 4_500_000, "LKR", RankingStrategyType.Hybrid,
            [("Python", true, 5, 3), ("SQL", true, 5, 3), ("SQL Server", false, 3, 2), ("CI/CD", false, 2, 2)]),

        new("Meridian HR Consulting", "Internal Operations",
            "Platform Engineer, Deployment and Reliability",
            "Own the platform every engagement deploys onto. Containers, pipelines and the observability that "
            + "makes an incident diagnosable at three in the morning. The estate spans two cloud providers.",
            "Own the container platform and deployment pipelines. Run incident response. Improve mean time to "
            + "recovery.",
            "London", "United Kingdom", WorkMode.Remote, EmploymentType.FullTime, SeniorityLevel.Senior,
            5, EducationLevel.Unspecified, 70_000, 95_000, "GBP", RankingStrategyType.SkillWeighted,
            [("Docker", true, 5, 4), ("Kubernetes", true, 5, 3), ("CI/CD", true, 4, 4), ("Azure", false, 2, 2), ("AWS", false, 2, 2)]),

        new("Meridian HR Consulting", "Technology Practice",
            "QA Automation Engineer",
            "Own the automated testing that lets client engagements release with confidence. The role spans "
            + "API testing, end to end coverage and the release verification process itself.",
            "Build and maintain regression suites. Run release verification. Work with engineers on "
            + "testability.",
            "Colombo", "Sri Lanka", WorkMode.Hybrid, EmploymentType.FullTime, SeniorityLevel.Mid,
            3, EducationLevel.Bachelors, 2_200_000, 3_400_000, "LKR", RankingStrategyType.SkillWeighted,
            [("Unit Testing", true, 5, 3), ("REST API Design", true, 4, 2), ("CI/CD", false, 3, 2)]),

        new("Meridian HR Consulting", "Internal Operations",
            "Graduate Software Engineer",
            "A first role for a recent graduate. You will join an engagement team, pair with senior engineers "
            + "and pick up production work quickly. We care about how you think, not how much you already "
            + "know.",
            "Contribute to a client engagement under supervision. Learn the codebase and the review process.",
            "Colombo", "Sri Lanka", WorkMode.OnSite, EmploymentType.FullTime, SeniorityLevel.Junior,
            0, EducationLevel.Bachelors, 900_000, 1_400_000, "LKR", RankingStrategyType.Hybrid,
            [("C#", false, 3, 0), ("SQL", false, 2, 0)]),

        new("Northwind Logistics", "Engineering",
            "Backend Engineer, Warehouse Systems",
            "Build the services behind warehouse operations across the region. Stock movements, pick paths "
            + "and the integrations to carrier systems. The domain is unglamorous and genuinely hard.",
            "Own warehouse management services. Integrate with carrier APIs. Support operations during peak.",
            "Singapore", "Singapore", WorkMode.OnSite, EmploymentType.FullTime, SeniorityLevel.Mid,
            4, EducationLevel.Bachelors, 90_000, 120_000, "SGD", RankingStrategyType.Hybrid,
            [("Java", false, 4, 3), ("C#", false, 4, 3), ("SQL", true, 4, 3), ("Microservices", false, 3, 2)]),

        new("Northwind Logistics", "Operations",
            "Operations Systems Analyst",
            "Sit between the operations floor and the engineering team. Translate what warehouse supervisors "
            + "need into requirements engineers can build, and explain back what is and is not possible.",
            "Gather and document requirements. Run user acceptance testing. Train operational staff.",
            "Singapore", "Singapore", WorkMode.OnSite, EmploymentType.FullTime, SeniorityLevel.Mid,
            3, EducationLevel.Bachelors, 70_000, 90_000, "SGD", RankingStrategyType.TfIdfSimilarity,
            [("Stakeholder Management", true, 4, 2), ("Agile", false, 3, 2)]),

        new("Halcyon Financial Group", "Digital Banking",
            "Senior Full Stack Engineer, Digital Banking",
            "Build the customer facing digital banking experience end to end. React on the front, ASP.NET "
            + "Core behind it, and a relational model that has to satisfy both product and audit.",
            "Deliver features across the stack. Participate in security review. Support the release process.",
            "London", "United Kingdom", WorkMode.Hybrid, EmploymentType.FullTime, SeniorityLevel.Senior,
            5, EducationLevel.Bachelors, 75_000, 105_000, "GBP", RankingStrategyType.Hybrid,
            [("C#", true, 5, 4), ("ASP.NET Core", true, 4, 3), ("React", true, 4, 3), ("SQL Server", false, 3, 2)]),

        new("Halcyon Financial Group", "Risk and Compliance",
            "Database Administrator",
            "Own the SQL Server estate supporting risk and compliance reporting. Performance, recovery and "
            + "the ability to answer an auditor's question about any figure in any report.",
            "Administer the SQL Server estate. Own backup and recovery. Tune reporting workloads.",
            "London", "United Kingdom", WorkMode.OnSite, EmploymentType.FullTime, SeniorityLevel.Senior,
            6, EducationLevel.Bachelors, 65_000, 85_000, "GBP", RankingStrategyType.SkillWeighted,
            [("SQL Server", true, 5, 5), ("SQL", true, 5, 5)]),
    ];
}
