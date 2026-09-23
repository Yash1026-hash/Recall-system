using System.ComponentModel.DataAnnotations;

namespace VehicleRecall.Shared.Models;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [RegularExpression(
        @"^[^@\s]+@[^@\s.]+(?:\.[^@\s.]+)+$",
        ErrorMessage = "Enter a complete email address, such as name@example.com.")]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(30, MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 30 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$",
        ErrorMessage = "Username can only contain letters, numbers, underscore and dot.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Full name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}