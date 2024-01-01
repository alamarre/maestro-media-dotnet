using Maestro;
using Maestro.Auth;
using Maestro.Models;
using Microsoft.EntityFrameworkCore;

namespace MaestroMediaCenter.Auth;
public interface IUserProfileProvider {
    Task<Profiles?> GetUserProfileAsync(CancellationToken cancellationToken);
}


public class UserProfileProvider(
    IHttpContextAccessor httpContextAccessor, 
    IDbContextFactory<MediaDbContext> dbContextFactory,
    IUserContextProvider userContextProvider) : IUserProfileProvider
{
    async Task<Profiles?> IUserProfileProvider.GetUserProfileAsync(CancellationToken cancellationToken)
    {
        if( httpContextAccessor.HttpContext == null || !httpContextAccessor.HttpContext.Request.Query.TryGetValue("profile", out var profileName)) {
            return null;
        }
        var user = userContextProvider.GetUserContext();
        if(user == null) {
            return null;
        }
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Profiles.FirstOrDefaultAsync(p => p.ProfileName == profileName && p.UserId == user.UserId, cancellationToken);
    }
}