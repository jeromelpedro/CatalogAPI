using System.ComponentModel.DataAnnotations;

namespace Catalog.Domain.Dto;

public class CreateGameReviewDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}