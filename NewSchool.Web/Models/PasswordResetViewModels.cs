using System.ComponentModel.DataAnnotations;

namespace NewSchool.Web.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Informe o email da conta.")]
    [EmailAddress(ErrorMessage = "Digite um email valido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public string? StatusMessage { get; set; }
}

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Crie uma nova senha.")]
    [MinLength(6, ErrorMessage = "Use pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [Compare(nameof(Password), ErrorMessage = "As senhas não conferem.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
