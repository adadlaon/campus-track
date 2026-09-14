namespace API.Entities;

public class MemberRole
{
    public string MemberId { get; set; } = null!;
    public Member Member { get; set; } = null!;
    public string RoleId { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
}