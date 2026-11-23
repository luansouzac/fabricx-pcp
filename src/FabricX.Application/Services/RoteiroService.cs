using FabricX.Application.DTOs.Roteiro;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;

namespace FabricX.Application.Services;

public class RoteiroService : IRoteiroService
{
    private readonly IRoteiroEtapaRepository _etapas;
    private readonly IProdutoRepository _produtos;
    private readonly IRecursoRepository _recursos;

    public RoteiroService(IRoteiroEtapaRepository etapas, IProdutoRepository produtos, IRecursoRepository recursos)
    {
        _etapas = etapas;
        _produtos = produtos;
        _recursos = recursos;
    }

    public async Task<IReadOnlyList<EtapaDto>> ListarAsync(int produtoId, CancellationToken cancellationToken)
    {
        await ObterProdutoOuFalharAsync(produtoId, cancellationToken);

        var etapas = await _etapas.ListarPorProdutoAsync(produtoId, cancellationToken);
        var recursos = (await _recursos.ListarAsync(cancellationToken)).ToDictionary(r => r.Id);

        return etapas.Select(e => EtapaDto.De(e, recursos[e.RecursoId])).ToList();
    }

    public async Task<EtapaDto> AdicionarAsync(int produtoId, CriarEtapaDto dto, CancellationToken cancellationToken)
    {
        var produto = await ObterProdutoOuFalharAsync(produtoId, cancellationToken);
        var recurso = await ObterRecursoAtivoOuFalharAsync(dto.RecursoId, cancellationToken);

        var etapa = new RoteiroEtapa(produto.Id, recurso.Id, dto.Sequencia, dto.Nome!, dto.TempoCicloMin, dto.UnidadesPorCiclo);
        await GarantirSequenciaLivreAsync(produto, etapa.Sequencia, null, cancellationToken);

        await _etapas.AdicionarAsync(etapa, cancellationToken);
        return EtapaDto.De(etapa, recurso);
    }

    public async Task<EtapaDto> AtualizarAsync(int produtoId, int etapaId, AtualizarEtapaDto dto, CancellationToken cancellationToken)
    {
        var produto = await ObterProdutoOuFalharAsync(produtoId, cancellationToken);
        var etapa = await ObterEtapaOuFalharAsync(produtoId, etapaId, cancellationToken);
        var recurso = await ObterRecursoAtivoOuFalharAsync(dto.RecursoId, cancellationToken);

        etapa.Atualizar(recurso.Id, dto.Sequencia, dto.Nome!, dto.TempoCicloMin, dto.UnidadesPorCiclo);
        await GarantirSequenciaLivreAsync(produto, etapa.Sequencia, etapa.Id, cancellationToken);

        await _etapas.AtualizarAsync(etapa, cancellationToken);
        return EtapaDto.De(etapa, recurso);
    }

    public async Task<EtapaDto> RemoverAsync(int produtoId, int etapaId, CancellationToken cancellationToken)
    {
        var etapa = await ObterEtapaOuFalharAsync(produtoId, etapaId, cancellationToken);
        var recurso = await _recursos.ObterPorIdAsync(etapa.RecursoId, cancellationToken)
            ?? throw new NotFoundException($"Recurso {etapa.RecursoId} não encontrado.");

        await _etapas.RemoverAsync(etapa, cancellationToken);
        return EtapaDto.De(etapa, recurso);
    }

    private async Task GarantirSequenciaLivreAsync(Produto produto, int sequencia, int? etapaAtualId, CancellationToken cancellationToken)
    {
        var existentes = await _etapas.ListarPorProdutoAsync(produto.Id, cancellationToken);
        if (existentes.Any(e => e.Sequencia == sequencia && e.Id != etapaAtualId))
            throw new DomainException($"O produto {produto.Codigo} já tem uma etapa na sequência {sequencia}.");
    }

    private async Task<Produto> ObterProdutoOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _produtos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Produto {id} não encontrado.");

    private async Task<Recurso> ObterRecursoAtivoOuFalharAsync(int id, CancellationToken cancellationToken)
    {
        var recurso = await _recursos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Recurso {id} não encontrado.");

        if (!recurso.Ativo)
            throw new DomainException($"O recurso {recurso.Nome} está inativo.");

        return recurso;
    }

    private async Task<RoteiroEtapa> ObterEtapaOuFalharAsync(int produtoId, int etapaId, CancellationToken cancellationToken)
    {
        var etapa = await _etapas.ObterPorIdAsync(etapaId, cancellationToken);
        if (etapa is null || etapa.ProdutoId != produtoId)
            throw new NotFoundException($"Etapa {etapaId} não encontrada no roteiro do produto {produtoId}.");

        return etapa;
    }
}
