using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Auth;

public class RegistrarUsuarioDto
{
    [Required, StringLength(100)]
    public string? Nome { get; set; }

    [Required, StringLength(200)]
    public string? Email { get; set; }

    [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter de 8 a 100 caracteres.")]
    public string? Senha { get; set; }
}
