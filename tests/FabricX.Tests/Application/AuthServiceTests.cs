using FabricX.Application.DTOs.Auth;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Infrastructure.Seguranca;
using FabricX.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace FabricX.Tests.Application;

public class AuthServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly UsuarioRepositoryFake _repositorio = new();
    private readonly SenhaHasherPbkdf2 _hasher = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var opcoes = Options.Create(new JwtOptions { Chave = ApiFactory.ChaveDeTeste, Emissor = "emissor", Audiencia = "audiencia", ExpiraEmMinutos = 30 });
        _service = new AuthService(_repositorio, _hasher, new GeradorDeTokenJwt(opcoes));
    }

    private static RegistrarUsuarioDto Registro(string email = "maria@fabricx.com", string senha = "Senha@123") =>
        new() { Nome = "Maria", Email = email, Senha = senha };

    [Fact]
    public async Task Registrar_CriaOperadorComSenhaEmHash()
    {
        var usuario = await _service.RegistrarAsync(Registro(), Ct);

        Assert.Equal(PerfilUsuario.Operador, usuario.Perfil);
        Assert.True(usuario.Ativo);
        var gravado = await _repositorio.ObterPorEmailAsync("maria@fabricx.com", Ct);
        Assert.NotNull(gravado);
        Assert.NotEqual("Senha@123", gravado.SenhaHash);
        Assert.True(_hasher.Verificar("Senha@123", gravado.SenhaHash));
    }

    [Fact]
    public async Task Registrar_EmailJaCadastradoEmOutraCaixa_LancaDomainException()
    {
        await _service.RegistrarAsync(Registro(), Ct);

        await Assert.ThrowsAsync<DomainException>(() => _service.RegistrarAsync(Registro("MARIA@fabricx.com"), Ct));
    }

    [Fact]
    public async Task Registrar_EmailInvalido_LancaDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(() => _service.RegistrarAsync(Registro("semarroba"), Ct));
    }

    [Fact]
    public async Task Login_CredenciaisCorretas_DevolveTokenComUsuarioEExpiracao()
    {
        await _service.RegistrarAsync(Registro(), Ct);

        var resultado = await _service.LoginAsync(new LoginDto { Email = " Maria@Fabricx.com ", Senha = "Senha@123" }, Ct);

        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.Equal("Bearer", resultado.Tipo);
        Assert.Equal("maria@fabricx.com", resultado.Usuario.Email);
        Assert.InRange(resultado.ExpiraEmUtc, DateTime.UtcNow.AddMinutes(29), DateTime.UtcNow.AddMinutes(31));
    }

    [Fact]
    public async Task Login_SenhaErrada_LancaNaoAutorizado()
    {
        await _service.RegistrarAsync(Registro(), Ct);

        await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            _service.LoginAsync(new LoginDto { Email = "maria@fabricx.com", Senha = "errada" }, Ct));
    }

    [Fact]
    public async Task Login_EmailInexistente_LancaNaoAutorizadoComAMesmaMensagemDaSenhaErrada()
    {
        await _service.RegistrarAsync(Registro(), Ct);

        var semUsuario = await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            _service.LoginAsync(new LoginDto { Email = "nao@existe.com", Senha = "Senha@123" }, Ct));
        var senhaErrada = await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            _service.LoginAsync(new LoginDto { Email = "maria@fabricx.com", Senha = "errada" }, Ct));

        Assert.Equal(senhaErrada.Message, semUsuario.Message);
    }

    [Fact]
    public async Task Login_UsuarioInativo_LancaNaoAutorizado()
    {
        await _service.RegistrarAsync(Registro(), Ct);
        (await _repositorio.ObterPorEmailAsync("maria@fabricx.com", Ct))!.Inativar();

        await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            _service.LoginAsync(new LoginDto { Email = "maria@fabricx.com", Senha = "Senha@123" }, Ct));
    }
}
