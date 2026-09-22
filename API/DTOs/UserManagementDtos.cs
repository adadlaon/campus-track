using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public sealed record UserManagementResponse(
    string Id,
    string Email,
    string DisplayName,
    string? ImageUrl,
    string[] Roles,
    string? HouseNumber,
    string? Zone,
    string? Barangay,
    string? City,
    string? Province,
    string? PhoneNumber);

public sealed record CreateUserRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required, MinLength(4)]
    public required string Password { get; init; }

    [Required]
    public required string DisplayName { get; init; }

    public string? ImageUrl { get; init; }
    public string[] Roles { get; init; } = [];
    public string? HouseNumber { get; init; }
    public string? Zone { get; init; }

    [Required]
    public required string Barangay { get; init; }

    [Required]
    public required string City { get; init; }

    [Required]
    public required string Province { get; init; }

    [Required, Phone]
    public required string PhoneNumber { get; init; }
}

public sealed record UpdateUserRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string DisplayName { get; init; }

    public string? ImageUrl { get; init; }
    public string[] Roles { get; init; } = [];
    public string? HouseNumber { get; init; }
    public string? Zone { get; init; }

    [Required]
    public required string Barangay { get; init; }

    [Required]
    public required string City { get; init; }

    [Required]
    public required string Province { get; init; }

    [Required, Phone]
    public required string PhoneNumber { get; init; }
}