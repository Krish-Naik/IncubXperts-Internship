using System.IdentityModel.Tokens.Jwt;
using System.Text;
using LOS.Api.Middleware;
using LOS.Application.Admin;
using LOS.Application.Applications;
using LOS.Application.Approvals;
using LOS.Application.Auth;
using LOS.Application.CreditScore;
using LOS.Application.Disbursement;
using LOS.Application.Email;
using LOS.Application.Kyc;
using LOS.Application.Options;
using LOS.Application.Reporting;
using LOS.Application.Storage;
using LOS.Application.Users;
using LOS.Domain.Constants;
using LOS.Infrastructure.Email;
using LOS.Infrastructure.Identity;
using LOS.Infrastructure.Persistence;
using LOS.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SecurityOptions>(
    builder.Configuration.GetSection(SecurityOptions.SectionName)
);
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.SectionName));
builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName)
);
builder.Services.Configure<StorageOptions>(
    builder.Configuration.GetSection(StorageOptions.SectionName)
);
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName)
);

var jwtOptions =
    builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var corsOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

if (isDevelopment)
{
    corsOrigins.Add("http://localhost:4200");
}

if (corsOrigins.Count == 0)
{
    corsOrigins.Add("http://localhost:4200");
}

builder.Services.AddDbContext<LOSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddHttpClient();

// Both storage providers are always registered so that documents already saved under
// one provider stay readable even after "Storage:Provider" is switched to the other
// (see IFileStorageResolver, used by KycService for reads). The provider named by
// configuration is what NEW uploads use.
builder.Services.AddScoped<LocalDiskFileStorageService>();
builder.Services.AddScoped<SupabaseStorageFileStorageService>();
builder.Services.AddScoped<IFileStorageResolver, FileStorageResolver>();
builder.Services.AddScoped<IFileStorageService>(sp =>
{
    var provider = builder.Configuration["Storage:Provider"] ?? "LocalDisk";
    return sp.GetRequiredService<IFileStorageResolver>().Resolve(provider);
});

builder.Services.AddScoped<SendGridEmailService>();
builder.Services.AddScoped<FakeSmtpEmailService>();

builder.Services.AddScoped<IEmailService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var environment = sp.GetRequiredService<IHostEnvironment>();
    var logger = sp.GetRequiredService<ILogger<ResilientEmailService>>();

    var provider = configuration["Email:Provider"]?.Trim();

    if (string.IsNullOrWhiteSpace(provider))
    {
        throw new InvalidOperationException(
            "Email:Provider is missing. Configure it as 'Mailtrap' for development "
                + "or 'SendGrid' for production."
        );
    }

    IEmailService sender = provider.ToLowerInvariant() switch
    {
        "sendgrid" => sp.GetRequiredService<SendGridEmailService>(),
        "mailtrap" or "fakesmtp" => sp.GetRequiredService<FakeSmtpEmailService>(),
        _ => throw new InvalidOperationException(
            $"Unsupported Email:Provider value '{provider}'. "
                + "Supported values are Mailtrap and SendGrid."
        ),
    };

    if (
        environment.IsProduction()
        && !provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase)
    )
    {
        throw new InvalidOperationException(
            "Production email delivery must use Email:Provider=SendGrid."
        );
    }

    return new ResilientEmailService(sender, logger);
});

builder.Services.AddScoped<LoanApplicationService>();
builder.Services.AddScoped<KycService>();
builder.Services.AddScoped<ICreditScoreProvider, CreditScoreService>();
builder.Services.AddScoped<LoanApprovalService>();
builder.Services.AddScoped<DisbursementService>();
builder.Services.AddScoped<LOS.Application.Brokers.BrokerService>();
builder.Services.AddScoped<BrandingService>();
builder.Services.AddScoped<BranchPolicyService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ReportExportService>();
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
        options.JsonSerializerOptions.NumberHandling = System
            .Text
            .Json
            .Serialization
            .JsonNumberHandling
            .AllowReadingFromString;
        options.JsonSerializerOptions.ReferenceHandler = System
            .Text
            .Json
            .Serialization
            .ReferenceHandler
            .IgnoreCycles;
    });
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context
            .ModelState.Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        var firstMessage =
            errors.Values.SelectMany(m => m).FirstOrDefault() ?? "The request was invalid.";
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            new { message = firstMessage, errors }
        );
    };
});
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
            policy
                .WithOrigins(corsOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
    );
});

builder.Services.AddAntiforgery();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = isDevelopment ? SameSiteMode.Strict : SameSiteMode.None;
    options.Secure = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !isDevelopment;
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)
            ),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = JwtRegisteredClaimNames.Sub,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue("los_access_token", out var token))
                    ctx.Token = token;
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.SystemAdministrator));
    options.AddPolicy(
        "VerificationOfficer",
        policy => policy.RequireRole(AppRoles.VerificationOfficer, AppRoles.SystemAdministrator)
    );
    options.AddPolicy(
        "BranchManager",
        policy => policy.RequireRole(AppRoles.BranchManager, AppRoles.SystemAdministrator)
    );
    options.AddPolicy(
        "Customer",
        policy => policy.RequireRole(AppRoles.Customer, AppRoles.SystemAdministrator)
    );
    options.AddPolicy(
        "BrokerAgent",
        policy => policy.RequireRole(AppRoles.BrokerAgent, AppRoles.SystemAdministrator)
    );
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (isDevelopment)
{
    app.MapOpenApi();
}

app.UseCookiePolicy();
app.UseRouting();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
