using ERMS.Application.Common.Interfaces;
using ERMS.Domain.Constants;
using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERMS.Infrastructure.Persistence.Seeds;

public class DataSeeder
{
    private readonly ERMSDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly SeedSettings _seedSettings;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        ERMSDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IOptions<SeedSettings> seedSettings,
        ILogger<DataSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasherService = passwordHasherService;
        _seedSettings = seedSettings.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedExpenseCategoriesAsync(cancellationToken);
        await SeedSuperAdminAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Roles.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var roles = new[]
        {
            new Role
            {
                Code = RoleCodes.SuperAdmin,
                Name = "Super Admin",
                CreatedAt = now,
                CreatedBy = 0
            },
            new Role
            {
                Code = RoleCodes.Admin,
                Name = "Admin",
                CreatedAt = now,
                CreatedBy = 0
            },
            new Role
            {
                Code = RoleCodes.Employee,
                Name = "Employee",
                CreatedAt = now,
                CreatedBy = 0
            }
        };

        _dbContext.Roles.AddRange(roles);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} roles.", roles.Length);
    }

    private async Task SeedExpenseCategoriesAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.ExpenseCategories.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var categories = new[]
        {
            new ExpenseCategory
            {
                Code = ExpenseCategoryCodes.Travel,
                Name = "Travel",
                DisplayOrder = 1,
                CreatedAt = now,
                CreatedBy = 0
            },
            new ExpenseCategory
            {
                Code = ExpenseCategoryCodes.Food,
                Name = "Food",
                DisplayOrder = 2,
                CreatedAt = now,
                CreatedBy = 0
            },
            new ExpenseCategory
            {
                Code = ExpenseCategoryCodes.Hotel,
                Name = "Hotel",
                DisplayOrder = 3,
                CreatedAt = now,
                CreatedBy = 0
            },
            new ExpenseCategory
            {
                Code = ExpenseCategoryCodes.Recharge,
                Name = "Recharge",
                DisplayOrder = 4,
                CreatedAt = now,
                CreatedBy = 0
            }
        };

        _dbContext.ExpenseCategories.AddRange(categories);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} expense categories.", categories.Length);
    }

    private async Task SeedSuperAdminAsync(CancellationToken cancellationToken)
    {
        var superAdminRole = await _dbContext.Roles
            .FirstOrDefaultAsync(role => role.Code == RoleCodes.SuperAdmin, cancellationToken);

        if (superAdminRole is null)
        {
            _logger.LogWarning("Super Admin role was not found. Skipping super admin user seed.");
            return;
        }

        var normalizedEmail = _seedSettings.SuperAdmin.Email.Trim().ToLowerInvariant();
        var employeeCode = _seedSettings.SuperAdmin.EmployeeCode;

        var existingEmployee = await _dbContext.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(
                e => e.EmployeeCode == employeeCode || e.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (existingEmployee is not null)
        {
            // If the seed credentials changed, update them on the existing Super Admin
            existingEmployee.Email = normalizedEmail;
            if (existingEmployee.User != null)
            {
                existingEmployee.User.PasswordHash = _passwordHasherService.HashPassword(_seedSettings.SuperAdmin.Password);
                existingEmployee.User.IsPasswordChangeRequired = true;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var employee = new Employee
        {
            EmployeeCode = _seedSettings.SuperAdmin.EmployeeCode,
            Name = _seedSettings.SuperAdmin.Name,
            Email = normalizedEmail,
            MobileNumber = "0000000000",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = 0
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var user = new User
        {
            EmployeeId = employee.Id,
            RoleId = superAdminRole.Id,
            PasswordHash = _passwordHasherService.HashPassword(_seedSettings.SuperAdmin.Password),
            IsPasswordChangeRequired = true,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = employee.Id
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        employee.CreatedBy = user.Id;
        user.CreatedBy = user.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded Super Admin user with email {Email}.",
            normalizedEmail);
    }
}
