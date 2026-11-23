using System.Security.Claims;
using System.Text;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FabricX.Infrastructure.Seguranca;

public class GeradorDeTokenJwt : IGeradorDeToken
{
    public const string ClaimPerfil = "role";

    private readonly JwtOptions _opcoes;

    public GeradorDeTokenJwt(IOptions<JwtOptions> opcoes)
    {
        _opcoes = opcoes.Value;
    }

    public TokenGerado Gerar(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_opcoes.ExpiraEmMinutos);

        var descritor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
                new Claim(ClaimPerfil, usuario.Perfil.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            Issuer = _opcoes.Emissor,
            Audience = _opcoes.Audiencia,
            Expires = expiraEm,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opcoes.Chave)),
                SecurityAlgorithms.HmacSha256)
        };

        return new TokenGerado(new JsonWebTokenHandler().CreateToken(descritor), expiraEm);
    }
}
