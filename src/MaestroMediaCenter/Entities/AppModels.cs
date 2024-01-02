using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Entities;

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

[PrimaryKey(nameof(TenantId), nameof(DomainName))]
[Index(nameof(DomainName), IsUnique = true)]
public record TenantDomain
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

public record VideoSourceRoot : TenantTable {
    [Key]
    public Guid VideoSourceRootId {get; set;}
    public required string VideoSourceRootPath {get; set;}
    public VideoSourceLocationType VideoSourceLocationType {get; set;}
}

[Index("VideoSourceRootId", "Port", IsUnique = true)]
public record VideoServerAlternate : TenantTable {
    [Key]
    public Guid VideoServerAlternateId {get; set;}
    public Guid VideoSourceRootId {get; set;}
    public int Port {get; set;}
    public required string Hostname {get; set;}
}

[Index(nameof(AddedDate), nameof(VideoType))]
[Index(nameof(VideoName), nameof(VideoType), nameof(Season), nameof(Episode), IsUnique = true)]
public record Video : TenantTable {
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

public record VideoSource : TenantTable {
    [Key]
    public Guid VideoSourceId {get; set;}
    [ForeignKey(nameof(Entities.VideoSourceRoot))]
    public Guid VideoSourceRootId {get; set;}

    public VideoSourceRoot? VideoSourceRoot {get; set;}
    [ForeignKey(nameof(Video))]
    public Guid VideoId {get; set;}
    public required string Source {get; set;}
    public VideoSourceType VideoSourceType {get; set;}
}

[PrimaryKey(nameof(ProfileId), nameof(VideoId))]
public record WatchProgress : TenantTable {
    [ForeignKey(nameof(Entities.Video))]
    public Guid VideoId {get; set;}

    public Video? Video {get; set;}
    [ForeignKey(nameof(Profile))]
    public Guid ProfileId {get; set;}
    public int ProgressInSeconds {get; set;}
    public required string Status {get; set;}
    public DateTime LastWatched {get; set;}

    public DateTime Expires {get; set;}
}

public record AccountUser : TenantTable {
    [Key]
    public Guid UserId {get; set;}
}

[Index(nameof(EmailAddress), IsUnique = true)]
public record AccountEmail : TenantTable {
    [Key]
    public Guid EmailId {get; set;}
    [ForeignKey(nameof(AccountUser))]
    public Guid UserId {get; set;}

    public required string EmailAddress {get; set;}
}

public record AccountLogin : TenantTable {
    [Key]
    public required string Username {get; set;}

    [ForeignKey(nameof(AccountUser))]
    public Guid UserId {get; set;}
    public required string HashedPassword {get; set;}

    public required int HashedPasswordPasses {get; set;}
}

public record Profile : TenantTable {
    [Key]
    public Guid ProfileId {get; set;}
    [ForeignKey(nameof(AccountUser))]
    public Guid UserId {get; set;}
    public required string ProfileName {get; set;}
    public string? ProfileImage {get; set;}
}

public record VideoCollection : TenantTable {
    [Key]
    public Guid VideoCollectionId {get; set;}
    [ForeignKey(nameof(AccountUser))]
    public required string VideoCollectionName {get; set;}

    public DateTime StartDate {get; set;}
    public DateTime EndDate {get; set;}

    public List<VideoCollectionItem> VideoCollectionItems {get; set;} = new List<VideoCollectionItem>();
}

public record VideoCollectionItem : TenantTable {
    [Key]
    public Guid VideoCollectionItemId {get; set;}
    [ForeignKey(nameof(VideoCollection))]
    public Guid VideoCollectionId {get; set;}
    [ForeignKey(nameof(Entities.Video))]
    public Guid VideoId {get; set;}

    public Video? Video {get; set;}

}