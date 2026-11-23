namespace FabricX.Application.DTOs.Indicadores;

public class IndicadoresDto
{
    public double? Qualidade { get; init; }
    public double? Eficiencia { get; init; }
    public double? ProdutividadePorHora { get; init; }
    public double? CumprimentoDePrazo { get; init; }
    public double TotalProduzido { get; init; }
    public double TotalDefeituoso { get; init; }
    public double TempoApontadoMin { get; init; }
    public int OrdensConcluidas { get; init; }
}
