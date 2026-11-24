using Helpdesk.Domain;

namespace Helpdesk.Application;

public sealed class TicketService(ITicketRepository tickets) : ITicketService
{
    public Task<IReadOnlyList<TicketResponse>> GetAllAsync(
        TicketFilters filters, Guid? requesterId, CancellationToken cancellationToken) =>
        tickets.GetAllAsync(filters, requesterId, cancellationToken);

    public Task<TicketResponse?> GetByIdAsync(int id, Guid? requesterId, CancellationToken cancellationToken) =>
        tickets.GetByIdAsync(id, requesterId, cancellationToken);

    public Task<TicketResponse> CreateAsync(
        CreateTicketRequest request, Guid requesterId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var ticket = new Ticket
        {
            TicketNumber = $"SQ-{now:yyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
            Subject = request.Subject.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            Priority = request.Priority,
            CreatedById = requesterId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        return tickets.CreateAsync(ticket, cancellationToken);
    }

    public Task<bool> UpdateStatusAsync(int id, TicketStatus status, CancellationToken cancellationToken) =>
        tickets.UpdateStatusAsync(id, status, cancellationToken);

    public Task<bool> AssignAsync(int id, Guid assigneeId, CancellationToken cancellationToken) =>
        tickets.AssignAsync(id, assigneeId, cancellationToken);

    public Task<DashboardStats> GetStatsAsync(Guid? requesterId, CancellationToken cancellationToken) =>
        tickets.GetStatsAsync(requesterId, cancellationToken);
}
