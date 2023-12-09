namespace MaestroMediaCenter;


public interface IVideoInfo
{
    string? Path { get; }
    string? Type {get;}
}

public record MovieInfo : IVideoInfo
{
    public string? Path { get; init; }
    public string? Type { get; init; }

    public string? Name {get; init;}
}

public record TvShowInfo : IVideoInfo
{
    public string? Path { get; init; }
    public string? Type { get; init; }
    public string? ShowName { get; init; }
    public string? Season { get; init; }
    public string? Episode { get; init; }
}

public class VideoUtilities
{
    public IVideoInfo? GetVideoInfo(string path, string type, string rootUrl)
    {
        if (path.StartsWith("/"))
        {
            path = path.Substring(1);
        }

        IVideoInfo videoInfo;

        var videoType = type.ToLower();
        if (videoType == "movie")
        {
            var moviePart = path.Substring(path.LastIndexOf("/") + 1);
            videoInfo = new MovieInfo { Path = $"{rootUrl}{path}", Name = moviePart };
        }
        else if (videoType == "tv")
        {
            var splitPath = path.Split("/");
            var showName = splitPath[1];
            var season = splitPath[2];
            var episode = splitPath[3];
            videoInfo = new TvShowInfo
            {
                Path = $"{rootUrl}{path}",
                ShowName = showName,
                Season = season,
                Episode = episode
            };
        }
        else
        {
            videoInfo = null; // Return null if videoType is not recognized
        }

        return videoInfo;
    }

}