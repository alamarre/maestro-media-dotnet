using Maestro;
using Maestro.Entities;

namespace MaestroMediaCenter.Auth;

public interface IUserProfileProvider
{
    Task<Profile?> GetUserProfileAsync(CancellationToken cancellationToken);
}
