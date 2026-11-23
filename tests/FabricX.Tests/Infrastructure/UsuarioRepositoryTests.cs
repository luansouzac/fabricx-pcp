using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class UsuarioRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    [Theory]
    [InlineData(PerfilUsuario.Operador, "OPERADOR")]
    [InlineData(PerfilUsuario.Administrador, "ADMINISTRADOR")]
    public async Task Adicionar_GravaPerfilComoTextoDoCheck(PerfilUsuario perfil, string esperado)
    {
        await using var contexto = _banco.CriarContexto();

        await new UsuarioRepository(contexto).AdicionarAsync(new Usuario("Maria", "maria@fabricx.com", "hash", perfil), Ct);

        var texto = await contexto.Database.SqlQuery<string>($"SELECT perfil AS Value FROM usuario").SingleAsync(Ct);
        Assert.Equal(esperado, texto);
    }

    [Fact]
    public async Task ObterPorEmail_AchaUsuarioPeloEmailNormalizado()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new UsuarioRepository(contexto);
        await repositorio.AdicionarAsync(new Usuario("Maria", "Maria@Fabricx.com", "hash", PerfilUsuario.Operador), Ct);

        var usuario = await repositorio.ObterPorEmailAsync("maria@fabricx.com", Ct);

        Assert.NotNull(usuario);
        Assert.Equal("Maria", usuario.Nome);
        Assert.Null(await repositorio.ObterPorEmailAsync("outro@fabricx.com", Ct));
    }

    [Fact]
    public async Task Adicionar_EmailDuplicado_ViolaOUniqueDoBanco()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new UsuarioRepository(contexto);
        await repositorio.AdicionarAsync(new Usuario("Maria", "maria@fabricx.com", "hash", PerfilUsuario.Operador), Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repositorio.AdicionarAsync(new Usuario("Outra", "maria@fabricx.com", "hash", PerfilUsuario.Operador), Ct));
    }

    [Fact]
    public async Task Listar_OrdenaPorNome()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new UsuarioRepository(contexto);
        await repositorio.AdicionarAsync(new Usuario("Zeca", "z@fabricx.com", "hash", PerfilUsuario.Operador), Ct);
        await repositorio.AdicionarAsync(new Usuario("Ana", "a@fabricx.com", "hash", PerfilUsuario.Operador), Ct);

        var usuarios = await repositorio.ListarAsync(Ct);

        Assert.Equal(["Ana", "Zeca"], usuarios.Select(u => u.Nome));
    }
}
