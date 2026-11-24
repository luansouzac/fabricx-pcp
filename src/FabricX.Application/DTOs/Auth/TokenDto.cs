namespace FabricX.Application.DTOs.Auth;

public class TokenDto
{
    public string Token { get; init; } = string.Empty;
    public string Tipo { get; init; } = "Bearer";
    public DateTime ExpiraEmUtc { get; init; }
    public UsuarioDto Usuario { get; init; } = null!;
}
