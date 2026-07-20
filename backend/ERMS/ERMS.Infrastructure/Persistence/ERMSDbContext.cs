using ERMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Infrastructure.Persistence;

public class ERMSDbContext : DbContext
{
    public ERMSDbContext(DbContextOptions<ERMSDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<State> States => Set<State>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<EmployeeArea> EmployeeAreas => Set<EmployeeArea>();

    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();

    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<TravelClaim> TravelClaims => Set<TravelClaim>();
    public DbSet<FoodClaim> FoodClaims => Set<FoodClaim>();
    public DbSet<HotelClaim> HotelClaims => Set<HotelClaim>();
    public DbSet<RechargeClaim> RechargeClaims => Set<RechargeClaim>();

    public DbSet<ClaimAttachment> ClaimAttachments => Set<ClaimAttachment>();
    public DbSet<ClaimStatusHistory> ClaimStatusHistories => Set<ClaimStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ERMSDbContext).Assembly);
    }
}
