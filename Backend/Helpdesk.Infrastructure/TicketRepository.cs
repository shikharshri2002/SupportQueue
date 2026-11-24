using System.Data;
using Dapper;
using Helpdesk.Application;
using Helpdesk.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure;

public sealed class TicketRepository(HelpdeskDbContext database) : ITicketRepository
{
    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(
        TicketFilters filters, Guid? requesterId, CancellationToken cancellationToken)
    {
        var query = database.Tickets.AsNoTracking()
            .Include(ticket => ticket.CreatedBy)
            .Include(ticket => ticket.AssignedTo)
            .AsQueryable();

        if (requesterId.HasValue)
            query = query.Where(ticket => ticket.CreatedById == requesterId.Value);

        if (filters.Status.HasValue)
            query = query.Where(ticket => ticket.Status == filters.Status.Value);

        if (filters.Priority.HasValue)
            query = query.Where(ticket => ticket.Priority == filters.Priority.Value);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.Trim();
            query = query.Where(ticket =>
                ticket.Subject.Contains(search) ||
                ticket.TicketNumber.Contains(search) ||
                ticket.Category.Contains(search) ||
                ticket.CreatedBy.FullName.Contains(search));
        }

        var results = await query.OrderByDescending(ticket => ticket.CreatedAtUtc)
            .Take(250)
            .ToListAsync(cancellationToken);
        return results.Select(Map).ToList();
    }

    public async Task<TicketResponse?> GetByIdAsync(
        int id, Guid? requesterId, CancellationToken cancellationToken)
    {
        var query = database.Tickets.AsNoTracking()
            .Include(ticket => ticket.CreatedBy)
            .Include(ticket => ticket.AssignedTo)
            .Where(ticket => ticket.Id == id);
        if (requesterId.HasValue)
            query = query.Where(ticket => ticket.CreatedById == requesterId.Value);

        var ticket = await query.SingleOrDefaultAsync(cancellationToken);
        return ticket is null ? null : Map(ticket);
    }

    public async Task<TicketResponse> CreateAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        database.Tickets.Add(ticket);
        await database.SaveChangesAsync(cancellationToken);
        await database.Entry(ticket).Reference(item => item.CreatedBy).LoadAsync(cancellationToken);
        return Map(ticket);
    }

    public async Task<bool> UpdateStatusAsync(
        int id, TicketStatus status, CancellationToken cancellationToken)
    {
        var ticket = await database.Tickets.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (ticket is null)
            return false;

        ticket.Status = status;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        ticket.ResolvedAtUtc = status == TicketStatus.Resolved ? ticket.UpdatedAtUtc : null;
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignAsync(int id, Guid assigneeId, CancellationToken cancellationToken)
    {
        var assigneeExists = await database.Users.AnyAsync(
            user => user.Id == assigneeId && user.Role == UserRole.SupportAgent,
            cancellationToken);
        if (!assigneeExists)
            return false;

        var ticket = await database.Tickets.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (ticket is null)
            return false;

        ticket.AssignedToId = assigneeId;
        ticket.UpdatedAtUtc = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DashboardStats> GetStatsAsync(Guid? requesterId, CancellationToken cancellationToken)
    {
        var connection = database.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            return await connection.QuerySingleAsync<DashboardStats>(
                new CommandDefinition(
                    "dbo.GetTicketDashboardStats",
                    new { UserId = requesterId, EmployeeOnly = requesterId.HasValue },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static TicketResponse Map(Ticket ticket) => new(
        ticket.Id,
        ticket.TicketNumber,
        ticket.Subject,
        ticket.Description,
        ticket.Category,
        ticket.Priority.ToString(),
        ticket.Status.ToString(),
        ticket.CreatedBy.FullName,
        ticket.CreatedBy.Email,
        ticket.CreatedById,
        ticket.AssignedTo?.FullName,
        ticket.AssignedToId,
        ticket.CreatedAtUtc,
        ticket.UpdatedAtUtc,
        ticket.ResolvedAtUtc);
}
