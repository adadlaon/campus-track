using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateUserRequestDto
{
    [Required, EmailAddress]
    public required string Email { get; set; } = string.Empty;

    [Required, MinLength(4)]
    public required string Password { get; set; } = string.Empty;

    [Required]
    public required string DisplayName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public string[] Roles { get; set; } = [];
    public string? HouseNumber { get; set; }
    public string? Zone { get; set; }

    [Required]
    public required string Barangay { get; set; } = string.Empty;

    [Required]
    public required string City { get; set; } = string.Empty;

    [Required]
    public required string Province { get; set; } = string.Empty;

    [Required, Phone]
    public required string PhoneNumber { get; set; } = string.Empty;
}