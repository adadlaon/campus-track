namespace API.DTOs;
public class RoleDto
{
    public required string Id { get; set; } 
    public required string Name { get; set; } 
    public string? Description { get; set; }
}