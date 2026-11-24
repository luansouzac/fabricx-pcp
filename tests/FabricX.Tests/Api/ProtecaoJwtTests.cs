using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FabricX.Domain.Enums;
using FabricX.Tests.Fakes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FabricX.Tests.Api;

public class ProtecaoJwtTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _fabrica;

    public ProtecaoJwtTests(ApiFactory fabrica)
    {
        _fabrica = fabrica;
    }

    private HttpClient ClienteComToken(string token)
    {
        var cliente = _fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    private static string TokenAssinado(string chave, string emissor = "FabricX.Api", string audiencia = "FabricX.Clientes", DateTime? expira = null) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity([new("sub", "1"), new("role", "Administrador")]),
            Issuer = emissor,
            Audience = audiencia,
            NotBefore = (expira ?? DateTime.UtcNow.AddHours(1)).AddHours(-2),
            Expires = expira ?? DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)), SecurityAlgorithms.HmacSha256)
        });

    [Theory]
    [InlineData("/api/produtos")]
    [InlineData("/api/recursos")]
    [InlineData("/api/ordens-producao")]
    [InlineData("/api/indicadores/resumo")]
    [InlineData("/api/capacidade")]
    [InlineData("/api/usuarios")]
    public async Task SemToken_Retorna401NoFormatoWebResponse(string rota)
    {
        var resposta = await _fabrica.CreateClient().GetAsync(rota);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(corpo.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(401, corpo.GetProperty("statusCode").GetInt32());
    }

    [Fact]
    public async Task TokenInvalido_Retorna401()
    {
        var resposta = await ClienteComToken("isto.nao.eh-um-jwt").GetAsync("/api/produtos");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenAssinadoComOutraChave_Retorna401()
    {
        var token = TokenAssinado("outra-chave-com-mais-de-trinta-e-dois-caracteres");

        var resposta = await ClienteComToken(token).GetAsync("/api/produtos");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenExpirado_Retorna401()
    {
        var token = TokenAssinado(ApiFactory.ChaveDeTeste, expira: DateTime.UtcNow.AddMinutes(-10));

        var resposta = await ClienteComToken(token).GetAsync("/api/produtos");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenDeOutroEmissor_Retorna401()
    {
        var token = TokenAssinado(ApiFactory.ChaveDeTeste, emissor: "outro-emissor");

        var resposta = await ClienteComToken(token).GetAsync("/api/produtos");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task TokenValido_Retorna200()
    {
        var resposta = await _fabrica.CriarClienteAutenticado(PerfilUsuario.Operador).GetAsync("/api/produtos");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task LoginReal_TokenRecebidoAbreEndpointProtegido()
    {
        var cliente = _fabrica.CreateClient();
        var email = $"u{Guid.NewGuid():N}@fabricx.com";
        _fabrica.CriarUsuario(email, "Senha@123", PerfilUsuario.Operador);
        var login = await cliente.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("token").GetString()!;

        var resposta = await ClienteComToken(token).GetAsync("/api/recursos");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Operador_NaoAcessaUsuarios_Retorna403NoFormatoWebResponse()
    {
        var resposta = await _fabrica.CriarClienteAutenticado(PerfilUsuario.Operador).GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(corpo.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(403, corpo.GetProperty("statusCode").GetInt32());
    }

    [Theory]
    [InlineData("/api/produtos/1")]
    [InlineData("/api/recursos/1")]
    public async Task Operador_NaoInativaProdutoNemRecurso_Retorna403(string rota)
    {
        var resposta = await _fabrica.CriarClienteAutenticado(PerfilUsuario.Operador).DeleteAsync(rota);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Administrador_AcessaUsuarios_Retorna200()
    {
        var resposta = await _fabrica.CriarClienteAutenticado(PerfilUsuario.Administrador).GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Administrador_CriaEAtualizaUsuario()
    {
        var admin = _fabrica.CriarClienteAutenticado(PerfilUsuario.Administrador);
        var email = $"u{Guid.NewGuid():N}@fabricx.com";

        var criado = await admin.PostAsJsonAsync("/api/usuarios", new { nome = "Pedro", email, senha = "Senha@123", perfil = "Administrador" });
        var id = (await criado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("id").GetInt32();
        var atualizado = await admin.PutAsJsonAsync($"/api/usuarios/{id}", new { nome = "Pedro Souza", perfil = "Operador" });
        var inativado = await admin.DeleteAsync($"/api/usuarios/{id}");

        Assert.Equal(HttpStatusCode.Created, criado.StatusCode);
        Assert.Equal("Operador", (await atualizado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("perfil").GetString());
        Assert.False((await inativado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("ativo").GetBoolean());
    }

    [Fact]
    public async Task UsuarioInativado_NaoConsegueMaisLogar()
    {
        var admin = _fabrica.CriarClienteAutenticado(PerfilUsuario.Administrador);
        var email = $"u{Guid.NewGuid():N}@fabricx.com";
        var criado = await admin.PostAsJsonAsync("/api/usuarios", new { nome = "Pedro", email, senha = "Senha@123", perfil = "Operador" });
        var id = (await criado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("id").GetInt32();
        await admin.DeleteAsync($"/api/usuarios/{id}");

        var login = await _fabrica.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task TokenDeUsuarioInativado_PassaASerRejeitadoAntesDeExpirar()
    {
        var usuario = _fabrica.CriarUsuario($"u{Guid.NewGuid():N}@fabricx.com", "Senha@123", PerfilUsuario.Operador);
        var token = _fabrica.CriarClienteAutenticadoComo(usuario);
        Assert.Equal(HttpStatusCode.OK, (await token.GetAsync("/api/produtos")).StatusCode);

        await _fabrica.CriarClienteAutenticado(PerfilUsuario.Administrador).DeleteAsync($"/api/usuarios/{usuario.Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, (await token.GetAsync("/api/produtos")).StatusCode);
    }

    [Fact]
    public async Task Login_EhOUnicoEndpointPublico()
    {
        var cliente = _fabrica.CreateClient();

        var login = await cliente.PostAsJsonAsync("/api/auth/login", new { email = "x@y.com", senha = "qualquer" });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task TransformadorOpenApi_DeclaraOEsquemaBearerEExigeOTokenPorPadrao()
    {
        var documento = new Microsoft.OpenApi.OpenApiDocument();

        await new FabricX.Api.Common.BearerOpenApiTransformer().TransformAsync(documento, null!, CancellationToken.None);

        var esquema = Assert.IsType<Microsoft.OpenApi.OpenApiSecurityScheme>(documento.Components!.SecuritySchemes!["Bearer"]);
        Assert.Equal("bearer", esquema.Scheme);
        Assert.Equal("JWT", esquema.BearerFormat);
        Assert.Equal(Microsoft.OpenApi.SecuritySchemeType.Http, esquema.Type);
        Assert.Single(documento.Security!);
    }
}
