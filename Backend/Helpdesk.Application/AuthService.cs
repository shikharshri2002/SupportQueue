namespace Helpdesk.Application;

public sealed class AuthService(
    IUserRepository users,
    IPasswordService passwords,
    ITokenIssuer tokens) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim(), cancellationToken);
        return user is not null && passwords.Verify(user, request.Password)
            ? tokens.Issue(user)
            : null;
    }
}
