using Maestro;
using Maestro.Auth;
using Maestro.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaestroMediaCenter.Auth;
public interface IUserProfileProvider {
    Task<Profile?> GetUserProfileAsync(CancellationToken cancellationToken);
}


public class UserProfileProvider(
    IHttpContextAccessor httpContextAccessor, 
    IDbContextFactory<MediaDbContext> dbContextFactory,
    IUserContextProvider userContextProvider) : IUserProfileProvider
{
    async Task<Profile?> IUserProfileProvider.GetUserProfileAsync(CancellationToken cancellationToken)
    {
        if( httpContextAccessor.HttpContext == null || !httpContextAccessor.HttpContext.Request.Query.TryGetValue("profile", out var profileName)) {
            return null;
        }
        var user = userContextProvider.GetUserContext();
        if(user == null) {
            return null;
        }
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Profile.FirstOrDefaultAsync(p => p.ProfileName == profileName && p.UserId == user.UserId, cancellationToken);
    }
}