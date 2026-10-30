using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MMSA.Authorization;
using MMSA.BLL.Options;
using MMSA.BLL.Services.Implementation;
using MMSA.BLL.Services.Implementation.Auth;
using MMSA.BLL.Services.Interfaces;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Implementations;
using MMSA.DAL.Repositories.Interfaces;
using MMSA.RateLimiting;
using MMSA.Seed;
using MMSA.Swagger;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AlgorithmDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AlgorithmDataContext>()
    .AddSignInManager();

builder.Services.AddOptions<AuthTokenOptions>().Bind(builder.Configuration.GetSection(AuthTokenOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is required")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is required")
    .Validate(o => Encoding.UTF8.GetBytes(o.SigningKey).Length >= 32, "Jwt:SigningKey must be at least 32 bytes")
    .Validate(o => o.RefreshTokenDays > 0, "Jwt:RefreshTokenDays must be positive")
    .ValidateOnStart();

var jwtOptions = builder.Configuration
    .GetSection(AuthTokenOptions.SectionName)
    .Get<AuthTokenOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = ClaimType.Name,
            RoleClaimType = ClaimType.Role
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(Policies.CanEditContent, policy =>
        policy.RequireRole(Roles.Admin, Roles.Teacher))
    .AddPolicy(Policies.CanManageUsers, policy =>
        policy.RequireRole(Roles.Admin));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    RateLimitPartition.GetTokenBucketLimiter(
        partitionKey: RateLimitPolicies.GetClientKey(httpContext),
        factory: _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            TokensPerPeriod = 50,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));

    options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: RateLimitPolicies.GetClientKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                
            }));

    options.OnRejected = (context, _) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }
        return ValueTask.CompletedTask;
    };
});

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddSingleton<IAuthorizationHandler, CourseOwnerHandler>();
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>();
builder.Services.AddScoped<IProgrammingTopicRepository, ProgrammingTopicRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IProgrammingLanguageRepository, ProgrammingLanguageRepository>();
builder.Services.AddScoped<IProgrammingLanguageService, ProgrammingLanguageService>();
builder.Services.AddScoped<IPageContentService, PageContentService>();
builder.Services.AddScoped<ISubPageRepository, SubPageRepository>();
builder.Services.AddScoped<ISubPageService, SubPageService>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MMSA API",
        Version = "v1"
    });

    options.AddSecurityDefinition(AuthorizeOperationFilter.SchemeId, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the access token from /api/Auth/login (without the 'Bearer ' prefix)."
    });

    options.OperationFilter<AuthorizeOperationFilter>();
});
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.UseMemberCasing();
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

app.UseRouting();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();    
app.UseAuthorization();
app.MapControllers();

app.Run();