using FabricX.Application.DTOs.Auth;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Infrastructure.Seguranca;
using FabricX.Tests.Fakes;

namespace FabricX.Tests.Application;

public class UsuarioServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly UsuarioRepositoryFake _repositorio = new();
    private readonly SenhaHasherPbkdf2 _hasher = new();
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _service = new UsuarioService(_repositorio, _hasher);
    }

    private static CriarUsuarioDto Dto(string email = "ana@fabricx.com", PerfilUsuario perfil = PerfilUsuario.Operador) =>
        new() { Nome = "Ana", Email = email, Senha = "Senha@123", Perfil = perfil };

    [Fact]
    public async Task Criar_GuardaSenhaEmHashEOPerfilInformado()
    {
        var usuario = await _service.CriarAsync(Dto(perfil: PerfilUsuario.Administrador), Ct);

        Assert.Equal(PerfilUsuario.Administrador, usuario.Perfil);
        var gravado = await _repositorio.ObterPorIdAsync(usuario.Id, Ct);
        Assert.True(_hasher.Verificar("Senha@123", gravado!.SenhaHash));
    }

    [Fact]
    public async Task Criar_EmailDuplicado_LancaDomainException()
    {
        await _service.CriarAsync(Dto(), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.CriarAsync(Dto("ANA@fabricx.com"), Ct));
    }

    [Fact]
    public async Task Atualizar_TrocaNomeEPerfil()
    {
        var criado = await _service.CriarAsync(Dto(), Ct);

        var atualizado = await _service.AtualizarAsync(criado.Id, new AtualizarUsuarioDto { Nome = "Ana Souza", Perfil = PerfilUsuario.Administrador }, Ct);

        Assert.Equal("Ana Souza", atualizado.Nome);
        Assert.Equal(PerfilUsuario.Administrador, atualizado.Perfil);
    }

    [Fact]
    public async Task Inativar_MarcaComoInativoSemApagar()
    {
        var criado = await _service.CriarAsync(Dto(), Ct);

        var inativo = await _service.InativarAsync(criado.Id, Ct);

        Assert.False(inativo.Ativo);
        Assert.Single(await _service.ListarAsync(Ct));
    }

    [Fact]
    public async Task ObterPorId_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterPorIdAsync(99, Ct));
    }

    [Fact]
    public async Task Atualizar_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.AtualizarAsync(99, new AtualizarUsuarioDto { Nome = "X", Perfil = PerfilUsuario.Operador }, Ct));
    }
}
