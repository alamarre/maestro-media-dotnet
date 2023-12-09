using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Models;

[Index(
    nameof(VideoSourceRootType), 
    nameof(VideoSourceRootPath), 
    IsUnique = true)]
public record VideoSourceRoots {
    [Key]
    public Guid VideoSourceRootId {get; set;}

    public string? VideoSourceRootPath {get; set;}

    public VideoSourceRootType VideoSourceRootType {get; set;}
}