using Maestro.Entities;

namespace Maestro.Cache;

public interface IVideoCacheManager {
    Task AddMovieAsync(string name, string source);

    Task DeleteMovieAsync(string name, string source);

    Task AddEpisodeAsync(string showName, string season, string episode, string source);

    Task AddMovieAsync(string showName, string season, string episode, string source);
}