using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartCareerPath.API.Hubs;
using SmartCareerPath.API.Middleware;
using SmartCareerPath.Application;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Application.Validator.Auth;
using SmartCareerPath.Domain.Entites.Identity;
using SmartCareerPath.Infrastructure.Persistence;
using SmartCareerPath.Infrastructure.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy
//            .AllowAnyOrigin()
//            .AllowAnyMethod()
//            .AllowAnyHeader();
//        //.AllowCredentials();
//    });
//});
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",           // ADD THIS (Vite dev server)
                "http://localhost:3000",           // keep for CRA if needed
                "https://your-frontend-domain.com" // production frontend URL
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ===============================================
// 1. DATABASE
// ===============================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ===============================================
// 2. IDENTITY (TPT)
// ===============================================
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ===============================================
// 3. JWT AUTHENTICATION
// ===============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ===============================================
// 4. APPLICATION SERVICES
// ===============================================

// -- Phase 2 ---------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<JwtTokenService>();

// -- Phase 3 ---------------------------------
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// -- Phase 4 ---------------------------------

builder.Services.AddScoped<ICareerTrackService, CareerTrackService>();
builder.Services.AddScoped<IRoadmapItemService, RoadmapItemService>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

// -- Phase 5 ---------------------------------

builder.Services.AddScoped<ISeekerService, SeekerService>();

// -- Phase 6 ---------------------------------

builder.Services.AddScoped<IMentorService, MentorService>();

// -- Phase 7 ---------------------------------

builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// -- Phase 8 ---------------------------------
builder.Services.AddScoped<IChatService, ChatService>();


//Phase 11

builder.Services.AddScoped<IChatRequestService, ChatRequestService>();
builder.Services.AddScoped<INotificationService, NotificationService>();



// Add HttpClient for the AI service
builder.Services.AddHttpClient<IAiRecommendationService, AiRecommendationService>();

// Register the service
builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();


// ===============================================
// 5. FLUENTVALIDATION
// ===============================================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(
    typeof(RegisterSeekerValidator).Assembly);

// ===============================================
// 6. CONTROLLERS + SWAGGER
// ===============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Career Path API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // NEW — group endpoints by controller area
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null) return [api.GroupName];
        var controller = api.ActionDescriptor.RouteValues["controller"];
        return [controller!];
    });

    //XML doc comments(optional)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

});




// Fluent Validation final error shape 
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value!.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors
                .Select(x => new { field = e.Key, message = x.ErrorMessage }))
            .ToList();

        return new BadRequestObjectResult(new
        {
            statusCode = 400,
            error = "Validation failed.",
            details = errors
        });
    };
});




// Serilog Logging

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
    )
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()  // requires Serilog.Enrichers.Environment
);






// Reset and Forget Password
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();


// --- Services ---
builder.Services.AddSignalR();






// Health check service registration
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: ["db", "sqlserver"]
    );


// ===============================================
// BUILD
// ===============================================
var app = builder.Build();


// ===============================================
// 7. SEED DATABASE
// ===============================================
// ===============================================
// 7. SEED DATABASE & AUTO-MIGRATE
// ===============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Get the database context
        var context = services.GetRequiredService<AppDbContext>();

        // 2. Automatically apply any pending migrations (Builds the tables in MonsterASP)
        await context.Database.MigrateAsync();

        // 3. Seed the initial data (Admin user, roles, etc.)
        await DataSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

// ===============================================
// 8. MIDDLEWARE PIPELINE (ORDER IS CRITICAL)
// ===============================================
app.UseMiddleware<GlobalExceptionMiddleware>(); // FIRST - wraps everything


app.UseCors("FrontendPolicy");


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthentication(); // validates JWT, populates User claims
app.UseAuthorization();  // enforces [Authorize] attributes

app.MapControllers();



// Health check middleware mapping
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});




app.MapHub<ChatHub>("/hubs/chat");

app.Run();

// ===============================================
// REQUIRED FOR WebApplicationFactory IN TESTS
// ===============================================
public partial class Program { }