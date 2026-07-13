using System.Reflection;
using System.Text;
using BankApi.Application.Interfaces;
using BankApi.Application.Options;
using BankApi.Application.Services;
using BankApi.Authorization;
using BankApi.Domain.Constants;
using BankApi.Infrastructure.Data;
using BankApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddControllers();

builder.Services.Configure<EntraIdOptions>(
    builder.Configuration.GetSection(EntraIdOptions.SectionName)
);

builder.Services.Configure<AppJwtOptions>(
    builder.Configuration.GetSection(AppJwtOptions.SectionName)
);

var entraOptions =
    builder.Configuration.GetSection(EntraIdOptions.SectionName).Get<EntraIdOptions>()
    ?? new EntraIdOptions();

var appJwtOptions =
    builder.Configuration.GetSection(AppJwtOptions.SectionName).Get<AppJwtOptions>()
    ?? new AppJwtOptions();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserDirectoryService, UserDirectoryService>();

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "AppJwt";
        options.DefaultChallengeScheme = "AppJwt";
    })
    .AddJwtBearer(
        "AppJwt",
        options =>
        {
            options.RequireHttpsMetadata = !isDevelopment;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = appJwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = appJwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(appJwtOptions.SigningKey)
                ),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            };
        }
    )
    .AddJwtBearer(
        "EntraId",
        options =>
        {
            options.Authority = $"{entraOptions.Instance}/{entraOptions.TenantId}/v2.0";
            options.RequireHttpsMetadata = !isDevelopment;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[]
                {
                    $"https://login.microsoftonline.com/{entraOptions.TenantId}/v2.0",
                    $"https://sts.windows.net/{entraOptions.TenantId}/",
                },
                ValidateAudience = true,
                ValidAudiences = new[] { entraOptions.Audience, entraOptions.ClientId },
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            };

            options.Events = new JwtBearerEvents();
        }
    );

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AppPolicies.AccountsOwnRead,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsOwnRead))
    );

    options.AddPolicy(
        AppPolicies.AccountsOwnDeposit,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsOwnDeposit))
    );

    options.AddPolicy(
        AppPolicies.AccountsOwnWithdraw,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsOwnWithdraw))
    );

    options.AddPolicy(
        AppPolicies.TransactionsOwnRead,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.TransactionsOwnRead))
    );

    options.AddPolicy(
        AppPolicies.AccountsAllRead,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsAllRead))
    );

    options.AddPolicy(
        AppPolicies.AccountsCashDeposit,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsCashDeposit))
    );

    options.AddPolicy(
        AppPolicies.AccountsCashWithdraw,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsCashWithdraw))
    );

    options.AddPolicy(
        AppPolicies.TransactionsTransfer,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.TransactionsTransfer))
    );

    options.AddPolicy(
        AppPolicies.AccountsCreate,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsCreate))
    );

    options.AddPolicy(
        AppPolicies.AccountsUpdate,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsUpdate))
    );

    options.AddPolicy(
        AppPolicies.AccountsClose,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AccountsClose))
    );

    options.AddPolicy(
        AppPolicies.TransactionsAllRead,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.TransactionsAllRead))
    );

    options.AddPolicy(
        AppPolicies.AdminUsersManage,
        policy =>
            policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(AppPermissions.AdminUsersManage))
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
