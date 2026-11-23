using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;
using FabricX.Domain.Services;

namespace FabricX.Tests.Domain;

public class ProgramacaoTests
{
    private static readonly DateTime Inicio = new(2026, 12, 1, 8, 0, 0);

    private static Programacao Nova() => new(1, 1, 1, Inicio, Inicio.AddMinutes(12));

    [Fact]
    public void Construtor_DadosValidos_IniciaAgendada()
    {
        var programacao = Nova();

        Assert.Equal(StatusProgramacao.Agendada, programacao.Status);
        Assert.Equal(Inicio.AddMinutes(12), programacao.FimPrevisto);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    public void Construtor_IdsInvalidos_LancaDomainException(int ordem, int etapa, int recurso)
    {
        Assert.Throws<DomainException>(() => new Programacao(ordem, etapa, recurso, Inicio, Inicio.AddHours(1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Construtor_FimNaoPosteriorAoInicio_LancaDomainException(int minutos)
    {
        Assert.Throws<DomainException>(() => new Programacao(1, 1, 1, Inicio, Inicio.AddMinutes(minutos)));
    }

    [Fact]
    public void Reprogramar_Agendada_AlteraAgenda()
    {
        var programacao = Nova();

        programacao.Reprogramar(2, Inicio.AddDays(1), Inicio.AddDays(1).AddHours(1));

        Assert.Equal(2, programacao.RecursoId);
        Assert.Equal(Inicio.AddDays(1), programacao.InicioPrevisto);
    }

    [Fact]
    public void Reprogramar_EmAndamento_LancaDomainException()
    {
        var programacao = Nova();
        programacao.Iniciar();

        Assert.Throws<DomainException>(() => programacao.Reprogramar(1, Inicio, Inicio.AddHours(1)));
        Assert.Throws<DomainException>(() => programacao.GarantirPodeSerRemovida());
    }

    [Fact]
    public void IniciarEConcluir_SeguemOFluxo()
    {
        var programacao = Nova();

        programacao.Iniciar();
        Assert.Equal(StatusProgramacao.EmAndamento, programacao.Status);

        programacao.Concluir();
        Assert.Equal(StatusProgramacao.Concluida, programacao.Status);
        Assert.Throws<DomainException>(() => programacao.Concluir());
        Assert.Throws<DomainException>(() => programacao.Iniciar());
    }
}

public class RegrasDeProgramacaoTests
{
    private static readonly DateTime Dia = new(2026, 12, 1);

    private static Recurso Recurso(double horas = 8)
    {
        var recurso = new Recurso("Montadora", TipoRecurso.Maquina, horas);
        typeof(Recurso).GetProperty("Id")!.SetValue(recurso, 1);
        return recurso;
    }

    private static Programacao Prog(int etapaId, DateTime inicio, DateTime fim, int recursoId = 1) => new(1, etapaId, recursoId, inicio, fim);

    private static RoteiroEtapa Etapa(int id, int sequencia, string nome)
    {
        var etapa = new RoteiroEtapa(1, 1, sequencia, nome, 2);
        typeof(RoteiroEtapa).GetProperty(nameof(RoteiroEtapa.Id))!.SetValue(etapa, id);
        return etapa;
    }

    [Fact]
    public void Sobreposicao_IntervalosQueSeCruzam_LancaDomainException()
    {
        var existentes = new[] { Prog(1, Dia.AddHours(8), Dia.AddHours(10)) };

        var erro = Assert.Throws<DomainException>(() =>
            RegrasDeProgramacao.ConferirSemSobreposicao(Recurso(), Dia.AddHours(9), Dia.AddHours(11), existentes));

        Assert.Contains("já está ocupado", erro.Message);
    }

    [Fact]
    public void Sobreposicao_IntervalosEncostadosNaoConflitam()
    {
        var existentes = new[] { Prog(1, Dia.AddHours(8), Dia.AddHours(10)) };

        RegrasDeProgramacao.ConferirSemSobreposicao(Recurso(), Dia.AddHours(10), Dia.AddHours(12), existentes);
        RegrasDeProgramacao.ConferirSemSobreposicao(Recurso(), Dia.AddHours(6), Dia.AddHours(8), existentes);
    }

    [Fact]
    public void Capacidade_DentroDoLimite_Permite()
    {
        var existentes = new[] { Prog(1, Dia.AddHours(8), Dia.AddHours(12)) };

        RegrasDeProgramacao.ConferirCapacidade(Recurso(8), Dia.AddHours(13), Dia.AddHours(17), existentes);
    }

    [Fact]
    public void Capacidade_EstourandoOLimiteDoDia_LancaDomainException()
    {
        var existentes = new[] { Prog(1, Dia.AddHours(8), Dia.AddHours(12)) };

        var erro = Assert.Throws<DomainException>(() =>
            RegrasDeProgramacao.ConferirCapacidade(Recurso(8), Dia.AddHours(13), Dia.AddHours(17).AddMinutes(1), existentes));

        Assert.Contains("01/12/2026", erro.Message);
        Assert.Contains("capacidade de 8 h", erro.Message);
    }

    [Fact]
    public void Capacidade_IntervaloQueCruzaAMeiaNoite_ContaEmCadaDia()
    {
        var existentes = new[] { Prog(1, Dia.AddHours(8), Dia.AddHours(14)) };

        Assert.Throws<DomainException>(() =>
            RegrasDeProgramacao.ConferirCapacidade(Recurso(8), Dia.AddHours(21), Dia.AddHours(26), existentes));
        RegrasDeProgramacao.ConferirCapacidade(Recurso(8), Dia.AddHours(22), Dia.AddHours(26), existentes);
    }

    [Fact]
    public void Sequencia_PrimeiraEtapa_SemRestricao()
    {
        var roteiro = new[] { Etapa(1, 1, "Montagem"), Etapa(2, 2, "Embalagem") };

        RegrasDeProgramacao.ConferirSequencia(roteiro[0], Dia.AddHours(8), Dia.AddHours(9), roteiro, []);
    }

    [Fact]
    public void Sequencia_EtapaAnteriorNaoProgramada_LancaDomainException()
    {
        var roteiro = new[] { Etapa(1, 1, "Montagem"), Etapa(2, 2, "Embalagem") };

        var erro = Assert.Throws<DomainException>(() =>
            RegrasDeProgramacao.ConferirSequencia(roteiro[1], Dia.AddHours(10), Dia.AddHours(11), roteiro, []));

        Assert.Contains("Programe antes a etapa Montagem", erro.Message);
    }

    [Fact]
    public void Sequencia_ComecandoAntesDoFimDaAnterior_LancaDomainException()
    {
        var roteiro = new[] { Etapa(1, 1, "Montagem"), Etapa(2, 2, "Embalagem") };
        var anterior = Prog(1, Dia.AddHours(8), Dia.AddHours(10));

        Assert.Throws<DomainException>(() =>
            RegrasDeProgramacao.ConferirSequencia(roteiro[1], Dia.AddHours(9), Dia.AddHours(11), roteiro, [anterior]));
        RegrasDeProgramacao.ConferirSequencia(roteiro[1], Dia.AddHours(10), Dia.AddHours(11), roteiro, [anterior]);
    }

    [Fact]
    public void Sequencia_TerminandoDepoisDoInicioDaPosteriorJaProgramada_LancaDomainException()
    {
        var roteiro = new[] { Etapa(1, 1, "Montagem"), Etapa(2, 2, "Embalagem") };
        var posterior = Prog(2, Dia.AddHours(10), Dia.AddHours(11));

        Assert.Throws<DomainException>(() =>
            RegrasDeProgramacao.ConferirSequencia(roteiro[0], Dia.AddHours(8), Dia.AddHours(10).AddMinutes(1), roteiro, [posterior]));
    }
}
