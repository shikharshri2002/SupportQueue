using Helpdesk.Application;
using Helpdesk.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure;

public sealed class UserRepository(HelpdeskDbContext database) : IUserRepository
{
    public Task<SupportUser?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        database.Users.SingleOrDefaultAsync(
            user => user.Email == email.ToLowerInvariant(), cancellationToken);

    public Task<SupportUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        database.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<IReadOnlyList<UserSummary>> GetSupportAgentsAsync(CancellationToken cancellationToken) =>
        await database.Users.AsNoTracking()
            .Where(user => user.Role == UserRole.SupportAgent)
            .OrderBy(user => user.FullName)
            .Select(user => new UserSummary(
                user.Id, user.FullName, user.Email, "SupportAgent", user.Department))
            .ToListAsync(cancellationToken);
}
