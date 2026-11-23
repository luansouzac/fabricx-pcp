using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Domain.Services;
using FabricX.Domain.ValueObjects;

namespace FabricX.Tests.Domain;

public class PeriodoMensalTests
{
    [Theory]
    [InlineData("2026-12", 2026, 12, 31)]
    [InlineData("2026-02", 2026, 2, 28)]
    [InlineData("2028-02", 2028, 2, 29)]
    [InlineData(" 2026-04 ", 2026, 4, 30)]
    public void Parse_PeriodoValido_CalculaDiasDoMes(string texto, int ano, int mes, int dias)
    {
        var periodo = PeriodoMensal.Parse(texto);

        Assert.Equal(ano, periodo.Ano);
        Assert.Equal(mes, periodo.Mes);
        Assert.Equal(dias, periodo.Dias);
        Assert.Equal(new DateTime(ano, mes, 1), periodo.Inicio);
        Assert.Equal(new DateTime(ano, mes, 1).AddMonths(1), periodo.Fim);
    }

    [Theory]
    [InlineData("")]
    [InlineData("2026")]
    [InlineData("2026-13")]
    [InlineData("2026-00")]
    [InlineData("26-12")]
    [InlineData("2026-1")]
    [InlineData("dezembro")]
    [InlineData(null)]
    public void Parse_PeriodoInvalido_LancaDomainException(string? texto)
    {
        Assert.Throws<DomainException>(() => PeriodoMensal.Parse(texto));
    }

    [Fact]
    public void ToString_FormataAnoEMes()
    {
        Assert.Equal("2026-03", PeriodoMensal.Parse("2026-03").ToString());
    }
}

public class DemandaPrevistaTests
{
    [Fact]
    public void Construtor_DadosValidos_PreencheCampos()
    {
        var demanda = new DemandaPrevista(3, "2026-12", 600);

        Assert.Equal(3, demanda.ProdutoId);
        Assert.Equal("2026-12", demanda.Periodo);
        Assert.Equal(600, demanda.Quantidade);
    }

    [Theory]
    [InlineData(0, "2026-12", 1)]
    [InlineData(1, "2026-13", 1)]
    [InlineData(1, "2026-12", 0)]
    [InlineData(1, "2026-12", -5)]
    [InlineData(1, "2026-12", double.NaN)]
    public void Construtor_Invalido_LancaDomainException(int produto, string periodo, double quantidade)
    {
        Assert.Throws<DomainException>(() => new DemandaPrevista(produto, periodo, quantidade));
    }

    [Fact]
    public void Atualizar_Invalido_MantemEstado()
    {
        var demanda = new DemandaPrevista(1, "2026-12", 10);

        Assert.Throws<DomainException>(() => demanda.Atualizar("2027-01", 0));

        Assert.Equal("2026-12", demanda.Periodo);
        Assert.Equal(10, demanda.Quantidade);
    }
}

public class CalculadoraDeCapacidadeTests
{
    private static readonly PeriodoMensal Dezembro = PeriodoMensal.Parse("2026-12");

    private static Recurso NovoRecurso(int id, double horasDia = 8)
    {
        var recurso = new Recurso($"R{id}", TipoRecurso.Maquina, horasDia);
        typeof(Recurso).GetProperty("Id")!.SetValue(recurso, id);
        return recurso;
    }

    private static Programacao Prog(int recursoId, DateTime inicio, DateTime fim) => new(1, 1, recursoId, inicio, fim);

    private static CargaDoRecurso Carga(Recurso recurso, IReadOnlyList<Programacao> programacoes, IReadOnlyList<(DemandaPrevista, IReadOnlyList<RoteiroEtapa>)> demandas) =>
        CalculadoraDeCapacidade.Calcular(Dezembro, [recurso], programacoes, demandas).Single();

    [Fact]
    public void Capacidade_EhHorasPorDiaVezesDiasDoMes()
    {
        var carga = Carga(NovoRecurso(1, 8), [], []);

        Assert.Equal(248, carga.CapacidadeHorasPeriodo);
        Assert.Equal(0, carga.PercentualCarga);
        Assert.Equal(ClassificacaoCarga.Ocioso, carga.Classificacao);
    }

    [Fact]
    public void HorasProgramadas_SoContamOsMinutosDentroDoMes()
    {
        var recurso = NovoRecurso(1);
        var programacoes = new[]
        {
            Prog(1, new DateTime(2026, 11, 30, 22, 0, 0), new DateTime(2026, 12, 1, 2, 0, 0)),
            Prog(1, new DateTime(2026, 12, 10, 8, 0, 0), new DateTime(2026, 12, 10, 12, 0, 0)),
            Prog(2, new DateTime(2026, 12, 10, 8, 0, 0), new DateTime(2026, 12, 10, 18, 0, 0))
        };

        var carga = Carga(recurso, programacoes, []);

        Assert.Equal(6, carga.HorasProgramadas);
    }

    [Fact]
    public void HorasNecessarias_SomamAsEtapasDoRoteiroNoRecursoComTetoDosCiclos()
    {
        var recurso = NovoRecurso(1);
        var roteiro = new[]
        {
            new RoteiroEtapa(1, 1, 1, "Montagem", 2, 10),
            new RoteiroEtapa(1, 2, 2, "Outra", 5, 1)
        };
        var demanda = new DemandaPrevista(1, "2026-12", 25);

        var carga = Carga(recurso, [], [(demanda, roteiro)]);

        Assert.Equal(6.0 / 60, carga.HorasNecessariasDemanda, 10);
    }

    [Theory]
    [InlineData(100, ClassificacaoCarga.Normal)]
    [InlineData(101, ClassificacaoCarga.Gargalo)]
    [InlineData(50, ClassificacaoCarga.Normal)]
    [InlineData(49.9, ClassificacaoCarga.Ocioso)]
    public void Classificacao_RespeitaOsLimitesDe50E100Porcento(double percentual, ClassificacaoCarga esperado)
    {
        var recurso = NovoRecurso(1, 10);
        var carga = new CargaDoRecurso(recurso, 100, percentual, 0);

        Assert.Equal(esperado, carga.Classificacao);
    }

    [Fact]
    public void Classificacao_DemandaAcimaDaCapacidade_EhGargaloMesmoSemProgramacao()
    {
        var recurso = NovoRecurso(1);
        var roteiro = new[] { new RoteiroEtapa(1, 1, 1, "Embalagem", 2, 1) };
        var demanda = new DemandaPrevista(1, "2026-12", 8000);

        var carga = Carga(recurso, [], [(demanda, roteiro)]);

        Assert.Equal(16000.0 / 60, carga.HorasNecessariasDemanda, 10);
        Assert.Equal(0, carga.PercentualProgramado);
        Assert.True(carga.PercentualDemanda > 100);
        Assert.Equal(ClassificacaoCarga.Gargalo, carga.Classificacao);
    }
}
