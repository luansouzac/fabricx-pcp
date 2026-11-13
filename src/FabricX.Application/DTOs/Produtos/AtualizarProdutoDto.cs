using System.ComponentModel.DataAnnotations;

namespace FabricX.Application.DTOs.Produtos;

public class AtualizarProdutoDto
{
    [Required, StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [Required, StringLength(10)]
    public string UnidadeMedida { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public double EstoqueMinimo { get; set; }

    [Range(0, double.MaxValue)]
    public double CustoUnitario { get; set; }
}
