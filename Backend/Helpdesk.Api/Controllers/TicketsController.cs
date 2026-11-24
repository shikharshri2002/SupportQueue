using System.Security.Claims;
using Helpdesk.Application;
using Helpdesk.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public sealed class TicketsController(ITicketService tickets) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        CancellationToken cancellationToken)
    {
        Guid? requesterId = IsEmployee ? CurrentUserId : null;
        var results = await tickets.GetAllAsync(
            new TicketFilters(search, status, priority), requesterId, cancellationToken);
        return Ok(results);
    }

    [HttpGet("stats")]
    [ProducesResponseType<DashboardStats>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStats>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await tickets.GetStatsAsync(IsEmployee ? CurrentUserId : null, cancellationToken);
        return Ok(stats);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(id, IsEmployee ? CurrentUserId : null, cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TicketResponse>> Create(
        CreateTicketRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Priority))
            return BadRequest(new { message = "Choose a valid priority." });

        var ticket = await tickets.CreateAsync(request, CurrentUserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,SupportAgent")]
    public async Task<IActionResult> UpdateStatus(
        int id, TicketStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status))
            return BadRequest(new { message = "Choose a valid ticket status." });

        var updated = await tickets.UpdateStatusAsync(id, request.Status, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}/assignment")]
    [Authorize(Roles = "Admin,SupportAgent")]
    public async Task<IActionResult> Assign(
        int id, AssignTicketRequest request, CancellationToken cancellationToken)
    {
        var updated = await tickets.AssignAsync(id, request.AssigneeId, cancellationToken);
        return updated ? NoContent() : NotFound(new { message = "Ticket or support agent was not found." });
    }

    private bool IsEmployee => User.IsInRole(UserRole.Employee.ToString());

    private Guid CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedAccessException("The authenticated user identifier is missing.");
}
