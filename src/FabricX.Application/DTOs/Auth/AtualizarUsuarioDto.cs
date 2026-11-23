using System.ComponentModel.DataAnnotations;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Auth;

public class AtualizarUsuarioDto
{
    [Required, StringLength(100)]
    public string? Nome { get; set; }

    [EnumDataType(typeof(PerfilUsuario))]
    public PerfilUsuario Perfil { get; set; }
}
