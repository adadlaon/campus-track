namespace API.Data;

public class SeedUserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public string? ImageUrl { get; set; }
    public required string DisplayName { get; set; }
    public string? HouseNumber { get; set; }
    public string? Zone { get; set; }
    public required string Barangay { get; set; }
    public required string City { get; set; }
    public required string Province { get; set; }
    public required string PhoneNumber { get; set; }
    public DateTime Created { get; set; } 
    public DateTime LastActive { get; set; }
}