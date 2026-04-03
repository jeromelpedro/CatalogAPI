namespace Catalog.Domain.Entity;

public class GameReview
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string GameId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}