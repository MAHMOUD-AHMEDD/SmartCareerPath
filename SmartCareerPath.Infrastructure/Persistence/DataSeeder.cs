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
                Name = "Artificial Intelligence",
                Description = "Build intelligent systems using deep learning, NLP, and AI engineering. "
                            + "Master Python, PyTorch/TensorFlow, and ML model deployment."
            },
            new CareerTrack
            {
                Name = "Data Science",
                Description = "Analyse large datasets, build predictive models, and extract business insights "
                            + "using Python, Pandas, scikit-learn, and data visualisation tools."
            },
            new CareerTrack
            {
                Name = "Development",
                Description = "Build web and mobile applications from frontend to backend. "
                            + "Master frameworks like React, Node.js, ASP.NET Core, and databases."
            },
            new CareerTrack
            {
                Name = "Security",
                Description = "Protect systems and networks from threats. Learn ethical hacking, "
                            + "penetration testing, network security, and compliance frameworks."
            },
            new CareerTrack
            {
                Name = "Software Development and Engineering",
                Description = "Design scalable software systems with solid engineering principles. "
                            + "Focus on system design, cloud computing, DevOps, and software quality."
            },
            new CareerTrack
            {
                Name = "User Experience (UX) and UI Design",
                Description = "Create intuitive and beautiful digital experiences. "
                            + "Master UX research, UI design, Figma, and accessibility principles."
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

        var tracks = await db.CareerTracks.ToDictionaryAsync(t => t.Name, t => t.Id);

        static Roadmap Build(int trackId, string title, string desc,
            params (string t, string d, string? link)[] items) => new()
            {
                TrackId = trackId,
                Title = title,
                Description = desc,
                Items = items.Select((x, i) => new RoadmapItem
                {
                    Title = x.t,
                    Description = x.d,
                    OrderIndex = i + 1,
                    DefaultStatus = "NotStarted",
                    Link = x.link
                }).ToList()
            };

        db.Roadmaps.AddRange(

            Build(tracks["Artificial Intelligence"],
                "Artificial Intelligence Roadmap",
                "From Python basics to deploying production AI models.",
                ("Python & Math Foundations", "NumPy, Pandas, linear algebra, calculus, and probability.", "https://www.python.org/"),
                ("Machine Learning Basics", "scikit-learn, supervised/unsupervised learning, evaluation.", "https://scikit-learn.org/"),
                ("Deep Learning", "PyTorch or TensorFlow, CNNs, RNNs, transformers.", "https://pytorch.org/tutorials/"),
                ("NLP & Computer Vision", "HuggingFace, BERT, YOLO, image classification pipelines.", "https://huggingface.co/"),
                ("MLOps & Deployment", "MLflow, FastAPI, Docker, model serving on cloud platforms.", "https://mlflow.org/")),

            Build(tracks["Data Science"],
                "Data Science Roadmap",
                "From data wrangling to business insight delivery.",
                ("Python for Data Science", "Pandas, NumPy, Matplotlib, Seaborn.", "https://pandas.pydata.org/"),
                ("Statistics & Probability", "Hypothesis testing, regression, Bayesian thinking.", "https://www.khanacademy.org/math/statistics-probability"),
                ("SQL & Databases", "Advanced SQL, window functions, data modelling.", "https://mode.com/sql-tutorial/"),
                ("Machine Learning", "scikit-learn pipelines, feature engineering, model selection.", "https://scikit-learn.org/"),
                ("Data Visualisation & BI", "Tableau, Power BI, Plotly, storytelling with data.", "https://public.tableau.com/")),

            Build(tracks["Development"],
                "Development Roadmap",
                "Full-stack web and mobile application development.",
                ("HTML, CSS & JavaScript", "Responsive design, ES2022+, TypeScript basics.", "https://developer.mozilla.org/"),
                ("Frontend Framework", "React or Angular — components, state, routing.", "https://react.dev/"),
                ("Backend & APIs", "Node.js or ASP.NET Core, REST APIs, authentication.", "https://learn.microsoft.com/en-us/aspnet/core/"),
                ("Databases", "SQL and NoSQL, ORM basics, query optimisation.", "https://www.postgresql.org/"),
                ("Deployment & DevOps", "Docker, CI/CD pipelines, cloud hosting.", "https://docs.docker.com/")),

            Build(tracks["Security"],
                "Cybersecurity Roadmap",
                "From networking fundamentals to ethical hacking.",
                ("Networking & OS Basics", "TCP/IP, Linux CLI, file permissions, process management.", "https://www.comptia.org/certifications/network"),
                ("Security Fundamentals", "CIA triad, encryption, PKI, TLS, certificate management.", "https://www.coursera.org/learn/crypto"),
                ("Ethical Hacking", "Kali Linux, Metasploit, Burp Suite, OWASP methodology.", "https://www.hackthebox.com/"),
                ("Threat Analysis", "SIEM tools, log analysis, threat modelling, forensics.", "https://www.splunk.com/"),
                ("Cloud Security", "AWS/Azure security services, IAM, SOC 2, ISO 27001.", "https://aws.amazon.com/security/")),

            Build(tracks["Software Development and Engineering"],
                "Software Engineering Roadmap",
                "System design, cloud, DevOps, and engineering excellence.",
                ("Data Structures & Algorithms", "Big-O, sorting, graphs, dynamic programming.", "https://leetcode.com/"),
                ("System Design", "Scalability, load balancing, caching, microservices.", "https://github.com/donnemartin/system-design-primer"),
                ("Cloud & Infrastructure", "AWS/Azure, Terraform, infrastructure as code.", "https://developer.hashicorp.com/terraform/docs"),
                ("CI/CD & DevOps", "GitHub Actions, Docker, Kubernetes, deployment strategies.", "https://docs.github.com/en/actions"),
                ("Software Quality", "Unit testing, TDD, code review, technical debt management.", "https://martinfowler.com/")),

            Build(tracks["User Experience (UX) and UI Design"],
                "UX & UI Design Roadmap",
                "From design thinking to production-ready interfaces.",
                ("Design Fundamentals", "Typography, colour theory, layout, visual hierarchy.", "https://www.canva.com/learn/design/"),
                ("UX Research & Thinking", "User interviews, personas, journey maps, usability testing.", "https://www.nngroup.com/"),
                ("Figma & Prototyping", "Wireframes, interactive prototypes, component libraries.", "https://www.figma.com/"),
                ("Accessibility & Standards", "WCAG guidelines, inclusive design, screen reader testing.", "https://www.w3.org/WAI/"),
                ("Handoff & Collaboration", "Dev handoff in Figma, design systems, working with devs.", "https://zeroheight.com/"))
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

        static Question YesNo(string text) => new()
        {
            QuestionText = text,
            QuestionType = "MCQ",
            Options =
            [
                new QuestionOption { OptionText = "Yes", CreatedAt = DateTime.UtcNow },
            new QuestionOption { OptionText = "No",  CreatedAt = DateTime.UtcNow }
            ]
        };

        db.Questions.AddRange(
            YesNo("When you visit a page online, are you curious how its look, buttons, and pages were actually made?"),
            YesNo("Do you ever look at an app on your phone and wonder how it was built?"),
            YesNo("Are you fascinated by computers that can make decisions or 'think' on their own, like chatbots or self-driving cars?"),
            YesNo("When something is broken or confusing, do you enjoy digging in step by step until you figure out why?"),
            YesNo("Do you find it interesting how hackers break into systems, and how people defend against them?"),
            YesNo("Are you curious how your phone connects to WiFi, or how computers talk to each other over the internet?"),
            YesNo("Do you like the idea of organizing huge amounts of information so it's easy to find later?"),
            YesNo("Do you enjoy looking at numbers or spreadsheets and turning them into charts that tell a story?"),
            YesNo("Do you enjoy checking whether two separate apps or systems actually work correctly when connected together?"),
            YesNo("Are you the type of person who gets annoyed when an app is slow, and wants to know exactly why?"),
            YesNo("Do you enjoy working with probabilities and numbers, like predicting outcomes or analyzing surveys?"),
            YesNo("Are you curious about how a computer can learn to recognize a face or voice just from seeing lots of examples?"),
            YesNo("Would you find it exciting to teach a computer to improve at a task just by giving it more data?"),
            YesNo("Do you like the idea of building the 'pipes' that move information from one place to another automatically?"),
            YesNo("Are you curious how things like Google Drive or Netflix run on remote servers in the cloud?"),
            YesNo("Are you curious about how cryptocurrencies like Bitcoin keep records securely without one central authority?"),
            YesNo("Do you enjoy planning things out on a big-picture level, like how all pieces of a large project fit together?"),
            YesNo("Do you enjoy organizing people, deadlines, and tasks to make sure a project gets finished on time?"),
            YesNo("Have you ever played a video game and wished you could build your own?"),
            YesNo("Are you interested in protecting a company's internet connections and servers from outside attacks?"),
            YesNo("Do you enjoy making things look visually appealing, like posters, logos, or color schemes?"),
            YesNo("Do you get frustrated when an app or website is confusing to use, and think about how it could be simpler?"),
            YesNo("Are you interested in smart gadgets, like smart watches, smart thermostats, or smart light bulbs?"),
            YesNo("Are you interested in how companies like Amazon or Google handle and store enormous amounts of data?"),
            YesNo("Are you curious how apps can automatically blur backgrounds, detect faces, or enhance photos?"),
            YesNo("Do you enjoy cleaning up messy information and reshaping it so it's more useful before analyzing it?"),
            YesNo("Do you have a knack for spotting mistakes or things that don't work right before anyone else notices?")
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
