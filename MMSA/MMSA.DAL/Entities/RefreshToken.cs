namespace MMSA.DAL.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public string TokenHash { get; set; } = null!;
    public Guid FamilyId { get; set; }
    public string? SecurityStamp { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
}