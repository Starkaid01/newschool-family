using System.ComponentModel.DataAnnotations;

namespace NewSchool.Web.Models;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
