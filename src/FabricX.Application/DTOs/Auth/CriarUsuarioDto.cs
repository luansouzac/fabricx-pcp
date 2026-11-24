using System.ComponentModel.DataAnnotations;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Auth;

public class CriarUsuarioDto
{
    [Required, StringLength(100)]
    public string? Nome { get; set; }

    [Required, StringLength(200)]
    public string? Email { get; set; }

    [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter de 8 a 100 caracteres.")]
    public string? Senha { get; set; }

    [EnumDataType(typeof(PerfilUsuario))]
    public PerfilUsuario Perfil { get; set; }
}
