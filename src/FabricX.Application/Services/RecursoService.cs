using FabricX.Application.DTOs.Recursos;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;

namespace FabricX.Application.Services;

public class RecursoService : IRecursoService
{
    private readonly IRecursoRepository _repositorio;

    public RecursoService(IRecursoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IReadOnlyList<RecursoDto>> ListarAsync(CancellationToken cancellationToken)
    {
        var recursos = await _repositorio.ListarAsync(cancellationToken);
        return recursos.Select(RecursoDto.De).ToList();
    }

    public async Task<RecursoDto> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var recurso = await ObterOuFalharAsync(id, cancellationToken);
        return RecursoDto.De(recurso);
    }

    public async Task<RecursoDto> CriarAsync(CriarRecursoDto dto, CancellationToken cancellationToken)
    {
        var recurso = new Recurso(dto.Nome!, dto.Tipo, dto.CapacidadeHorasDia);

        await GarantirNomeLivreAsync(recurso.Nome, null, cancellationToken);

        await _repositorio.AdicionarAsync(recurso, cancellationToken);
        return RecursoDto.De(recurso);
    }

    public async Task<RecursoDto> AtualizarAsync(int id, AtualizarRecursoDto dto, CancellationToken cancellationToken)
    {
        var recurso = await ObterOuFalharAsync(id, cancellationToken);

        recurso.Atualizar(dto.Nome!, dto.Tipo, dto.CapacidadeHorasDia);
        await GarantirNomeLivreAsync(recurso.Nome, recurso.Id, cancellationToken);

        await _repositorio.AtualizarAsync(recurso, cancellationToken);
        return RecursoDto.De(recurso);
    }

    public async Task<RecursoDto> InativarAsync(int id, CancellationToken cancellationToken)
    {
        var recurso = await ObterOuFalharAsync(id, cancellationToken);

        recurso.Inativar();

        await _repositorio.AtualizarAsync(recurso, cancellationToken);
        return RecursoDto.De(recurso);
    }

    private async Task GarantirNomeLivreAsync(string nome, int? idAtual, CancellationToken cancellationToken)
    {
        var existente = await _repositorio.ObterPorNomeAsync(nome, cancellationToken);
        if (existente is not null && existente.Id != idAtual)
            throw new DomainException($"Já existe um recurso com o nome {nome}.");
    }

    private async Task<Recurso> ObterOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _repositorio.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Recurso {id} não encontrado.");
}
