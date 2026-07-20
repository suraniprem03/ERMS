using ERMS.Application.Common.Exceptions;
using ERMS.Application.Features.Claims;
using ERMS.Application.Features.Claims.DTOs;
using ERMS.Domain.Entities;
using ERMS.Domain.Enums;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Infrastructure.Services;

public class ClaimService : IClaimService
{
    private readonly ERMSDbContext _dbContext;

    public ClaimService(ERMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClaimDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var claim = await _dbContext.Claims
            .Include(c => c.Employee)
            .Include(c => c.ExpenseCategory)
            .Include(c => c.TravelClaim)
            .Include(c => c.FoodClaim)
            .Include(c => c.HotelClaim)
            .Include(c => c.RechargeClaim)
            .Include(c => c.Attachments)
            .Include(c => c.StatusHistory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Claim with ID {id} not found.");

        return MapToDto(claim);
    }

    public async Task<IEnumerable<ClaimDto>> GetMyClaimsAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        var claims = await _dbContext.Claims
            .Include(c => c.Employee)
            .Include(c => c.ExpenseCategory)
            .Include(c => c.TravelClaim)
            .Include(c => c.FoodClaim)
            .Include(c => c.HotelClaim)
            .Include(c => c.RechargeClaim)
            .Include(c => c.Attachments)
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return claims.Select(MapToDto);
    }

    public async Task<IEnumerable<ClaimDto>> GetAllPendingClaimsAsync(CancellationToken cancellationToken = default)
    {
        var claims = await _dbContext.Claims
            .Include(c => c.Employee)
            .Include(c => c.ExpenseCategory)
            .Include(c => c.TravelClaim)
            .Include(c => c.FoodClaim)
            .Include(c => c.HotelClaim)
            .Include(c => c.RechargeClaim)
            .Include(c => c.Attachments)
            .Where(c => c.Status == ClaimStatus.Submitted)
            .OrderBy(c => c.SubmittedAt)
            .ToListAsync(cancellationToken);

        return claims.Select(MapToDto);
    }

    public async Task<IEnumerable<ClaimDto>> GetAllClaimsAsync(CancellationToken cancellationToken = default)
    {
        var claims = await _dbContext.Claims
            .Include(c => c.Employee)
            .Include(c => c.ExpenseCategory)
            .Include(c => c.TravelClaim)
            .Include(c => c.FoodClaim)
            .Include(c => c.HotelClaim)
            .Include(c => c.RechargeClaim)
            .Include(c => c.Attachments)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return claims.Select(MapToDto);
    }

    public async Task<ClaimDto> CreateAsync(long employeeId, CreateClaimRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.ExpenseCategories.FirstOrDefaultAsync(ec => ec.Id == request.ExpenseCategoryId, cancellationToken)
            ?? throw new NotFoundException($"Expense Category with ID {request.ExpenseCategoryId} not found.");

        ValidateClaimDetails(category.Code, request);

        // Generate Claim Number
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var claimCount = await _dbContext.Claims.CountAsync(c => c.ClaimNumber.StartsWith($"CLM-{today}"), cancellationToken);
        var claimNumber = $"CLM-{today}-{(claimCount + 1):D4}";

        var claimStatus = request.IsSubmit ? ClaimStatus.Submitted : ClaimStatus.Draft;

        var claim = new Claim
        {
            ClaimNumber = claimNumber,
            EmployeeId = employeeId,
            ExpenseCategoryId = request.ExpenseCategoryId,
            Status = claimStatus,
            SubmittedAt = request.IsSubmit ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        ApplyClaimDetails(category.Code, claim, request);

        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            Status = claimStatus,
            Comment = request.IsSubmit ? "Claim submitted" : "Claim created as draft",
            CreatedBy = employeeId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        _dbContext.Claims.Add(claim);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(claim.Id, cancellationToken);
    }

    public async Task<ClaimDto> UpdateDraftAsync(long id, long employeeId, UpdateClaimRequest request, CancellationToken cancellationToken = default)
    {
        var claim = await _dbContext.Claims
            .Include(c => c.ExpenseCategory)
            .Include(c => c.TravelClaim)
            .Include(c => c.FoodClaim)
            .Include(c => c.HotelClaim)
            .Include(c => c.RechargeClaim)
            .Include(c => c.Attachments)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmployeeId == employeeId, cancellationToken)
            ?? throw new NotFoundException($"Claim with ID {id} not found.");

        if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.Returned)
        {
            throw new ValidationException("Only Draft or Returned claims can be updated.");
        }

        var category = await _dbContext.ExpenseCategories.FirstOrDefaultAsync(ec => ec.Id == request.ExpenseCategoryId, cancellationToken)
            ?? throw new NotFoundException($"Expense Category with ID {request.ExpenseCategoryId} not found.");

        ValidateClaimDetails(category.Code, request);

        // Remove old details if category changed, or simply replace them
        claim.ExpenseCategoryId = request.ExpenseCategoryId;
        claim.ExpenseCategory = category;
        
        if (claim.TravelClaim != null) _dbContext.TravelClaims.Remove(claim.TravelClaim);
        if (claim.FoodClaim != null) _dbContext.FoodClaims.Remove(claim.FoodClaim);
        if (claim.HotelClaim != null) _dbContext.HotelClaims.Remove(claim.HotelClaim);
        if (claim.RechargeClaim != null) _dbContext.RechargeClaims.Remove(claim.RechargeClaim);

        ApplyClaimDetails(category.Code, claim, request);

        if (request.IsSubmit)
        {
            claim.Status = ClaimStatus.Submitted;
            claim.SubmittedAt = DateTimeOffset.UtcNow;
            claim.StatusHistory.Add(new ClaimStatusHistory
            {
                Status = ClaimStatus.Submitted,
                Comment = "Claim updated and submitted",
                CreatedBy = employeeId,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        claim.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(claim.Id, cancellationToken);
    }

    public async Task<ClaimDto> SubmitClaimAsync(long id, long employeeId, CancellationToken cancellationToken = default)
    {
        var claim = await _dbContext.Claims
            .FirstOrDefaultAsync(c => c.Id == id && c.EmployeeId == employeeId, cancellationToken)
            ?? throw new NotFoundException($"Claim with ID {id} not found.");

        if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.Returned)
        {
            throw new ValidationException("Only Draft or Returned claims can be submitted.");
        }

        claim.Status = ClaimStatus.Submitted;
        claim.SubmittedAt = DateTimeOffset.UtcNow;
        claim.UpdatedAt = DateTimeOffset.UtcNow;

        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            Status = ClaimStatus.Submitted,
            Comment = "Claim submitted",
            CreatedBy = employeeId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(claim.Id, cancellationToken);
    }

    public async Task<ClaimDto> ProcessAdminActionAsync(long id, long adminUserId, AdminActionRequest request, CancellationToken cancellationToken = default)
    {
        var claim = await _dbContext.Claims
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Claim with ID {id} not found.");

        if (claim.Status != ClaimStatus.Submitted)
        {
            throw new ValidationException("Only Submitted claims can be processed.");
        }

        if (request.Action != ClaimStatus.Approved && request.Action != ClaimStatus.Rejected && request.Action != ClaimStatus.Returned)
        {
            throw new ValidationException("Invalid admin action.");
        }

        claim.Status = request.Action;
        claim.AdminRemarks = request.Remarks;
        claim.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.Action == ClaimStatus.Approved)
        {
            claim.ApprovedAt = DateTimeOffset.UtcNow;
            claim.ApprovedByUserId = adminUserId;
        }

        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            Status = request.Action,
            Comment = request.Remarks,
            CreatedBy = adminUserId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(claim.Id, cancellationToken);
    }

    private void ValidateClaimDetails(string categoryCode, CreateClaimRequest request)
    {
        var upperCode = categoryCode.ToUpper();
        if (upperCode == "TRAVEL" && request.TravelDetails == null)
            throw new ValidationException("Travel details are required for TRAVEL category.");
        if (upperCode == "FOOD" && request.FoodDetails == null)
            throw new ValidationException("Food details are required for FOOD category.");
        if (upperCode == "HOTEL" && request.HotelDetails == null)
            throw new ValidationException("Hotel details are required for HOTEL category.");
        if (upperCode == "RECHARGE" && request.RechargeDetails == null)
            throw new ValidationException("Recharge details are required for RECHARGE category.");
    }

    private void ApplyClaimDetails(string categoryCode, Claim claim, CreateClaimRequest request)
    {
        var upperCode = categoryCode.ToUpper();
        
        claim.TravelClaim = null;
        claim.FoodClaim = null;
        claim.HotelClaim = null;
        claim.RechargeClaim = null;

        if (upperCode == "TRAVEL" && request.TravelDetails != null)
        {
            claim.TravelClaim = new TravelClaim
            {
                TravelDate = request.TravelDetails.TravelDate,
                Day = request.TravelDetails.Day,
                FromLocation = request.TravelDetails.FromLocation,
                ToLocation = request.TravelDetails.ToLocation,
                TotalKm = request.TravelDetails.TotalKm,
                RatePerKm = request.TravelDetails.RatePerKm,
                CalculatedAmount = request.TravelDetails.TotalKm * request.TravelDetails.RatePerKm,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
        else if (upperCode == "FOOD" && request.FoodDetails != null)
        {
            claim.FoodClaim = new FoodClaim
            {
                Amount = request.FoodDetails.Amount,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
        else if (upperCode == "HOTEL" && request.HotelDetails != null)
        {
            claim.HotelClaim = new HotelClaim
            {
                CheckInDate = request.HotelDetails.CheckInDate,
                CheckOutDate = request.HotelDetails.CheckOutDate,
                Amount = request.HotelDetails.Amount,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
        else if (upperCode == "RECHARGE" && request.RechargeDetails != null)
        {
            claim.RechargeClaim = new RechargeClaim
            {
                Amount = request.RechargeDetails.Amount,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
    }

    private static ClaimDto MapToDto(Claim claim)
    {
        return new ClaimDto
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            EmployeeId = claim.EmployeeId,
            EmployeeName = claim.Employee?.Name ?? string.Empty,
            ExpenseCategoryId = claim.ExpenseCategoryId,
            ExpenseCategoryName = claim.ExpenseCategory?.Name ?? string.Empty,
            StatusId = (int)claim.Status,
            StatusName = claim.Status.ToString(),
            AdminRemarks = claim.AdminRemarks,
            SubmittedAt = claim.SubmittedAt,
            ApprovedAt = claim.ApprovedAt,
            ApprovedByUserId = claim.ApprovedByUserId,
            // To fetch ApprovedByUserName, we would need to include the User table. For now leaving it out or handle later.

            TravelDetails = claim.TravelClaim == null ? null : new TravelClaimDto
            {
                Id = claim.TravelClaim.Id,
                TravelDate = claim.TravelClaim.TravelDate,
                Day = claim.TravelClaim.Day,
                FromLocation = claim.TravelClaim.FromLocation,
                ToLocation = claim.TravelClaim.ToLocation,
                TotalKm = claim.TravelClaim.TotalKm,
                RatePerKm = claim.TravelClaim.RatePerKm,
                CalculatedAmount = claim.TravelClaim.CalculatedAmount
            },
            FoodDetails = claim.FoodClaim == null ? null : new FoodClaimDto
            {
                Id = claim.FoodClaim.Id,
                Amount = claim.FoodClaim.Amount
            },
            HotelDetails = claim.HotelClaim == null ? null : new HotelClaimDto
            {
                Id = claim.HotelClaim.Id,
                CheckInDate = claim.HotelClaim.CheckInDate,
                CheckOutDate = claim.HotelClaim.CheckOutDate,
                Amount = claim.HotelClaim.Amount
            },
            RechargeDetails = claim.RechargeClaim == null ? null : new RechargeClaimDto
            {
                Id = claim.RechargeClaim.Id,
                Amount = claim.RechargeClaim.Amount
            },
            Attachments = claim.Attachments.Select(a => new ClaimAttachmentDto
            {
                Id = a.Id,
                ClaimId = a.ClaimId,
                FileName = a.FileName,
                OriginalFileName = a.OriginalFileName,
                FileExtension = a.FileExtension,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                CreatedAt = a.CreatedAt
            }).ToList(),
            StatusHistory = claim.StatusHistory.Select(sh => new ClaimStatusHistoryDto
            {
                Id = sh.Id,
                StatusId = (int)sh.Status,
                StatusName = sh.Status.ToString(),
                Remarks = sh.Comment,
                CreatedAt = sh.CreatedAt,
                CreatedBy = sh.CreatedBy
            }).ToList()
        };
    }
}
