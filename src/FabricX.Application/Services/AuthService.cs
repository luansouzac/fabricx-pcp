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

    private readonly IUsuarioRepository _repositorio;
    private readonly ISenhaHasher _hasher;
    private readonly IGeradorDeToken _gerador;

    public AuthService(IUsuarioRepository repositorio, ISenhaHasher hasher, IGeradorDeToken gerador)
    {
        _repositorio = repositorio;
        _hasher = hasher;
        _gerador = gerador;
    }

    public async Task<UsuarioDto> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(dto.Nome!, dto.Email!, _hasher.Gerar(dto.Senha!), PerfilUsuario.Operador);

        if (await _repositorio.ObterPorEmailAsync(usuario.Email, cancellationToken) is not null)
            throw new DomainException($"Já existe um usuário com o e-mail {usuario.Email}.");

        await _repositorio.AdicionarAsync(usuario, cancellationToken);
        return UsuarioDto.De(usuario);
    }

    public async Task<TokenDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _repositorio.ObterPorEmailAsync(dto.Email!.Trim().ToLowerInvariant(), cancellationToken);

        if (usuario is null || !usuario.Ativo || !_hasher.Verificar(dto.Senha!, usuario.SenhaHash))
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
