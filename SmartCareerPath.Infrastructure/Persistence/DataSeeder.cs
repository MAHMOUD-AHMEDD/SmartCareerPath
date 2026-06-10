// =============================================================================
// 
//  Covers: Roles, Admin, LookupTypes, LookupValues, CareerTracks,
//          Roadmaps + RoadmapItems, Questions + Options,
//          Seekers, Mentors, Answers, SeekerQuestionOptions,
//          Recommendations, SeekerRoadmapProgress
// =============================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager);
        await SeedLookupsAsync(db);
        await SeedCareerTracksAsync(db);
        await SeedRoadmapsAsync(db);
        await SeedQuestionsAsync(db);
        await SeedSeekersAsync(userManager, db);
        await SeedMentorsAsync(userManager, db);
        await SeedSeekersAnswersAndRecommendationsAsync(db);
    }

    // -------------------------------------------------------------------------
    // 1. ROLES
    // -------------------------------------------------------------------------
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Seeker", "Mentor", "Admin" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    // -------------------------------------------------------------------------
    // 2. ADMIN
    // -------------------------------------------------------------------------
    private static async Task SeedAdminAsync(UserManager<AppUser> userManager)
    {
        const string email = "admin@smartcareer.com";
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var admin = new Admin
        {
            UserName = email,
            Email = email,
            FirstName = "System",
            LastName = "Admin",
            EmailConfirmed = true,
            RegistrationDate = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(admin, "Admin@12345");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    // -------------------------------------------------------------------------
    // 3. LOOKUP TYPES + VALUES
    //    LookupType 1 = "JobTitle"   → values used as CurrentJobId for users
    //    LookupType 2 = "Industry"   → values describe industry sectors
    // -------------------------------------------------------------------------
    private static async Task SeedLookupsAsync(AppDbContext db)
    {
        if (await db.LookupTypes.AnyAsync()) return;

        var jobTitleType = new LookupType { Name = "JobTitle" };
        var industryType = new LookupType { Name = "Industry" };

        db.LookupTypes.AddRange(jobTitleType, industryType);
        await db.SaveChangesAsync();

        db.LookupValues.AddRange(
            // Job titles  (LookupTypeId = jobTitleType.Id)
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "Student" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "Fresh Graduate" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "Junior Developer" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "Mid-Level Developer" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "Senior Developer" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "Data Analyst" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "UX Designer" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "Product Manager" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "DevOps Engineer" },
            new LookupValue { LookupTypeId = jobTitleType.Id, Value = "Cybersecurity Analyst" },

            // Industries  (LookupTypeId = industryType.Id)
            new LookupValue { LookupTypeId = industryType.Id, Value = "Technology" },
            new LookupValue { LookupTypeId = industryType.Id, Value = "Finance" },
            new LookupValue { LookupTypeId = industryType.Id, Value = "Healthcare" },
            new LookupValue { LookupTypeId = industryType.Id, Value = "Education" },
            new LookupValue { LookupTypeId = industryType.Id, Value = "E-commerce" }
        );
        await db.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 4. CAREER TRACKS
    //    5 tracks matching common CS/tech career paths
    // -------------------------------------------------------------------------
    private static async Task SeedCareerTracksAsync(AppDbContext db)
    {
        if (await db.CareerTracks.AnyAsync()) return;

        db.CareerTracks.AddRange(
            new CareerTrack
            {
                Name = "Backend Development",
                Description = "Build robust server-side systems, APIs, and databases. "
                            + "Master languages like C#, Java, or Node.js alongside "
                            + "cloud infrastructure and system design."
            },
            new CareerTrack
            {
                Name = "Frontend Development",
                Description = "Create engaging web interfaces using modern JavaScript "
                            + "frameworks like React or Angular. Focus on UX, "
                            + "accessibility, and performance."
            },
            new CareerTrack
            {
                Name = "Data Science & AI",
                Description = "Analyse large datasets, build machine-learning models, "
                            + "and deploy intelligent systems using Python, TensorFlow, "
                            + "and cloud ML platforms."
            },
            new CareerTrack
            {
                Name = "Cybersecurity",
                Description = "Protect systems and networks from threats. Learn ethical "
                            + "hacking, penetration testing, threat analysis, and "
                            + "security compliance frameworks."
            },
            new CareerTrack
            {
                Name = "DevOps & Cloud Engineering",
                Description = "Automate software delivery pipelines, manage cloud "
                            + "infrastructure with AWS/Azure/GCP, and master "
                            + "containerisation with Docker and Kubernetes."
            }
        );
        await db.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 5. ROADMAPS + ROADMAP ITEMS
    //    One roadmap per career track (enforced by unique index in Phase 1)
    //    Each roadmap has 5 ordered items
    // -------------------------------------------------------------------------
    private static async Task SeedRoadmapsAsync(AppDbContext db)
    {
        if (await db.Roadmaps.AnyAsync()) return;

        var tracks = await db.CareerTracks.OrderBy(t => t.Id).ToListAsync();

        // Helper to build a Roadmap with its items inline
        static Roadmap BuildRoadmap(
            CareerTrack track, string title, string desc,
            IEnumerable<(string t, string d, string? link)> items)
        {
            var roadmap = new Roadmap
            {
                TrackId = track.Id,
                Title = title,
                Description = desc,
                Items = items.Select((item, idx) => new RoadmapItem
                {
                    Title = item.t,
                    Description = item.d,
                    OrderIndex = idx + 1,
                    DefaultStatus = "NotStarted",
                    Link = item.link
                }).ToList()
            };
            return roadmap;
        }

        var backendTrack = tracks.First(t => t.Name == "Backend Development");
        var frontendTrack = tracks.First(t => t.Name == "Frontend Development");
        var dsTrack = tracks.First(t => t.Name == "Data Science & AI");
        var secTrack = tracks.First(t => t.Name == "Cybersecurity");
        var devopsTrack = tracks.First(t => t.Name == "DevOps & Cloud Engineering");

        db.Roadmaps.AddRange(

            // --- Backend ---
            BuildRoadmap(backendTrack,
                "Backend Developer Roadmap",
                "Step-by-step guide from zero to production-ready backend engineer.",
                new[]
                {
                    ("Master C# & .NET Fundamentals",
                     "OOP, LINQ, async/await, dependency injection, and the .NET runtime model.",
                     "https://learn.microsoft.com/en-us/dotnet/csharp/"),
                    ("Build REST APIs with ASP.NET Core",
                     "Controllers, routing, middleware, model binding, and Swagger documentation.",
                     "https://learn.microsoft.com/en-us/aspnet/core/web-api/"),
                    ("Database Design & Entity Framework Core",
                     "Relational modelling, EF Core migrations, LINQ queries, and performance tuning.",
                     "https://learn.microsoft.com/en-us/ef/core/"),
                    ("Authentication & Security",
                     "JWT, OAuth 2.0, ASP.NET Identity, HTTPS, and OWASP top-10 mitigations.",
                     "https://owasp.org/www-project-top-ten/"),
                    ("Cloud Deployment & CI/CD",
                     "Docker containers, GitHub Actions pipelines, and deploying to Azure or AWS.",
                     "https://docs.docker.com/get-started/")
                }),

            // --- Frontend ---
            BuildRoadmap(frontendTrack,
                "Frontend Developer Roadmap",
                "From HTML basics to production React applications.",
                new[]
                {
                    ("HTML5, CSS3 & Responsive Design",
                     "Semantic markup, Flexbox, Grid, media queries, and accessibility standards.",
                     "https://developer.mozilla.org/en-US/docs/Web"),
                    ("JavaScript & TypeScript Essentials",
                     "ES2022+, closures, promises, async/await, and TypeScript type system.",
                     "https://www.typescriptlang.org/docs/"),
                    ("React & State Management",
                     "Hooks, component patterns, Context API, Redux Toolkit, and React Query.",
                     "https://react.dev/"),
                    ("Testing & Performance",
                     "Jest, React Testing Library, Lighthouse audits, and Core Web Vitals.",
                     "https://jestjs.io/"),
                    ("Build Tools & Deployment",
                     "Vite, Webpack basics, CI pipelines, and deploying to Vercel or Netlify.",
                     "https://vitejs.dev/")
                }),

            // --- Data Science ---
            BuildRoadmap(dsTrack,
                "Data Science & AI Roadmap",
                "Build a foundation in statistics, ML, and AI engineering.",
                new[]
                {
                    ("Python for Data Science",
                     "NumPy, Pandas, Matplotlib, and Seaborn for data wrangling and visualisation.",
                     "https://pandas.pydata.org/docs/"),
                    ("Statistics & Probability",
                     "Descriptive stats, hypothesis testing, regression, and Bayesian thinking.",
                     "https://www.khanacademy.org/math/statistics-probability"),
                    ("Machine Learning with scikit-learn",
                     "Supervised and unsupervised learning, model evaluation, and pipelines.",
                     "https://scikit-learn.org/stable/"),
                    ("Deep Learning with TensorFlow / PyTorch",
                     "Neural networks, CNNs, RNNs, transfer learning, and model deployment.",
                     "https://www.tensorflow.org/tutorials"),
                    ("Data Engineering & MLOps",
                     "SQL/NoSQL databases, Spark basics, MLflow experiment tracking, and API deployment.",
                     "https://mlflow.org/docs/latest/index.html")
                }),

            // --- Cybersecurity ---
            BuildRoadmap(secTrack,
                "Cybersecurity Roadmap",
                "From networking fundamentals to ethical hacking and cloud security.",
                new[]
                {
                    ("Networking & Operating Systems",
                     "TCP/IP, DNS, HTTP/S, Linux CLI, file permissions, and process management.",
                     "https://www.comptia.org/certifications/network"),
                    ("Security Fundamentals & Cryptography",
                     "CIA triad, encryption algorithms, PKI, TLS, and certificate management.",
                     "https://www.coursera.org/learn/crypto"),
                    ("Ethical Hacking & Penetration Testing",
                     "Kali Linux, Metasploit, Burp Suite, OWASP methodology, and CTF practice.",
                     "https://www.hackthebox.com/"),
                    ("Threat Analysis & Incident Response",
                     "SIEM tools, log analysis, threat modelling, and forensic investigation.",
                     "https://www.splunk.com/en_us/training.html"),
                    ("Cloud Security & Compliance",
                     "AWS/Azure security services, IAM policies, SOC 2, ISO 27001 basics.",
                     "https://aws.amazon.com/security/")
                }),

            // --- DevOps ---
            BuildRoadmap(devopsTrack,
                "DevOps & Cloud Engineering Roadmap",
                "Automate, scale, and secure modern software delivery pipelines.",
                new[]
                {
                    ("Linux, Bash & Version Control",
                     "Shell scripting, file system, systemd, and advanced Git workflows.",
                     "https://www.gnu.org/software/bash/manual/"),
                    ("Docker & Container Orchestration",
                     "Dockerfile, docker-compose, Kubernetes pods, deployments, and services.",
                     "https://kubernetes.io/docs/home/"),
                    ("CI/CD Pipelines",
                     "GitHub Actions, GitLab CI, Jenkins, artifact management, and deployment strategies.",
                     "https://docs.github.com/en/actions"),
                    ("Infrastructure as Code",
                     "Terraform, AWS CloudFormation, Ansible, and managing state safely.",
                     "https://developer.hashicorp.com/terraform/docs"),
                    ("Monitoring, Logging & Reliability",
                     "Prometheus, Grafana, ELK stack, SLOs, and on-call runbooks.",
                     "https://prometheus.io/docs/introduction/overview/")
                })
        );

        await db.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 6. QUESTIONS + OPTIONS
    //    10 MCQ questions (one per career-relevant skill area) +
    //    2 OpenText  questions
    // -------------------------------------------------------------------------
    private static async Task SeedQuestionsAsync(AppDbContext db)
    {
        if (await db.Questions.AnyAsync()) return;

        // ── MCQ helper ────────────────────────────────────────────────────────
        static Question MCQ(string text, params string[] options)
            => new()
            {
                QuestionText = text,
                QuestionType = "MCQ",
                Options = options.Select(o => new QuestionOption
                {
                    OptionText = o,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };

        // ── OpenText helper ───────────────────────────────────────────────────
        static Question Open(string text)
            => new() { QuestionText = text, QuestionType = "OpenText" };

        db.Questions.AddRange(

            // Q1 — preferred work type
            MCQ("What type of work do you enjoy most?",
                "Building and designing systems or applications",
                "Analysing data and finding patterns",
                "Securing and protecting systems",
                "Automating and managing infrastructure"),

            // Q2 — programming preference
            MCQ("Which programming activity excites you the most?",
                "Writing server-side logic and APIs",
                "Creating interactive user interfaces",
                "Training machine-learning models",
                "Writing automation scripts and pipelines"),

            // Q3 — problem-solving style
            MCQ("How do you prefer to solve problems?",
                "Breaking down large systems into components",
                "Visualising data to find insights",
                "Thinking like an attacker to find weaknesses",
                "Optimising processes for speed and reliability"),

            // Q4 — preferred tools
            MCQ("Which set of tools appeals to you the most?",
                "C#, .NET, SQL Server, Docker",
                "React, TypeScript, CSS, Figma",
                "Python, Jupyter, TensorFlow, SQL",
                "Linux, Kubernetes, Terraform, Prometheus"),

            // Q5 — work environment
            MCQ("What kind of team role suits you best?",
                "Back-end engineer responsible for APIs and databases",
                "Front-end engineer responsible for the user experience",
                "Data engineer or ML engineer building intelligent features",
                "Cloud/platform engineer ensuring reliability at scale"),

            // Q6 — learning preference
            MCQ("How do you prefer to learn new technology?",
                "Reading official documentation and building projects",
                "Watching design tutorials and replicating UIs",
                "Experimenting with datasets and Kaggle competitions",
                "Following cloud certifications and labs"),

            // Q7 — career goal
            MCQ("What is your primary career goal in the next two years?",
                "Land a backend or full-stack developer role",
                "Become a frontend or UI engineer",
                "Join a data science or AI team",
                "Work as a security analyst or cloud engineer"),

            // Q8 — strength
            MCQ("What is your greatest technical strength right now?",
                "Writing clean, well-structured code",
                "Turning designs into pixel-perfect web pages",
                "Working with numbers, statistics, and spreadsheets",
                "Configuring servers, networks, or cloud services"),

            // Q9 — interest in security
            MCQ("How interested are you in cybersecurity topics?",
                "Very interested — I want to make it my career",
                "Somewhat interested — I want security awareness",
                "Neutral — security is just one aspect of my work",
                "Not my focus — I prefer other areas"),

            // Q10 — cloud comfort
            MCQ("How comfortable are you with cloud and DevOps concepts?",
                "Very comfortable — I already use cloud services",
                "Somewhat comfortable — I have basic knowledge",
                "Learning — I am just getting started",
                "Not yet — I prefer local development for now"),

            // Q11 — open text: background
            Open("Briefly describe your educational background and any relevant work experience."),

            // Q12 — open text: goal
            Open("What specific career outcome are you hoping to achieve in the next 12 months?")
        );

        await db.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 7. SEEKERS  (3 seeker users)
    // -------------------------------------------------------------------------
    private static async Task SeedSeekersAsync(
        UserManager<AppUser> userManager, AppDbContext db)
    {
        // Re-use LookupValue IDs seeded in step 3
        var studentJobId = (await db.LookupValues
            .FirstOrDefaultAsync(v => v.Value == "Student"))?.Id;
        var freshGradJobId = (await db.LookupValues
            .FirstOrDefaultAsync(v => v.Value == "Fresh Graduate"))?.Id;

        var seekerDefs = new[]
        {
            new
            {
                Email     = "ahmed.hassan@seeker.com",
                Password  = "Seeker@1234",
                FirstName = "Ahmed",
                LastName  = "Hassan",
                LinkedIn  = "https://linkedin.com/in/ahmed-hassan",
                JobId     = studentJobId
            },
            new
            {
                Email     = "sara.ali@seeker.com",
                Password  = "Seeker@1234",
                FirstName = "Sara",
                LastName  = "Ali",
                LinkedIn  = "https://linkedin.com/in/sara-ali",
                JobId     = freshGradJobId
            },
            new
            {
                Email     = "omar.khaled@seeker.com",
                Password  = "Seeker@1234",
                FirstName = "Omar",
                LastName  = "Khaled",
                LinkedIn  = (string?)null,
                JobId     = studentJobId
            }
        };

        foreach (var def in seekerDefs)
        {
            if (await userManager.FindByEmailAsync(def.Email) is not null) continue;

            var seeker = new Seeker
            {
                UserName = def.Email,
                Email = def.Email,
                FirstName = def.FirstName,
                LastName = def.LastName,
                LinkedIn = def.LinkedIn,
                CurrentJobId = def.JobId,
                EmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(seeker, def.Password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(seeker, "Seeker");
        }
    }

    // -------------------------------------------------------------------------
    // 8. MENTORS  (3 mentor users, each assigned to a different track)
    // -------------------------------------------------------------------------
    private static async Task SeedMentorsAsync(
        UserManager<AppUser> userManager, AppDbContext db)
    {
        var tracks = await db.CareerTracks.ToDictionaryAsync(t => t.Name, t => t.Id);
        var seniorDevJobId = (await db.LookupValues
            .FirstOrDefaultAsync(v => v.Value == "Senior Developer"))?.Id;

        var mentorDefs = new[]
        {
            new
            {
                Email              = "khaled.omar@mentor.com",
                Password           = "Mentor@1234",
                FirstName          = "Khaled",
                LastName           = "Omar",
                YearsOfExperience  = 8,
                TotalStudentsTaught= 120,
                Description        = "Senior .NET backend engineer with 8 years at top-tier "
                                   + "Egyptian tech companies. I help seekers break into "
                                   + "backend roles with hands-on API and database projects.",
                Company            = "Instabug",
                LinkedIn           = "https://linkedin.com/in/khaled-omar",
                TrackName          = "Backend Development",
                CurrentJobId       = seniorDevJobId
            },
            new
            {
                Email              = "nour.ibrahim@mentor.com",
                Password           = "Mentor@1234",
                FirstName          = "Nour",
                LastName           = "Ibrahim",
                YearsOfExperience  = 5,
                TotalStudentsTaught= 85,
                Description        = "Frontend engineer specialising in React and TypeScript. "
                                   + "I mentor career changers who want to build polished, "
                                   + "accessible web applications.",
                Company            = "Swvl",
                LinkedIn           = "https://linkedin.com/in/nour-ibrahim",
                TrackName          = "Frontend Development",
                CurrentJobId       = seniorDevJobId
            },
            new
            {
                Email              = "youssef.salem@mentor.com",
                Password           = "Mentor@1234",
                FirstName          = "Youssef",
                LastName           = "Salem",
                YearsOfExperience  = 10,
                TotalStudentsTaught= 200,
                Description        = "Data science lead with a decade of experience in ML "
                                   + "and AI product development. Passionate about making "
                                   + "data science accessible to new graduates.",
                Company            = "Robusta Studio",
                LinkedIn           = "https://linkedin.com/in/youssef-salem",
                TrackName          = "Data Science & AI",
                CurrentJobId       = seniorDevJobId
            }
        };

        foreach (var def in mentorDefs)
        {
            if (await userManager.FindByEmailAsync(def.Email) is not null) continue;

            var mentor = new Mentor
            {
                UserName = def.Email,
                Email = def.Email,
                FirstName = def.FirstName,
                LastName = def.LastName,
                YearsOfExperience = def.YearsOfExperience,
                TotalStudentsTaught = def.TotalStudentsTaught,
                Description = def.Description,
                Company = def.Company,
                LinkedIn = def.LinkedIn,
                TrackId = tracks.TryGetValue(def.TrackName, out var tid) ? tid : null,
                CurrentJobId = def.CurrentJobId,
                EmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(mentor, def.Password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(mentor, "Mentor");
        }
    }

    // -------------------------------------------------------------------------
    // 9. SEEKER ANSWERS, QUESTION OPTIONS, RECOMMENDATIONS, ROADMAP PROGRESS
    //    Only seeds if recommendations table is still empty (idempotent guard).
    //    Simulates one fully-completed seeker journey for "Ahmed Hassan".
    // -------------------------------------------------------------------------
    private static async Task SeedSeekersAnswersAndRecommendationsAsync(AppDbContext db)
    {
        if (await db.Recommendations.AnyAsync()) return;

        // Find Ahmed Hassan (our demo seeker)
        var ahmed = await db.Seekers
            .FirstOrDefaultAsync(s => s.Email == "ahmed.hassan@seeker.com");
        if (ahmed is null) return;

        var questions = await db.Questions
            .Include(q => q.Options)
            .OrderBy(q => q.Id)
            .ToListAsync();

        if (questions.Count < 10) return;  // safety — ensure questions were seeded

        // ── MCQ Answers (Questions 1–10) ─────────────────────────────────────
        // Ahmed's answers consistently point toward Backend Development (option index 0)
        var mcqPairs = new[]
        {
            (questions[0], 0),  // "Building and designing systems" → Backend
            (questions[1], 0),  // "Writing server-side logic" → Backend
            (questions[2], 0),  // "Breaking down large systems" → Backend
            (questions[3], 0),  // "C#, .NET, SQL Server, Docker" → Backend
            (questions[4], 0),  // "Back-end engineer" → Backend
            (questions[5], 0),  // "Reading documentation and building" → Backend
            (questions[6], 0),  // "Land a backend/full-stack role" → Backend
            (questions[7], 0),  // "Writing clean structured code" → Backend
            (questions[8], 2),  // "Neutral on security" → Backend-adjacent
            (questions[9], 0),  // "Very comfortable with cloud" → DevOps adjacent
        };

        var seekerOptions = new List<SeekerQuestionOption>();
        foreach (var (question, optIdx) in mcqPairs)
        {
            if (question.Options.Count <= optIdx) continue;
            var option = question.Options.OrderBy(o => o.Id).ElementAt(optIdx);

            var alreadyAnswered = await db.SeekerQuestionOptions
                .AnyAsync(s => s.SeekerId == ahmed.Id && s.QuestionId == question.Id);
            if (!alreadyAnswered)
            {
                seekerOptions.Add(new SeekerQuestionOption
                {
                    SeekerId = ahmed.Id,
                    QuestionId = question.Id,
                    OptionId = option.Id,
                    SelectedAt = DateTime.UtcNow
                });
            }
        }
        if (seekerOptions.Any())
        {
            db.SeekerQuestionOptions.AddRange(seekerOptions);
            await db.SaveChangesAsync();
        }

        // ── Open-Text Answers (Questions 11–12) ──────────────────────────────
        if (questions.Count >= 12)
        {
            var openQ1 = questions[10]; // educational background
            var openQ2 = questions[11]; // career goal

            var existingAnswers = await db.Answers
                .Where(a => a.SeekerId == ahmed.Id)
                .Select(a => a.QuestionId)
                .ToListAsync();

            var newAnswers = new List<Answer>();

            if (!existingAnswers.Contains(openQ1.Id))
                newAnswers.Add(new Answer
                {
                    SeekerId = ahmed.Id,
                    QuestionId = openQ1.Id,
                    AnswerText = "I am a third-year Computer Science student at Fayoum University. "
                               + "I completed an internship at a local software house where I "
                               + "built CRUD APIs using ASP.NET Core and SQL Server.",
                    AnsweredAt = DateTime.UtcNow
                });

            if (!existingAnswers.Contains(openQ2.Id))
                newAnswers.Add(new Answer
                {
                    SeekerId = ahmed.Id,
                    QuestionId = openQ2.Id,
                    AnswerText = "I want to land a junior backend developer position at a "
                               + "product company in Egypt within 12 months of graduation. "
                               + "My goal is to build production-quality .NET APIs.",
                    AnsweredAt = DateTime.UtcNow
                });

            if (newAnswers.Any())
            {
                db.Answers.AddRange(newAnswers);
                await db.SaveChangesAsync();
            }
        }

        // ── Recommendations (simulate AI output for Ahmed) ───────────────────
        var tracks = await db.CareerTracks.ToDictionaryAsync(t => t.Name, t => t.Id);

        var recommendations = new List<Recommendation>();
        var rankMap = new[]
        {
            ("Backend Development",       1),
            ("DevOps & Cloud Engineering",2),
            ("Frontend Development",      3),
            ("Data Science & AI",         4),
            ("Cybersecurity",             5),
        };

        foreach (var (trackName, rank) in rankMap)
        {
            if (!tracks.TryGetValue(trackName, out var trackId)) continue;

            var exists = await db.Recommendations.AnyAsync(
                r => r.SeekerId == ahmed.Id && r.TrackId == trackId);
            if (!exists)
                recommendations.Add(new Recommendation
                {
                    SeekerId = ahmed.Id,
                    TrackId = trackId,
                    Rank = rank,
                    RecommendedAt = DateTime.UtcNow
                });
        }

        if (recommendations.Any())
        {
            db.Recommendations.AddRange(recommendations);
            await db.SaveChangesAsync();
        }

        // ── Roadmap Progress (Ahmed started the Backend roadmap) ─────────────
        if (!tracks.TryGetValue("Backend Development", out var backendTrackId)) return;

        var backendRoadmap = await db.Roadmaps
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.TrackId == backendTrackId);

        if (backendRoadmap is null) return;

        var orderedItems = backendRoadmap.Items.OrderBy(i => i.OrderIndex).ToList();

        // Ahmed completed item 1, is in progress on item 2, rest not started
        var progressMap = new Dictionary<int, string>();
        for (var i = 0; i < orderedItems.Count; i++)
        {
            var status = i == 0 ? "Completed"
                       : i == 1 ? "InProgress"
                       : "NotStarted";
            progressMap[orderedItems[i].Id] = status;
        }

        foreach (var (itemId, status) in progressMap)
        {
            var exists = await db.SeekerRoadmapProgress.AnyAsync(
                p => p.SeekerId == ahmed.Id && p.RoadmapItemId == itemId);
            if (!exists)
                db.SeekerRoadmapProgress.Add(new SeekerRoadmapProgress
                {
                    SeekerId = ahmed.Id,
                    RoadmapItemId = itemId,
                    Status = status,
                    UpdatedAt = DateTime.UtcNow
                });
        }

        await db.SaveChangesAsync();
    }
}
