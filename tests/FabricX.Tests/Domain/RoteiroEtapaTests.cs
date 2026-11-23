using FabricX.Domain.Entities;
using FabricX.Domain.Exceptions;

namespace FabricX.Tests.Domain;

public class RoteiroEtapaTests
{
    private static RoteiroEtapa Etapa(double ciclo = 2, int unidades = 1) => new(1, 1, 1, "Montagem", ciclo, unidades);

    [Fact]
    public void Construtor_DadosValidos_PreencheCampos()
    {
        var etapa = new RoteiroEtapa(3, 4, 2, "  Montagem ", 2.5, 10);

        Assert.Equal(3, etapa.ProdutoId);
        Assert.Equal(4, etapa.RecursoId);
        Assert.Equal(2, etapa.Sequencia);
        Assert.Equal("Montagem", etapa.Nome);
        Assert.Equal(2.5, etapa.TempoCicloMin);
        Assert.Equal(10, etapa.UnidadesPorCiclo);
    }

    [Fact]
    public void Construtor_SemUnidadesPorCiclo_AssumeUmaUnidade()
    {
        Assert.Equal(1, new RoteiroEtapa(1, 1, 1, "Etapa", 2).UnidadesPorCiclo);
    }

    [Theory]
    [InlineData(0, 1, 1, 1, 2)]
    [InlineData(1, 0, 1, 1, 2)]
    [InlineData(1, 1, 0, 1, 2)]
    [InlineData(1, 1, 1, 0, 2)]
    [InlineData(1, 1, 1, -1, 2)]
    [InlineData(1, 1, 1, double.NaN, 2)]
    [InlineData(1, 1, 1, 1, 0)]
    public void Construtor_ValorInvalido_LancaDomainException(int produto, int recurso, int sequencia, double ciclo, int unidades)
    {
        Assert.Throws<DomainException>(() => new RoteiroEtapa(produto, recurso, sequencia, "Etapa", ciclo, unidades));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Construtor_NomeVazio_LancaDomainException(string nome)
    {
        Assert.Throws<DomainException>(() => new RoteiroEtapa(1, 1, 1, nome, 2));
    }

    [Theory]
    [InlineData(60, 2, 10, 12)]
    [InlineData(10, 2, 1, 20)]
    [InlineData(25, 2, 10, 6)]
    [InlineData(1, 5, 10, 5)]
    [InlineData(20, 3, 10, 6)]
    public void CalcularDuracaoMinutos_UsaTetoDosCiclos(double quantidade, double ciclo, int unidades, double esperado)
    {
        Assert.Equal(esperado, Etapa(ciclo, unidades).CalcularDuracaoMinutos(quantidade));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void CalcularDuracaoMinutos_QuantidadeInvalida_LancaDomainException(double quantidade)
    {
        Assert.Throws<DomainException>(() => Etapa().CalcularDuracaoMinutos(quantidade));
    }

    [Fact]
    public void Atualizar_ValorInvalido_LancaDomainExceptionEMantemEstado()
    {
        var etapa = Etapa(2, 10);

        Assert.Throws<DomainException>(() => etapa.Atualizar(1, 1, "Etapa", 0, 10));

        Assert.Equal(2, etapa.TempoCicloMin);
    }
}
