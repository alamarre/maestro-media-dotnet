namespace Maestro.Components.Shared.Utilities;

public interface ITokenProvider
{
    Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);
}
