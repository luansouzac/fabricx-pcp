using FabricX.Application.DTOs.Ordens;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Application.Services;

public class OrdemProducaoService : IOrdemProducaoService
{
    private readonly IOrdemProducaoRepository _ordens;
    private readonly IProdutoRepository _produtos;
    private readonly IItemEstruturaRepository _estruturas;
    private readonly IMovimentacaoEstoqueRepository _movimentacoes;

    public OrdemProducaoService(
        IOrdemProducaoRepository ordens,
        IProdutoRepository produtos,
        IItemEstruturaRepository estruturas,
        IMovimentacaoEstoqueRepository movimentacoes)
    {
        _ordens = ordens;
        _produtos = produtos;
        _estruturas = estruturas;
        _movimentacoes = movimentacoes;
    }

    public async Task<IReadOnlyList<OrdemProducaoDto>> ListarAsync(StatusOrdem? status, CancellationToken cancellationToken)
    {
        var ordens = await _ordens.ListarAsync(status, cancellationToken);
        var produtos = (await _produtos.ListarAsync(null, cancellationToken)).ToDictionary(p => p.Id);

        return ordens.Select(o => OrdemProducaoDto.De(o, produtos[o.ProdutoId])).ToList();
    }

    public async Task<OrdemProducaoDto> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await ObterOrdemOuFalharAsync(id, cancellationToken);
        return OrdemProducaoDto.De(ordem, await ObterProdutoOuFalharAsync(ordem.ProdutoId, cancellationToken));
    }

    public async Task<OrdemProducaoDto> CriarAsync(CriarOrdemProducaoDto dto, CancellationToken cancellationToken)
    {
        var produto = await ObterProdutoOuFalharAsync(dto.ProdutoId, cancellationToken);

        if (!produto.Ativo)
            throw new DomainException($"O produto {produto.Codigo} está inativo.");

        var numero = await GerarNumeroAsync(cancellationToken);
        var ordem = new OrdemProducao(numero, produto.Id, dto.Quantidade, dto.PrazoEntrega!.Value, dto.Prioridade);

        await ConferirSaldoDosComponentesAsync(ordem, cancellationToken);

        await _ordens.AdicionarAsync(ordem, cancellationToken);
        return OrdemProducaoDto.De(ordem, produto);
    }

    public async Task<OrdemProducaoDto> AlterarAsync(int id, AlterarOrdemProducaoDto dto, CancellationToken cancellationToken)
    {
        var ordem = await ObterOrdemOuFalharAsync(id, cancellationToken);
        var produto = await ObterProdutoOuFalharAsync(ordem.ProdutoId, cancellationToken);

        ordem.AlterarPlanejamento(dto.Quantidade, dto.PrazoEntrega!.Value, dto.Prioridade);
        await ConferirSaldoDosComponentesAsync(ordem, cancellationToken);

        await _ordens.AtualizarAsync(ordem, cancellationToken);
        return OrdemProducaoDto.De(ordem, produto);
    }

    public async Task<OrdemProducaoDto> AvancarAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await ObterOrdemOuFalharAsync(id, cancellationToken);
        var produto = await ObterProdutoOuFalharAsync(ordem.ProdutoId, cancellationToken);

        switch (ordem.Status)
        {
            case StatusOrdem.Programada:
                await IniciarProducaoAsync(ordem, cancellationToken);
                break;
            case StatusOrdem.EmProducao:
                await ConcluirAsync(ordem, produto, cancellationToken);
                break;
            default:
                ordem.Avancar();
                await _ordens.AtualizarAsync(ordem, cancellationToken);
                break;
        }

        return OrdemProducaoDto.De(ordem, produto);
    }

    public async Task<OrdemProducaoDto> CancelarAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await ObterOrdemOuFalharAsync(id, cancellationToken);
        var produto = await ObterProdutoOuFalharAsync(ordem.ProdutoId, cancellationToken);

        ordem.Cancelar();

        await _ordens.AtualizarAsync(ordem, cancellationToken);
        return OrdemProducaoDto.De(ordem, produto);
    }

    private async Task IniciarProducaoAsync(OrdemProducao ordem, CancellationToken cancellationToken)
    {
        var necessidades = await ConferirSaldoDosComponentesAsync(ordem, cancellationToken);

        ordem.Avancar();

        var movimentacoes = new List<MovimentacaoEstoque>();
        foreach (var (componente, necessario) in necessidades)
        {
            componente.RegistrarSaida(necessario);
            movimentacoes.Add(new MovimentacaoEstoque(componente.Id, TipoMovimentacao.Saida, necessario, $"Consumo na ordem {ordem.Numero}", ordem.Id));
        }

        await _movimentacoes.AdicionarVariasAsync(movimentacoes, cancellationToken);
    }

    private async Task ConcluirAsync(OrdemProducao ordem, Produto produto, CancellationToken cancellationToken)
    {
        ordem.Avancar();
        produto.RegistrarEntrada(ordem.Quantidade);

        var entrada = new MovimentacaoEstoque(produto.Id, TipoMovimentacao.Entrada, ordem.Quantidade, $"Conclusão da ordem {ordem.Numero}", ordem.Id);
        await _movimentacoes.AdicionarAsync(entrada, cancellationToken);
    }

    private async Task<IReadOnlyList<(Produto Componente, double Necessario)>> ConferirSaldoDosComponentesAsync(OrdemProducao ordem, CancellationToken cancellationToken)
    {
        var necessidades = new List<(Produto, double)>();

        foreach (var item in await _estruturas.ListarPorPaiAsync(ordem.ProdutoId, cancellationToken))
        {
            var componente = await ObterProdutoOuFalharAsync(item.ComponenteId, cancellationToken);
            var necessario = item.Quantidade * ordem.Quantidade;

            if (necessario > componente.EstoqueAtual)
                throw new DomainException($"Estoque insuficiente de {componente.Codigo}: disponível {componente.EstoqueAtual}, necessário {necessario}.");

            necessidades.Add((componente, necessario));
        }

        return necessidades;
    }

    private async Task<string> GerarNumeroAsync(CancellationToken cancellationToken)
    {
        var ano = DateTime.Now.Year;
        var proximo = await _ordens.ObterMaiorSequencialDoAnoAsync(ano, cancellationToken) + 1;
        return $"OP-{ano}-{proximo:0000}";
    }

    private async Task<OrdemProducao> ObterOrdemOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _ordens.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Ordem de produção {id} não encontrada.");

    private async Task<Produto> ObterProdutoOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _produtos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Produto {id} não encontrado.");
}
