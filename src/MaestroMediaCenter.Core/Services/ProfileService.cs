using Maestro.Auth;
using Maestro.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Services;

public class ProfileService(
    IDbContextFactory<MediaDbContext> dbContextFactory,
    IUserContextProvider userContextProvider)
{
    public async Task<List<Profile>> GetProfilesAsync(CancellationToken cancellationToken)
    {
        var user = userContextProvider.GetUserContext();
        if (user == null)
        {
            return new List<Profile>();
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Profile.Where(u => u.UserId == user.UserId).ToListAsync(cancellationToken);
    }

    public async Task CreateProfileAsync(string profileName, CancellationToken cancellationToken)
    {
        var user = userContextProvider.GetUserContext();
        if (user == null || user.UserId == null)
        {
            return;
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var profile = new Profile { ProfileName = profileName, UserId = user.UserId.Value };
        await db.Profile.AddAsync(profile, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
