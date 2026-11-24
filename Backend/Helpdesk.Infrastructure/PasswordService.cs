using Helpdesk.Application;
using Helpdesk.Domain;
using Microsoft.AspNetCore.Identity;

namespace Helpdesk.Infrastructure;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<SupportUser> hasher = new();

    public string Hash(SupportUser user, string password) =>
        hasher.HashPassword(user, password);

    public bool Verify(SupportUser user, string password) =>
        hasher.VerifyHashedPassword(user, user.PasswordHash, password) is
            PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
