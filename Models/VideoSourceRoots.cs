using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maestro.Models;
public class VideoSourceRoots {
    [Key]
    public long VideoSourceRootId {get; set;}

    public string? VideoSourceRootPath {get; set;}

    public VideoSourceRootType VideoSourceRootType {get; set;}
}