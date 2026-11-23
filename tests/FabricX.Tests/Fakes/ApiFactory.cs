using System.Net.Http.Headers;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FabricX.Tests.Fakes;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string ChaveDeTeste = "chave-de-teste-com-mais-de-trinta-e-dois-caracteres";

    private readonly string _arquivoBanco = Path.Combine(Path.GetTempPath(), $"fabricx-teste-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Chave", ChaveDeTeste);
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_arquivoBanco};Pooling=False");
    }

    public HttpClient CriarClienteAutenticado(PerfilUsuario perfil = PerfilUsuario.Administrador)
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GerarToken(perfil));
        return cliente;
    }

    public string GerarToken(PerfilUsuario perfil)
    {
        using var escopo = Services.CreateScope();
        var usuario = new Usuario("Teste", $"{perfil}@fabricx.com".ToLowerInvariant(), "hash", perfil);
        typeof(Usuario).GetProperty(nameof(Usuario.Id))!.SetValue(usuario, 1);
        return escopo.ServiceProvider.GetRequiredService<IGeradorDeToken>().Gerar(usuario).Token;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (File.Exists(_arquivoBanco))
            File.Delete(_arquivoBanco);
    }
}
