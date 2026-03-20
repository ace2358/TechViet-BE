namespace techviet_be.Entity;

public class Review
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public Product Product { get; set; } = null!;

    public User User { get; set; } = null!;
}
