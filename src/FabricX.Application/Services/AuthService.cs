using FabricX.Application.DTOs.Auth;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Application.Services;

public class AuthService : IAuthService
{
    private const string MensagemCredenciaisInvalidas = "E-mail ou senha inválidos.";

    // Hash válido de uma senha qualquer: e-mail inexistente também paga o custo do PBKDF2, e o tempo não revela quais e-mails existem.
    private const string HashFicticio = "PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=";

    private readonly IUsuarioRepository _repositorio;
    private readonly ISenhaHasher _hasher;
    private readonly IGeradorDeToken _gerador;

    public AuthService(IUsuarioRepository repositorio, ISenhaHasher hasher, IGeradorDeToken gerador)
    {
        _repositorio = repositorio;
        _hasher = hasher;
        _gerador = gerador;
    }

    public async Task<TokenDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _repositorio.ObterPorEmailAsync(dto.Email!.Trim().ToLowerInvariant(), cancellationToken);

        var senhaConfere = _hasher.Verificar(dto.Senha!, usuario?.SenhaHash ?? HashFicticio);

        if (usuario is null || !usuario.Ativo || !senhaConfere)
            throw new NaoAutorizadoException(MensagemCredenciaisInvalidas);

        var token = _gerador.Gerar(usuario);

        return new TokenDto
        {
            Token = token.Token,
            ExpiraEmUtc = token.ExpiraEmUtc,
            Usuario = UsuarioDto.De(usuario)
        };
    }
}
