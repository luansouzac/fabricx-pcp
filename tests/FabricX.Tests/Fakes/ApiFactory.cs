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
        builder.UseSetting("RateLimit:LoginPorMinuto", "1000");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_arquivoBanco};Pooling=False");
    }

    public HttpClient CriarClienteAutenticado(PerfilUsuario perfil = PerfilUsuario.Administrador)
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GerarToken(perfil));
        return cliente;
    }

    public HttpClient CriarClienteAutenticadoComo(Usuario usuario)
    {
        using var escopo = Services.CreateScope();
        var token = escopo.ServiceProvider.GetRequiredService<IGeradorDeToken>().Gerar(usuario).Token;
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    public Usuario CriarUsuario(string email, string senha, PerfilUsuario perfil)
    {
        using var escopo = Services.CreateScope();
        var hasher = escopo.ServiceProvider.GetRequiredService<ISenhaHasher>();
        var usuario = new Usuario(email, email, hasher.Gerar(senha), perfil);
        escopo.ServiceProvider.GetRequiredService<IUsuarioRepository>().AdicionarAsync(usuario, CancellationToken.None).GetAwaiter().GetResult();
        return usuario;
    }

    public string GerarToken(PerfilUsuario perfil)
    {
        using var escopo = Services.CreateScope();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var email = $"{perfil}@fabricx.com".ToLowerInvariant();
        var usuario = repositorio.ObterPorEmailAsync(email, CancellationToken.None).GetAwaiter().GetResult()
            ?? CriarUsuario(email, "Senha@123", perfil);
        return escopo.ServiceProvider.GetRequiredService<IGeradorDeToken>().Gerar(usuario).Token;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (File.Exists(_arquivoBanco))
            File.Delete(_arquivoBanco);
    }
}
