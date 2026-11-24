using FabricX.Api.Common;
using FabricX.Application.DTOs.Produtos;
using FabricX.Application.Interfaces;
using FabricX.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FabricX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(IProdutoService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<ProdutoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] TipoProduto? tipo, CancellationToken cancellationToken)
    {
        var produtos = await _service.ListarAsync(tipo, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<ProdutoDto>>.Success(produtos));
    }

    [HttpGet("estoque-baixo")]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<ProdutoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarComEstoqueBaixo(CancellationToken cancellationToken)
    {
        var produtos = await _service.ListarComEstoqueBaixoAsync(cancellationToken);
        return Ok(WebResponse<IReadOnlyList<ProdutoDto>>.Success(produtos));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<ProdutoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var produto = await _service.ObterPorIdAsync(id, cancellationToken);
        return Ok(WebResponse<ProdutoDto>.Success(produto));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WebResponse<ProdutoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarProdutoDto dto, CancellationToken cancellationToken)
    {
        var produto = await _service.CriarAsync(dto, cancellationToken);
        var resposta = WebResponse<ProdutoDto>.Success(produto, StatusCodes.Status201Created);
        return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, resposta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WebResponse<ProdutoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarProdutoDto dto, CancellationToken cancellationToken)
    {
        var produto = await _service.AtualizarAsync(id, dto, cancellationToken);
        return Ok(WebResponse<ProdutoDto>.Success(produto));
    }

    [HttpPost("{id:int}/movimentacoes")]
    [ProducesResponseType(typeof(WebResponse<ProdutoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MovimentarEstoque(int id, [FromBody] MovimentarEstoqueDto dto, CancellationToken cancellationToken)
    {
        var produto = await _service.MovimentarEstoqueAsync(id, dto, cancellationToken);
        return Ok(WebResponse<ProdutoDto>.Success(produto));
    }

    [HttpGet("{id:int}/movimentacoes")]
    [ProducesResponseType(typeof(WebResponse<IReadOnlyList<MovimentacaoEstoqueDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarMovimentacoes(int id, CancellationToken cancellationToken)
    {
        var movimentacoes = await _service.ListarMovimentacoesAsync(id, cancellationToken);
        return Ok(WebResponse<IReadOnlyList<MovimentacaoEstoqueDto>>.Success(movimentacoes));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(WebResponse<ProdutoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(WebResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id, CancellationToken cancellationToken)
    {
        var produto = await _service.InativarAsync(id, cancellationToken);
        return Ok(WebResponse<ProdutoDto>.Success(produto));
    }
}
