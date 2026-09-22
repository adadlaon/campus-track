namespace API.DTOs;

public class UserManagementResponseDto
{
    public required string Id { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string DisplayName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string[] Roles { get; set; } = [];
    public string? HouseNumber { get; set; }
    public string? Zone { get; set; }
    public string? Barangay { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PhoneNumber { get; set; }
}