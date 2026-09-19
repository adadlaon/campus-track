using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities;

public class Member
{
    public string Id { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public required string DisplayName { get; set; }
    public string? HouseNumber { get; set; }
    public string? Zone { get; set; }
    public required string Barangay { get; set; }
    public required string City { get; set; }
    public required string Province { get; set; }
    public required string PhoneNumber { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    [JsonIgnore]
    public List<MemberRole> MemberRoles { get; set; } = [];

    [JsonIgnore]
    [ForeignKey(nameof(Id))]
    public AppUser User { get; set; } = null!;
}