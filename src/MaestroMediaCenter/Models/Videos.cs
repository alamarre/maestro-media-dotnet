using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Models;

[Index(nameof(Name), IsUnique = true)]
public record Videos {
    [Key]
    public long VideoId { get; init; }

    public string? Name { get; init; }

    public VideoType VideoType { get; init; }
}