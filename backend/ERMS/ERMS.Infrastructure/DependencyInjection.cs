using System.Text;
using ERMS.Application.Common.Interfaces;
using ERMS.Application.Features.Auth;
using ERMS.Infrastructure.Identity;
using ERMS.Infrastructure.Persistence;
using ERMS.Infrastructure.Persistence.Seeds;
using ERMS.Infrastructure.Services;
using ERMS.Application.Features.Employees;
using ERMS.Application.Features.States;
using ERMS.Application.Features.Areas;
using ERMS.Application.Features.ExpenseCategories;
using ERMS.Application.Features.Claims;
using ERMS.Application.Features.Reports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ERMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SeedSettings>(configuration.GetSection(SeedSettings.SectionName));

        services.AddDbContext<ERMSDbContext>(options =>
            options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    builder => builder.MigrationsAssembly(typeof(ERMSDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IStateService, StateService>();
        services.AddScoped<IAreaService, AreaService>();
        services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<DataSeeder>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                    ?? throw new InvalidOperationException("JWT settings are not configured.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
