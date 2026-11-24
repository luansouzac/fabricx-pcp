using System.Text;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Seguranca;
using FabricX.Tests.Fakes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FabricX.Tests.Infrastructure;

public class GeradorDeTokenJwtTests
{
    private static readonly Usuario Usuario = CriarUsuario();

    private static Usuario CriarUsuario()
    {
        var usuario = new Usuario("Maria", "maria@fabricx.com", "hash", PerfilUsuario.Administrador);
        typeof(Usuario).GetProperty(nameof(Usuario.Id))!.SetValue(usuario, 7);
        return usuario;
    }

    private static GeradorDeTokenJwt Gerador(int expiraEmMinutos = 30, string chave = ApiFactory.ChaveDeTeste) =>
        new(Options.Create(new JwtOptions { Chave = chave, Emissor = "emissor", Audiencia = "audiencia", ExpiraEmMinutos = expiraEmMinutos }));

    private static TokenValidationParameters Parametros(string chave = ApiFactory.ChaveDeTeste) => new()
    {
        ValidIssuer = "emissor",
        ValidAudience = "audiencia",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
        ClockSkew = TimeSpan.Zero
    };

    [Fact]
    public async Task Gerar_TokenValidaComAChaveECarregaAsClaimsDoUsuario()
    {
        var token = Gerador().Gerar(Usuario);

        var resultado = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, Parametros());

        Assert.True(resultado.IsValid);
        var jwt = (JsonWebToken)resultado.SecurityToken;
        Assert.Equal("7", jwt.Subject);
        Assert.Equal("maria@fabricx.com", jwt.GetClaim(JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Administrador", jwt.GetClaim("role").Value);
        Assert.Equal("emissor", jwt.Issuer);
        Assert.Contains("audiencia", jwt.Audiences);
    }

    [Fact]
    public void Gerar_ExpiraEmUtcBateComAConfiguracao()
    {
        var token = Gerador(expiraEmMinutos: 15).Gerar(Usuario);

        Assert.InRange(token.ExpiraEmUtc, DateTime.UtcNow.AddMinutes(14), DateTime.UtcNow.AddMinutes(16));
    }

    [Fact]
    public async Task Gerar_ChaveDiferenteNaValidacao_Invalida()
    {
        var token = Gerador().Gerar(Usuario);

        var resultado = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, Parametros("outra-chave-com-mais-de-trinta-e-dois-caracteres"));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public async Task Gerar_TokenExpirado_Invalida()
    {
        var token = Gerador(expiraEmMinutos: -1).Gerar(Usuario);

        var resultado = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, Parametros());

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Gerar_CadaTokenTemJtiProprio()
    {
        var gerador = Gerador();
        var primeiro = new JsonWebToken(gerador.Gerar(Usuario).Token);
        var segundo = new JsonWebToken(gerador.Gerar(Usuario).Token);

        Assert.NotEqual(primeiro.Id, segundo.Id);
    }
}
