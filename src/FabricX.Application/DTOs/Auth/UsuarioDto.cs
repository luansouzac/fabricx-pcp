using FabricX.Domain.Entities;
using FabricX.Domain.Enums;

namespace FabricX.Application.DTOs.Auth;

public class UsuarioDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public PerfilUsuario Perfil { get; init; }
    public bool Ativo { get; init; }

    public static UsuarioDto De(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Perfil = usuario.Perfil,
        Ativo = usuario.Ativo
    };
}
