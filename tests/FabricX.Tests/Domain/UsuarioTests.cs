using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Tests.Domain;

public class UsuarioTests
{
    [Fact]
    public void Construtor_DadosValidos_CriaUsuarioAtivoComEmailNormalizado()
    {
        var usuario = new Usuario("  Maria  ", "  Maria@Fabricx.COM ", "hash", PerfilUsuario.Operador);

        Assert.Equal("Maria", usuario.Nome);
        Assert.Equal("maria@fabricx.com", usuario.Email);
        Assert.Equal("hash", usuario.SenhaHash);
        Assert.Equal(PerfilUsuario.Operador, usuario.Perfil);
        Assert.True(usuario.Ativo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Construtor_NomeVazio_LancaDomainException(string nome)
    {
        Assert.Throws<DomainException>(() => new Usuario(nome, "a@b.com", "hash", PerfilUsuario.Operador));
    }

    [Theory]
    [InlineData("")]
    [InlineData("semarroba")]
    [InlineData("@b.com")]
    [InlineData("a@b")]
    [InlineData("a@@b.com")]
    [InlineData("a @b.com")]
    [InlineData("a@b.")]
    public void Construtor_EmailInvalido_LancaDomainException(string email)
    {
        Assert.Throws<DomainException>(() => new Usuario("Maria", email, "hash", PerfilUsuario.Operador));
    }

    [Fact]
    public void Construtor_SenhaHashVazio_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new Usuario("Maria", "a@b.com", " ", PerfilUsuario.Operador));
    }

    [Fact]
    public void Construtor_PerfilInexistente_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new Usuario("Maria", "a@b.com", "hash", (PerfilUsuario)99));
    }

    [Fact]
    public void Atualizar_TrocaNomeEPerfilSemMexerNoEmail()
    {
        var usuario = new Usuario("Maria", "a@b.com", "hash", PerfilUsuario.Operador);

        usuario.Atualizar("Maria Silva", PerfilUsuario.Administrador);

        Assert.Equal("Maria Silva", usuario.Nome);
        Assert.Equal(PerfilUsuario.Administrador, usuario.Perfil);
        Assert.Equal("a@b.com", usuario.Email);
    }

    [Fact]
    public void InativarEAtivar_AlternamAtivo()
    {
        var usuario = new Usuario("Maria", "a@b.com", "hash", PerfilUsuario.Operador);

        usuario.Inativar();
        Assert.False(usuario.Ativo);

        usuario.Ativar();
        Assert.True(usuario.Ativo);
    }

    [Fact]
    public void TrocarSenha_GuardaONovoHash()
    {
        var usuario = new Usuario("Maria", "a@b.com", "antigo", PerfilUsuario.Operador);

        usuario.TrocarSenha("novo");

        Assert.Equal("novo", usuario.SenhaHash);
    }
}
