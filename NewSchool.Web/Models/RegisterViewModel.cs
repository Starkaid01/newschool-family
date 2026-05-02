using System.ComponentModel.DataAnnotations;

namespace NewSchool.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Informe seu nome completo.")]
    [Display(Name = "Nome completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu melhor email.")]
    [EmailAddress(ErrorMessage = "Digite um email valido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Crie uma senha.")]
    [MinLength(6, ErrorMessage = "Use pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme sua senha.")]
    [Compare(nameof(Password), ErrorMessage = "As senhas nao conferem.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar senha")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Codigo de convite")]
    public string? ReferralCode { get; set; }

    public string? IntendedTrack { get; set; }

    public string? ErrorMessage { get; set; }
}
