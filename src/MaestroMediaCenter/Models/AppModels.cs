using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Models;

public enum VideoType {
    None = 0,
    Movie = 1,
    TvShow = 2,
}


public enum VideoSourceLocationType {
    None = 0,
    LocalSource = 1,
    HttpSource = 2,
    CloudfrontSource = 3
}

public enum VideoSourceType {
    None = 0,
    Video = 1,
    Subtitle = 2,
}

public record LocalVideoChange(
    string Type, 
    string RootUrl, 
    string Path
);

[PrimaryKey(nameof(TenantId), nameof(DomainName))]
[Index(nameof(DomainName), IsUnique = true)]
public record TenantDomains
{
	public Guid TenantId { get; set; }
	public string DomainName { get; set; } = string.Empty;
}

// make it easy to delete all records that are quite dated
[Index("SoftDeleteDate")]
// cover clearing by tenant and tenant specific soft delete cleanup
[Index("TenantId", "SoftDeleteDate")]
public record TenantTable {
    public Guid? TenantId { get; set; }

    public DateTime? SoftDeleteDate { get; set; }
    public bool SoftDeleted { get; set; } = false;

    public void SoftDelete() {
        SoftDeleted = true;
        SoftDeleteDate = DateTime.UtcNow;
    }
};

public record VideoSourceRoots : TenantTable {
    [Key]
    public Guid VideoSourceRootId {get; set;}
    public required string VideoSourceRootPath {get; set;}
    public VideoSourceLocationType VideoSourceLocationType {get; set;}
}

[Index("VideoSourceRootId", "Port", IsUnique = true)]
public record VideoServerAlternates : TenantTable {
    [Key]
    public Guid VideoServerAlternateId {get; set;}
    public Guid VideoSourceRootId {get; set;}
    public int Port {get; set;}
    public required string Hostname {get; set;}
}

[Index(nameof(AddedDate), nameof(VideoType))]
[Index(nameof(VideoName), nameof(VideoType), nameof(Season), nameof(Episode), IsUnique = true)]
public record Videos : TenantTable {
    [Key]
    public Guid VideoId {get; set;}
    public required string VideoName {get; set;}

    // name of episode for instance
    public string? Subname {get; set;}
    public required VideoType VideoType {get; set;}

    // These could be replaced with a JSON object or another table in the future, but this is simpler for now
    public int? Season {get; set;}
    public int? Episode {get; set;}

    public DateTime AddedDate { get; set; }
}

public record VideoSources : TenantTable {
    [Key]
    public Guid VideoSourceId {get; set;}
    [ForeignKey(nameof(VideoSourceRoots))]
    public Guid VideoSourceRootId {get; set;}

    public VideoSourceRoots? VideoSourceRoot {get; set;}
    [ForeignKey(nameof(Videos))]
    public Guid VideoId {get; set;}
    public required string Source {get; set;}
    public VideoSourceType VideoSourceType {get; set;}
}

[PrimaryKey(nameof(ProfileId), nameof(VideoId))]
public record WatchProgress : TenantTable {
    [ForeignKey(nameof(Videos))]
    public Guid VideoId {get; set;}

    public Videos? Video {get; set;}
    [ForeignKey(nameof(Profiles))]
    public Guid ProfileId {get; set;}
    public int ProgressInSeconds {get; set;}
    public required string Status {get; set;}
    public DateTime LastWatched {get; set;}

    public DateTime Expires {get; set;}
}

public record AccountUsers : TenantTable {
    [Key]
    public Guid UserId {get; set;}
}

[Index(nameof(EmailAddress), IsUnique = true)]
public record AccountEmails : TenantTable {
    [Key]
    public Guid EmailId {get; set;}
    [ForeignKey(nameof(AccountUsers))]
    public Guid UserId {get; set;}

    public required string EmailAddress {get; set;}
}

public record AccountLogins : TenantTable {
    [Key]
    public required string Username {get; set;}

    [ForeignKey(nameof(AccountUsers))]
    public Guid UserId {get; set;}
    public required string HashedPassword {get; set;}

    public required int HashedPasswordPasses {get; set;}
}

public record Profiles : TenantTable {
    [Key]
    public Guid ProfileId {get; set;}
    [ForeignKey(nameof(AccountUsers))]
    public Guid UserId {get; set;}
    public required string ProfileName {get; set;}
    public required string ProfileImage {get; set;}
}

public record VideoCollections : TenantTable {
    [Key]
    public Guid VideoCollectionId {get; set;}
    [ForeignKey(nameof(AccountUsers))]
    public required string VideoCollectionName {get; set;}

    public DateTime StartDate {get; set;}
    public DateTime EndDate {get; set;}

    public List<VideoCollectionItems> VideoCollectionItems {get; set;} = new List<VideoCollectionItems>();
}

public record VideoCollectionItems : TenantTable {
    [Key]
    public Guid VideoCollectionItemId {get; set;}
    [ForeignKey(nameof(VideoCollections))]
    public Guid VideoCollectionId {get; set;}
    [ForeignKey(nameof(Videos))]
    public Guid VideoId {get; set;}

    public Videos? Video {get; set;}

}