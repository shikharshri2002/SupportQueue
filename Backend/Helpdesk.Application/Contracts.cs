using System.ComponentModel.DataAnnotations;
using Helpdesk.Domain;

namespace Helpdesk.Application;

public sealed record LoginRequest(
    [param: Required, EmailAddress, MaxLength(254)] string Email,
    [param: Required, MaxLength(128)] string Password);

public sealed record UserSummary(Guid Id, string FullName, string Email, string Role, string Department);

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, UserSummary User);

public sealed record CreateTicketRequest
{
    [Required, StringLength(120, MinimumLength = 4)]
    public required string Subject { get; init; }

    [Required, StringLength(3000, MinimumLength = 10)]
    public required string Description { get; init; }

    [Required, StringLength(40)]
    public required string Category { get; init; }

    public required TicketPriority Priority { get; init; }
}

public sealed record TicketStatusRequest
{
    public required TicketStatus Status { get; init; }
}

public sealed record AssignTicketRequest
{
    public required Guid AssigneeId { get; init; }
}

public sealed record TicketResponse(
    int Id,
    string TicketNumber,
    string Subject,
    string Description,
    string Category,
    string Priority,
    string Status,
    string RequesterName,
    string RequesterEmail,
    Guid RequesterId,
    string? AssigneeName,
    Guid? AssigneeId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ResolvedAtUtc);

public sealed record DashboardStats(
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int ResolvedTickets,
    int UrgentTickets,
    double ResolutionPercent);

public sealed record TicketFilters(string? Search, TicketStatus? Status, TicketPriority? Priority);
