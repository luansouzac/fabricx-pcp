using FabricX.Application.DTOs.Auth;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repositorio;
    private readonly ISenhaHasher _hasher;

    public UsuarioService(IUsuarioRepository repositorio, ISenhaHasher hasher)
    {
        _repositorio = repositorio;
        _hasher = hasher;
    }

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken cancellationToken)
    {
        var usuarios = await _repositorio.ListarAsync(cancellationToken);
        return usuarios.Select(UsuarioDto.De).ToList();
    }

    public async Task<UsuarioDto> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await ObterOuFalharAsync(id, cancellationToken);
        return UsuarioDto.De(usuario);
    }

    public async Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(dto.Nome!, dto.Email!, _hasher.Gerar(dto.Senha!), dto.Perfil);

        if (await _repositorio.ObterPorEmailAsync(usuario.Email, cancellationToken) is not null)
            throw new DomainException($"Já existe um usuário com o e-mail {usuario.Email}.");

        await _repositorio.AdicionarAsync(usuario, cancellationToken);
        return UsuarioDto.De(usuario);
    }

    public async Task<UsuarioDto> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var usuario = await ObterOuFalharAsync(id, cancellationToken);

        if (dto.Perfil != PerfilUsuario.Administrador)
            await GarantirOutroAdministradorAtivoAsync(usuario, cancellationToken);

        usuario.Atualizar(dto.Nome!, dto.Perfil);

        await _repositorio.AtualizarAsync(usuario, cancellationToken);
        return UsuarioDto.De(usuario);
    }

    public async Task<UsuarioDto> InativarAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await ObterOuFalharAsync(id, cancellationToken);

        await GarantirOutroAdministradorAtivoAsync(usuario, cancellationToken);

        usuario.Inativar();

        await _repositorio.AtualizarAsync(usuario, cancellationToken);
        return UsuarioDto.De(usuario);
    }

    private async Task GarantirOutroAdministradorAtivoAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        if (usuario is not { Perfil: PerfilUsuario.Administrador, Ativo: true })
            return;

        var usuarios = await _repositorio.ListarAsync(cancellationToken);
        if (!usuarios.Any(u => u.Id != usuario.Id && u.Ativo && u.Perfil == PerfilUsuario.Administrador))
            throw new DomainException("O sistema precisa de ao menos um administrador ativo.");
    }

    private async Task<Usuario> ObterOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _repositorio.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Usuário {id} não encontrado.");
}
