using System.Text.Json.Serialization;

namespace API.Entities;

public class ApplicationRole
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    // Navigation property
    [JsonIgnore]
    public List<MemberRole> MemberRoles { get; set; } = [];
}