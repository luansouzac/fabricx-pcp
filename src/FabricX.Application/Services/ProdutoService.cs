using FabricX.Application.DTOs.Produtos;
using FabricX.Application.Exceptions;
using FabricX.Application.Interfaces;
using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repositorio;
    private readonly IMovimentacaoEstoqueRepository _movimentacoes;

    public ProdutoService(IProdutoRepository repositorio, IMovimentacaoEstoqueRepository movimentacoes)
    {
        _repositorio = repositorio;
        _movimentacoes = movimentacoes;
    }

    public async Task<IReadOnlyList<ProdutoDto>> ListarAsync(TipoProduto? tipo, CancellationToken cancellationToken)
    {
        var produtos = await _repositorio.ListarAsync(tipo, cancellationToken);
        return produtos.Select(ProdutoDto.De).ToList();
    }

    public async Task<IReadOnlyList<ProdutoDto>> ListarComEstoqueBaixoAsync(CancellationToken cancellationToken)
    {
        var produtos = await _repositorio.ListarAsync(null, cancellationToken);
        return produtos
            .Where(p => p.Ativo && p.EstoqueAbaixoDoMinimo)
            .Select(ProdutoDto.De)
            .ToList();
    }

    public async Task<ProdutoDto> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var produto = await ObterOuFalharAsync(id, cancellationToken);
        return ProdutoDto.De(produto);
    }

    public async Task<ProdutoDto> CriarAsync(CriarProdutoDto dto, CancellationToken cancellationToken)
    {
        var codigo = dto.Codigo?.Trim() ?? string.Empty;

        if (await _repositorio.ObterPorCodigoAsync(codigo, cancellationToken) is not null)
            throw new DomainException($"Já existe um produto com o código {codigo}.");

        var produto = new Produto(
            dto.Codigo!,
            dto.Nome,
            dto.Tipo,
            dto.UnidadeMedida,
            dto.EstoqueAtual,
            dto.EstoqueMinimo,
            dto.CustoUnitario,
            dto.Descricao);

        await _repositorio.AdicionarAsync(produto, cancellationToken);
        return ProdutoDto.De(produto);
    }

    public async Task<ProdutoDto> AtualizarAsync(int id, AtualizarProdutoDto dto, CancellationToken cancellationToken)
    {
        var produto = await ObterOuFalharAsync(id, cancellationToken);

        produto.Atualizar(dto.Nome, dto.Descricao, dto.UnidadeMedida, dto.EstoqueMinimo, dto.CustoUnitario);

        await _repositorio.AtualizarAsync(produto, cancellationToken);
        return ProdutoDto.De(produto);
    }

    public async Task<ProdutoDto> MovimentarEstoqueAsync(int id, MovimentarEstoqueDto dto, CancellationToken cancellationToken)
    {
        var produto = await ObterOuFalharAsync(id, cancellationToken);

        produto.Movimentar(dto.Tipo, dto.Quantidade);

        await _movimentacoes.AdicionarAsync(new MovimentacaoEstoque(produto.Id, dto.Tipo, dto.Quantidade, dto.Motivo), cancellationToken);
        return ProdutoDto.De(produto);
    }

    public async Task<IReadOnlyList<MovimentacaoEstoqueDto>> ListarMovimentacoesAsync(int id, CancellationToken cancellationToken)
    {
        await ObterOuFalharAsync(id, cancellationToken);

        var movimentacoes = await _movimentacoes.ListarPorProdutoAsync(id, cancellationToken);
        return movimentacoes.Select(MovimentacaoEstoqueDto.De).ToList();
    }

    public async Task<ProdutoDto> InativarAsync(int id, CancellationToken cancellationToken)
    {
        var produto = await ObterOuFalharAsync(id, cancellationToken);

        produto.Inativar();

        await _repositorio.AtualizarAsync(produto, cancellationToken);
        return ProdutoDto.De(produto);
    }

    private async Task<Produto> ObterOuFalharAsync(int id, CancellationToken cancellationToken)
    {
        return await _repositorio.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Produto {id} não encontrado.");
    }
}
