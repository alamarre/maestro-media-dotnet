using Maestro.Entities;
using Maestro.Services;

namespace Maestro.Controllers;

public class ProfileController(ProfileService profileService) :IController {
    public async Task<IResult> GetProfilesAsync(CancellationToken cancellationToken) {
        var profiles = await profileService.GetProfilesAsync(cancellationToken);
        return Results.Ok(profiles);
    }

    public async Task<IResult> CreateProfileAsync(string profileName, CancellationToken cancellationToken) {
        await profileService.CreateProfileAsync(profileName, cancellationToken);
        return Results.Ok();
    }

    void IController.MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1.0/profiles", GetProfilesAsync);
        routes.MapPost("/api/v1.0/profiles", CreateProfileAsync);
    }
}