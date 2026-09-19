using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    [Required]
    public string RoleId { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(4)]
    public string Password { get; set; } = string.Empty;
    public string? HouseNumber { get; set; }
    public string? Zone { get; set; }
    [Required]
    public string Barangay { get; set; } = string.Empty;
    [Required]
    public required string City { get; set; } = string.Empty;
    [Required]
    public required string Province { get; set; } = string.Empty;
    [Required]
    [Phone]
    public required string PhoneNumber { get; set; } = string.Empty;
}