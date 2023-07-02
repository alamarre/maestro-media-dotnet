using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maestro.Models;
public record VideoSources {
    [Key]
    public long VideoSourceId {get; set;}

    [ForeignKey("Videos")]
    public long VideoId {get; set;}

    public string? Source {get; set;}

    public VideoSourceType VideoSourceType {get; set;}


}