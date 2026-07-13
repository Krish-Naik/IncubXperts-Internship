using LOS.Domain.Entities;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Admin;

public class BranchPolicyService(LOSDbContext db)
{
    public async Task<BranchPolicy> UpsertAsync(
        Guid branchId,
        decimal minRate,
        decimal maxRate,
        decimal threshold,
        CancellationToken ct
    )
    {
        var policy = await db.BranchPolicies.FirstOrDefaultAsync(p => p.BranchId == branchId, ct);
        if (policy is null)
        {
            policy = new BranchPolicy { Id = Guid.NewGuid(), BranchId = branchId };
            db.BranchPolicies.Add(policy);
        }
        policy.MinInterestRate = minRate;
        policy.MaxInterestRate = maxRate;
        policy.HighValueThreshold = threshold;
        await db.SaveChangesAsync(ct);
        return policy;
    }
}
