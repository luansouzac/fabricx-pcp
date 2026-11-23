using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FabricX.Tests.Fakes;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FabricX.Tests.Api;

public class AuthControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _cliente;

    public AuthControllerTests(ApiFactory fabrica)
    {
        _cliente = fabrica.CreateClient();
    }

    private static string EmailUnico() => $"u{Guid.NewGuid():N}@fabricx.com";

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta) =>
        await resposta.Content.ReadFromJsonAsync<JsonElement>();

    private Task<HttpResponseMessage> RegistrarAsync(string email, string senha = "Senha@123") =>
        _cliente.PostAsJsonAsync("/api/auth/registro", new { nome = "Maria", email, senha });

    [Fact]
    public async Task Registro_DadosValidos_Retorna201SemExporSenha()
    {
        var resposta = await RegistrarAsync(EmailUnico());

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var corpo = await LerAsync(resposta);
        Assert.True(corpo.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Operador", corpo.GetProperty("data").GetProperty("perfil").GetString());
        Assert.False(corpo.GetProperty("data").TryGetProperty("senhaHash", out _));
        Assert.False(corpo.GetProperty("data").TryGetProperty("senha", out _));
    }

    [Fact]
    public async Task Registro_EmailDuplicado_Retorna400()
    {
        var email = EmailUnico();
        await RegistrarAsync(email);

        var resposta = await RegistrarAsync(email);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.False((await LerAsync(resposta)).GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task Registro_SenhaCurta_Retorna400()
    {
        var resposta = await RegistrarAsync(EmailUnico(), "curta");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_CredenciaisCorretas_Retorna200ComJwt()
    {
        var email = EmailUnico();
        await RegistrarAsync(email);

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
        await RegistrarAsync(email);

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
}
