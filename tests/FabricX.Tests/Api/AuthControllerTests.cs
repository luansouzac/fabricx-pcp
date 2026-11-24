using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FabricX.Domain.Enums;
using FabricX.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FabricX.Tests.Api;

public class AuthControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _fabrica;
    private readonly HttpClient _cliente;

    public AuthControllerTests(ApiFactory fabrica)
    {
        _fabrica = fabrica;
        _cliente = fabrica.CreateClient();
    }

    private static string EmailUnico() => $"u{Guid.NewGuid():N}@fabricx.com";

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta) =>
        await resposta.Content.ReadFromJsonAsync<JsonElement>();

    [Fact]
    public async Task Login_CredenciaisCorretas_Retorna200ComJwt()
    {
        var email = EmailUnico();
        _fabrica.CriarUsuario(email, "Senha@123", PerfilUsuario.Operador);

        var resposta = await _cliente.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var dados = (await LerAsync(resposta)).GetProperty("data");
        Assert.Equal("Bearer", dados.GetProperty("tipo").GetString());
        var jwt = new JsonWebToken(dados.GetProperty("token").GetString());
        Assert.Equal(email, jwt.GetClaim("email").Value);
        Assert.Equal("Operador", jwt.GetClaim("role").Value);
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_SenhaErrada_Retorna401NoFormatoWebResponse()
    {
        var email = EmailUnico();
        _fabrica.CriarUsuario(email, "Senha@123", PerfilUsuario.Operador);

        var resposta = await _cliente.PostAsJsonAsync("/api/auth/login", new { email, senha = "errada" });

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var corpo = await LerAsync(resposta);
        Assert.False(corpo.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(401, corpo.GetProperty("statusCode").GetInt32());
    }

    [Fact]
    public async Task Login_CamposVazios_Retorna400()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/auth/login", new { email = "", senha = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Registro_NaoExiste()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/auth/registro", new { nome = "Maria", email = EmailUnico(), senha = "Senha@123" });

        Assert.True(resposta.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Unauthorized or HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Login_AcimaDoLimite_Retorna429NoFormatoWebResponse()
    {
        var limitada = _fabrica.WithWebHostBuilder(b => b.UseSetting("RateLimit:LoginPorMinuto", "2")).CreateClient();
        var corpo = new { email = "x@y.com", senha = "errada" };

        await limitada.PostAsJsonAsync("/api/auth/login", corpo);
        await limitada.PostAsJsonAsync("/api/auth/login", corpo);
        var terceira = await limitada.PostAsJsonAsync("/api/auth/login", corpo);

        Assert.Equal(HttpStatusCode.TooManyRequests, terceira.StatusCode);
        var resposta = await LerAsync(terceira);
        Assert.False(resposta.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(429, resposta.GetProperty("statusCode").GetInt32());
    }
}
