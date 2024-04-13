namespace Maestro.Auth;

public record UserContext(
    Guid? TenantId,
    Guid? UserId,
    bool IsAuthenticated,
    bool IsAdmin,
    bool IsGlobalAdmin);
