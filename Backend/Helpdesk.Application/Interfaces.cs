using Helpdesk.Domain;

namespace Helpdesk.Application;

public interface IUserRepository
{
    Task<SupportUser?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<SupportUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserSummary>> GetSupportAgentsAsync(CancellationToken cancellationToken);
}

public interface ITicketRepository
{
    Task<IReadOnlyList<TicketResponse>> GetAllAsync(
        TicketFilters filters, Guid? requesterId, CancellationToken cancellationToken);
    Task<TicketResponse?> GetByIdAsync(int id, Guid? requesterId, CancellationToken cancellationToken);
    Task<TicketResponse> CreateAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(int id, TicketStatus status, CancellationToken cancellationToken);
    Task<bool> AssignAsync(int id, Guid assigneeId, CancellationToken cancellationToken);
    Task<DashboardStats> GetStatsAsync(Guid? requesterId, CancellationToken cancellationToken);
}

public interface IPasswordService
{
    string Hash(SupportUser user, string password);
    bool Verify(SupportUser user, string password);
}

public interface ITokenIssuer
{
    LoginResponse Issue(SupportUser user);
}

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

public interface ITicketService
{
    Task<IReadOnlyList<TicketResponse>> GetAllAsync(
        TicketFilters filters, Guid? requesterId, CancellationToken cancellationToken);
    Task<TicketResponse?> GetByIdAsync(int id, Guid? requesterId, CancellationToken cancellationToken);
    Task<TicketResponse> CreateAsync(
        CreateTicketRequest request, Guid requesterId, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(int id, TicketStatus status, CancellationToken cancellationToken);
    Task<bool> AssignAsync(int id, Guid assigneeId, CancellationToken cancellationToken);
    Task<DashboardStats> GetStatsAsync(Guid? requesterId, CancellationToken cancellationToken);
}
