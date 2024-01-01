using System.Text.RegularExpressions;
using Maestro.Models;

namespace Maestro.Utilities;


public interface IVideoInfo
{
    string? Path { get; }
    string? TypeName {get;}

    string Name { get; }

    string? Subname {get;}

    VideoType VideoType { get; }
}

public record MovieInfo : IVideoInfo
{
    public string? Path { get; init; }
    public string? TypeName => "movie";

    public VideoType VideoType => VideoType.Movie;

    public required string Name {get; init;}

    public string? Subname => null;
}

public record TvShowInfo : IVideoInfo
{
    public VideoType VideoType => VideoType.TvShow;
    public string? Path { get; init; }
    public string? TypeName => "tv";
    public required string ShowName { get; init; }

    public string Name => ShowName;

    public string? Subname => EpisodeName;
    public required string EpisodeName { get; init; }
    public int? Season { get; init; }
    public int? Episode { get; init; }
}

public class VideoUtilities
{

    private static readonly Regex pattern1 = new Regex(@"(S[0-9]{2})?\s*EP?([0-9]{2})", RegexOptions.IgnoreCase);
    private static readonly Regex pattern2 = new Regex(@"[0-9]{1,2}x([0-9]{2})", RegexOptions.IgnoreCase);
    private static readonly Regex pattern3 = new Regex(@"(\s|[.])[1-9]([0-9]{2})(\s|[.])", RegexOptions.IgnoreCase);
    private static readonly Regex yearPattern = new Regex(@"(.*) [(]?([0-9]{4})[)]?$");

    public IVideoInfo? GetVideoInfo(string internalPath) {
        if(internalPath.StartsWith("/")) {
            internalPath = internalPath.Substring(1);
        }

        var splitPath = internalPath.Split("/");
        var category = splitPath[0];
        if(category == "Movies") {
            return new MovieInfo { Path = internalPath, Name = Path.GetFileNameWithoutExtension(splitPath[1]) };
        }

        if(category == "TV") {
            var showName = splitPath[1];
            var season = GetSeasonNumber(splitPath[2]);
            var episode = GetEpisodeNumber(splitPath[3]);
            return new TvShowInfo {
                Path = internalPath,
                ShowName = showName,
                Season = season,
                Episode = episode,
                EpisodeName = GetEpisodeName(splitPath[3])
            };
        }

        return null;
    }

    public IVideoInfo? GetVideoInfo(string path, string type, string rootUrl)
    {
        if (path.StartsWith("/"))
        {
            path = path.Substring(1);
        }

        var videoType = type.ToLower();
        if (videoType == "movie")
        {
            var moviePart = path.Substring(path.LastIndexOf("/") + 1);
            var movieName = Path.GetFileNameWithoutExtension(moviePart);
            return new MovieInfo { Path = $"{rootUrl}{path}", Name = movieName };
        }

        if (videoType == "tv")
        {
            var splitPath = path.Split("/");
            var showName = splitPath[1];
            var season = GetSeasonNumber(splitPath[2]);
            var episode = GetEpisodeNumber(splitPath[3]);
            return new TvShowInfo
            {
                Path = $"{rootUrl}{path}",
                ShowName = showName,
                Season = season,
                Episode = episode,
                EpisodeName = GetEpisodeName(splitPath[3])
            };
        }

        return null;
    }

    public string GetEpisodeName(string episode)
    {
        return Path.GetFileNameWithoutExtension(episode);
    }

    private int? GetSeasonNumber(string season)
    {
        // use a regex to get the season number assuming it ends with a number
        var match = Regex.Match(season, @"\d+$");
        if (match.Success && match.Groups.Count == 1)
        {
            return int.Parse(match.Groups[0].Value);
        }
        return null;
    }

    public static int? GetEpisodeNumber(string episode)
    {
        string? episodeNumber = null;
        Match result = pattern1.Match(episode);
        
        if (!result.Success || result.Groups.Count != 3)
        {
            result = pattern2.Match(episode);
            if (!result.Success || result.Groups.Count != 2)
            {
                result = pattern3.Match(episode);
                if (!result.Success || result.Groups.Count != 4)
                {
                    return null;
                }
                else
                {
                    episodeNumber = result.Groups[2].Value;
                }
            }
            else
            {
                episodeNumber = result.Groups[1].Value;
            }
        }
        else
        {
            episodeNumber = result.Groups[2].Value;
        }
        if(int.TryParse(episodeNumber, out int episodeNumberInt))
        {
            return episodeNumberInt;
        }
        return null;
    }

    public static long? ParseYear(string input)
    {
        Match match = yearPattern.Match(input);
        if (match.Success && match.Groups.Count == 3 
            && long.TryParse(match.Groups[2].Value, out long year))
        {
            return year;
        }
        return null;
    }

}