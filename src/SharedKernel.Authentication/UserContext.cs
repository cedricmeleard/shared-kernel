using System.Security.Claims;
using Authentication.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
        httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;

    public string Email =>
        httpContextAccessor.HttpContext?.User.GetEmail() ?? string.Empty;

    public IReadOnlyList<string> Roles =>
        httpContextAccessor.HttpContext?.User.GetRoles() ?? [];

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? principal?.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public static string GetEmail(this ClaimsPrincipal? principal) =>
        principal?.FindFirst(ClaimTypes.Email)?.Value
        ?? principal?.FindFirst("email")?.Value
        ?? string.Empty;

    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal? principal) =>
        principal?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
        ?? [];
}