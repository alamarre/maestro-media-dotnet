using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maestro.Models;
public record VideoSources {
    [Key]
    public Guid VideoSourceId {get; set;}

    [ForeignKey("VideoSourceRoots")]
    public Guid VideoSourceRootId {get; set;}

    [ForeignKey("Videos")]
    public Guid VideoId {get; set;}

    public string? Source {get; set;}

    public VideoSourceType VideoSourceType {get; set;}


}