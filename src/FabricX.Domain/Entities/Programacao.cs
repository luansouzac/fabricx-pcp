using FabricX.Domain.Enums;
using FabricX.Domain.Exceptions;

namespace FabricX.Domain.Entities;

public class Programacao
{
    public int Id { get; private set; }
    public int OrdemId { get; private set; }
    public int EtapaId { get; private set; }
    public int RecursoId { get; private set; }
    public DateTime InicioPrevisto { get; private set; }
    public DateTime FimPrevisto { get; private set; }
    public StatusProgramacao Status { get; private set; }

    private Programacao()
    {
    }

    public Programacao(int ordemId, int etapaId, int recursoId, DateTime inicioPrevisto, DateTime fimPrevisto)
    {
        if (ordemId <= 0)
            throw new DomainException("A ordem da programação é obrigatória.");

        if (etapaId <= 0)
            throw new DomainException("A etapa da programação é obrigatória.");

        OrdemId = ordemId;
        EtapaId = etapaId;
        Status = StatusProgramacao.Agendada;
        DefinirAgenda(recursoId, inicioPrevisto, fimPrevisto);
    }

    public void Reprogramar(int recursoId, DateTime inicioPrevisto, DateTime fimPrevisto)
    {
        GarantirAgendada("reprogramada");
        DefinirAgenda(recursoId, inicioPrevisto, fimPrevisto);
    }

    public void GarantirPodeSerReprogramada() => GarantirAgendada("reprogramada");

    public void GarantirPodeSerRemovida() => GarantirAgendada("removida");

    public void Iniciar()
    {
        if (Status != StatusProgramacao.Agendada)
            throw new DomainException("Só uma programação agendada pode ser iniciada.");

        Status = StatusProgramacao.EmAndamento;
    }

    public void Concluir()
    {
        if (Status == StatusProgramacao.Concluida)
            throw new DomainException("A programação já foi concluída.");

        Status = StatusProgramacao.Concluida;
    }

    private void GarantirAgendada(string acao)
    {
        if (Status != StatusProgramacao.Agendada)
            throw new DomainException($"Só uma programação agendada pode ser {acao}.");
    }

    private void DefinirAgenda(int recursoId, DateTime inicioPrevisto, DateTime fimPrevisto)
    {
        if (recursoId <= 0)
            throw new DomainException("O recurso da programação é obrigatório.");

        if (fimPrevisto <= inicioPrevisto)
            throw new DomainException("O fim previsto deve ser depois do início previsto.");

        RecursoId = recursoId;
        InicioPrevisto = inicioPrevisto;
        FimPrevisto = fimPrevisto;
    }
}
