using FabricX.Application.DTOs.Estrutura;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;
using FabricX.Domain.Services;

namespace FabricX.Application.Services;

public class ItemEstruturaService : IItemEstruturaService
{
    private readonly IItemEstruturaRepository _itens;
    private readonly IProdutoRepository _produtos;

    public ItemEstruturaService(IItemEstruturaRepository itens, IProdutoRepository produtos)
    {
        _itens = itens;
        _produtos = produtos;
    }

    public async Task<IReadOnlyList<ItemEstruturaDto>> ListarAsync(int produtoPaiId, CancellationToken cancellationToken)
    {
        await ObterProdutoOuFalharAsync(produtoPaiId, cancellationToken);

        var itens = await _itens.ListarPorPaiAsync(produtoPaiId, cancellationToken);
        var porId = (await _produtos.ListarAsync(null, cancellationToken)).ToDictionary(p => p.Id);

        return itens.Select(i => ItemEstruturaDto.De(i, porId[i.ComponenteId])).ToList();
    }

    public async Task<ItemEstruturaDto> AdicionarAsync(int produtoPaiId, AdicionarItemEstruturaDto dto, CancellationToken cancellationToken)
    {
        var pai = await ObterProdutoOuFalharAsync(produtoPaiId, cancellationToken);
        var componente = await ObterProdutoOuFalharAsync(dto.ComponenteId, cancellationToken);

        if (!pai.Ativo)
            throw new DomainException($"O produto {pai.Codigo} está inativo.");

        if (!componente.Ativo)
            throw new DomainException($"O produto {componente.Codigo} está inativo.");

        var item = new ItemEstrutura(pai.Id, componente.Id, dto.Quantidade);

        if (await _itens.ObterAsync(pai.Id, componente.Id, cancellationToken) is not null)
            throw new DomainException($"{componente.Codigo} já é componente de {pai.Codigo}.");

        var caminho = DetectorCicloEstrutura.CaminhoDoCiclo(pai.Id, componente.Id, await _itens.ListarTodosAsync(cancellationToken));
        if (caminho is not null)
            throw new DomainException($"A estrutura formaria um ciclo: {await MontarCaminhoAsync(caminho, cancellationToken)}");

        await _itens.AdicionarAsync(item, cancellationToken);
        return ItemEstruturaDto.De(item, componente);
    }

    public async Task<ItemEstruturaDto> AlterarQuantidadeAsync(int produtoPaiId, int componenteId, AlterarQuantidadeItemEstruturaDto dto, CancellationToken cancellationToken)
    {
        var item = await ObterItemOuFalharAsync(produtoPaiId, componenteId, cancellationToken);
        var componente = await ObterProdutoOuFalharAsync(componenteId, cancellationToken);

        item.AlterarQuantidade(dto.Quantidade);

        await _itens.AtualizarAsync(item, cancellationToken);
        return ItemEstruturaDto.De(item, componente);
    }

    public async Task<ItemEstruturaDto> RemoverAsync(int produtoPaiId, int componenteId, CancellationToken cancellationToken)
    {
        var item = await ObterItemOuFalharAsync(produtoPaiId, componenteId, cancellationToken);
        var componente = await ObterProdutoOuFalharAsync(componenteId, cancellationToken);

        await _itens.RemoverAsync(item, cancellationToken);
        return ItemEstruturaDto.De(item, componente);
    }

    private async Task<string> MontarCaminhoAsync(IReadOnlyList<int> caminho, CancellationToken cancellationToken)
    {
        var porId = (await _produtos.ListarAsync(null, cancellationToken)).ToDictionary(p => p.Id);
        return string.Join(" → ", caminho.Select(id => porId[id].Nome));
    }

    private async Task<Produto> ObterProdutoOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _produtos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Produto {id} não encontrado.");

    private async Task<ItemEstrutura> ObterItemOuFalharAsync(int produtoPaiId, int componenteId, CancellationToken cancellationToken) =>
        await _itens.ObterAsync(produtoPaiId, componenteId, cancellationToken)
            ?? throw new NotFoundException($"O produto {componenteId} não é componente do produto {produtoPaiId}.");
}
