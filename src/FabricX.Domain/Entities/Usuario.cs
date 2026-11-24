using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public PerfilUsuario Perfil { get; private set; }
    public bool Ativo { get; private set; }

    private Usuario()
    {
        Nome = null!;
        Email = null!;
        SenhaHash = null!;
    }

    public Usuario(string nome, string email, string senhaHash, PerfilUsuario perfil)
    {
        Nome = ValidarNome(nome);
        Email = ValidarEmail(email);
        SenhaHash = ValidarSenhaHash(senhaHash);
        Perfil = ValidarPerfil(perfil);
        Ativo = true;
    }

    public void Atualizar(string nome, PerfilUsuario perfil)
    {
        Nome = ValidarNome(nome);
        Perfil = ValidarPerfil(perfil);
    }

    public void TrocarSenha(string senhaHash) => SenhaHash = ValidarSenhaHash(senhaHash);

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;

    private static string ValidarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do usuário é obrigatório.");

        return nome.Trim();
    }

    private static string ValidarEmail(string? email)
    {
        var normalizado = email?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(normalizado))
            throw new DomainException("O e-mail do usuário é obrigatório.");

        var arroba = normalizado.IndexOf('@');
        var emailValido = arroba > 0
            && arroba == normalizado.LastIndexOf('@')
            && normalizado.IndexOf('.', arroba) > arroba + 1
            && !normalizado.EndsWith('.')
            && !normalizado.Contains(' ');

        if (!emailValido)
            throw new DomainException("E-mail inválido.");

        return normalizado;
    }

    private static string ValidarSenhaHash(string? senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("A senha do usuário é obrigatória.");

        return senhaHash;
    }

    private static PerfilUsuario ValidarPerfil(PerfilUsuario perfil)
    {
        if (!Enum.IsDefined(perfil))
            throw new DomainException("Perfil de usuário inválido.");

        return perfil;
    }
}
