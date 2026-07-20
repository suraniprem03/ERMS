namespace ERMS.Infrastructure.Persistence.Seeds;

public class SeedSettings
{
    public const string SectionName = "Seed";

    public SuperAdminSeedSettings SuperAdmin { get; set; } = new();
}

public class SuperAdminSeedSettings
{
    public string EmployeeCode { get; set; } = "SA001";

    public string Name { get; set; } = "Super Admin";

    public string Email { get; set; } = "superadmin@erms.local";

    public string Password { get; set; } = "ChangeMe@123";
}
