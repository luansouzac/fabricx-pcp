using FabricX.Application.DTOs.Auth;
using FabricX.Application.Exceptions;
using FabricX.Application.Services;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
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

    private async Task CadastrarMariaAsync() =>
        await _repositorio.AdicionarAsync(new Usuario("Maria", "maria@fabricx.com", _hasher.Gerar("Senha@123"), PerfilUsuario.Operador), Ct);

    [Fact]
    public async Task Login_CredenciaisCorretas_DevolveTokenComUsuarioEExpiracao()
    {
        await CadastrarMariaAsync();

        var resultado = await _service.LoginAsync(new LoginDto { Email = " Maria@Fabricx.com ", Senha = "Senha@123" }, Ct);

        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.Equal("Bearer", resultado.Tipo);
        Assert.Equal("maria@fabricx.com", resultado.Usuario.Email);
        Assert.InRange(resultado.ExpiraEmUtc, DateTime.UtcNow.AddMinutes(29), DateTime.UtcNow.AddMinutes(31));
    }

    [Fact]
    public async Task Login_SenhaErrada_LancaNaoAutorizado()
    {
        await CadastrarMariaAsync();

        await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            _service.LoginAsync(new LoginDto { Email = "maria@fabricx.com", Senha = "errada" }, Ct));
    }

    [Fact]
    public async Task Login_EmailInexistente_LancaNaoAutorizadoComAMesmaMensagemDaSenhaErrada()
    {
        await CadastrarMariaAsync();

        var semUsuario = await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            _service.LoginAsync(new LoginDto { Email = "nao@existe.com", Senha = "Senha@123" }, Ct));
        var senhaErrada = await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            _service.LoginAsync(new LoginDto { Email = "maria@fabricx.com", Senha = "errada" }, Ct));

        Assert.Equal(senhaErrada.Message, semUsuario.Message);
    }

    [Fact]
    public async Task Login_UsuarioInativo_LancaNaoAutorizado()
    {
        await CadastrarMariaAsync();
        (await _repositorio.ObterPorEmailAsync("maria@fabricx.com", Ct))!.Inativar();

        await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            _service.LoginAsync(new LoginDto { Email = "maria@fabricx.com", Senha = "Senha@123" }, Ct));
    }

    [Fact]
    public async Task Login_EmailInexistente_AindaVerificaASenhaParaNaoRevelarOTempo()
    {
        var hasher = new HasherContador(_hasher);
        var service = new AuthService(_repositorio, hasher, new GeradorDeTokenJwt(Options.Create(new JwtOptions { Chave = ApiFactory.ChaveDeTeste })));

        await Assert.ThrowsAsync<NaoAutorizadoException>(() =>
            service.LoginAsync(new LoginDto { Email = "nao@existe.com", Senha = "Senha@123" }, Ct));

        Assert.Equal(1, hasher.Verificacoes);
    }

    private sealed class HasherContador(SenhaHasherPbkdf2 interno) : FabricX.Application.Interfaces.ISenhaHasher
    {
        public int Verificacoes { get; private set; }

        public string Gerar(string senha) => interno.Gerar(senha);

        public bool Verificar(string senha, string hash)
        {
            Verificacoes++;
            return interno.Verificar(senha, hash);
        }
    }
}
