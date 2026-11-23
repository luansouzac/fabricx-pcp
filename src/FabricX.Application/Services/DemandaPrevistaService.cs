using FabricX.Application.DTOs.Demanda;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;
using FabricX.Domain.ValueObjects;

namespace FabricX.Application.Services;

public class DemandaPrevistaService : IDemandaPrevistaService
{
    private readonly IDemandaPrevistaRepository _demandas;
    private readonly IProdutoRepository _produtos;

    public DemandaPrevistaService(IDemandaPrevistaRepository demandas, IProdutoRepository produtos)
    {
        _demandas = demandas;
        _produtos = produtos;
    }

    public async Task<IReadOnlyList<DemandaPrevistaDto>> ListarAsync(string? periodo, CancellationToken cancellationToken)
    {
        var filtro = periodo is null ? null : PeriodoMensal.Parse(periodo).ToString();

        var demandas = await _demandas.ListarAsync(filtro, cancellationToken);
        var produtos = (await _produtos.ListarAsync(null, cancellationToken)).ToDictionary(p => p.Id);

        return demandas.Select(d => DemandaPrevistaDto.De(d, produtos[d.ProdutoId])).ToList();
    }

    public async Task<DemandaPrevistaDto> CriarAsync(CriarDemandaPrevistaDto dto, CancellationToken cancellationToken)
    {
        var produto = await ObterProdutoAtivoOuFalharAsync(dto.ProdutoId, cancellationToken);
        var demanda = new DemandaPrevista(produto.Id, dto.Periodo!, dto.Quantidade);

        await GarantirUnicaAsync(produto, demanda.Periodo, null, cancellationToken);

        await _demandas.AdicionarAsync(demanda, cancellationToken);
        return DemandaPrevistaDto.De(demanda, produto);
    }

    public async Task<DemandaPrevistaDto> AtualizarAsync(int id, AtualizarDemandaPrevistaDto dto, CancellationToken cancellationToken)
    {
        var demanda = await ObterOuFalharAsync(id, cancellationToken);
        var produto = await ObterProdutoAtivoOuFalharAsync(demanda.ProdutoId, cancellationToken);

        await GarantirUnicaAsync(produto, PeriodoMensal.Parse(dto.Periodo).ToString(), demanda.Id, cancellationToken);

        demanda.Atualizar(dto.Periodo!, dto.Quantidade);

        await _demandas.AtualizarAsync(demanda, cancellationToken);
        return DemandaPrevistaDto.De(demanda, produto);
    }

    public async Task<DemandaPrevistaDto> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var demanda = await ObterOuFalharAsync(id, cancellationToken);
        var produto = await _produtos.ObterPorIdAsync(demanda.ProdutoId, cancellationToken)
            ?? throw new NotFoundException($"Produto {demanda.ProdutoId} não encontrado.");

        await _demandas.RemoverAsync(demanda, cancellationToken);
        return DemandaPrevistaDto.De(demanda, produto);
    }

    private async Task GarantirUnicaAsync(Produto produto, string periodo, int? idAtual, CancellationToken cancellationToken)
    {
        var existente = await _demandas.ObterPorProdutoEPeriodoAsync(produto.Id, periodo, cancellationToken);
        if (existente is not null && existente.Id != idAtual)
            throw new DomainException($"Já existe demanda prevista de {produto.Codigo} para {periodo}.");
    }

    private async Task<DemandaPrevista> ObterOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _demandas.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Demanda prevista {id} não encontrada.");

    private async Task<Produto> ObterProdutoAtivoOuFalharAsync(int id, CancellationToken cancellationToken)
    {
        var produto = await _produtos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Produto {id} não encontrado.");

        if (!produto.Ativo)
            throw new DomainException($"O produto {produto.Codigo} está inativo.");

        return produto;
    }
}
