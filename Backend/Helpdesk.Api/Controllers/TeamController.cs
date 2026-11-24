using Helpdesk.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,SupportAgent")]
[Route("api/team")]
public sealed class TeamController(IUserRepository users) : ControllerBase
{
    [HttpGet("agents")]
    public async Task<ActionResult<IReadOnlyList<UserSummary>>> GetAgents(CancellationToken cancellationToken) =>
        Ok(await users.GetSupportAgentsAsync(cancellationToken));
}
