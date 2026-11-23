using FabricX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class ApontamentoConfiguration : IEntityTypeConfiguration<Apontamento>
{
    public void Configure(EntityTypeBuilder<Apontamento> builder)
    {
        builder.ToTable("apontamento");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.ProgramacaoId).HasColumnName("programacao_id");
        builder.Property(a => a.DataHora).HasColumnName("data_hora");
        builder.Property(a => a.QtdProduzida).HasColumnName("qtd_produzida");
        builder.Property(a => a.QtdDefeituosa).HasColumnName("qtd_defeituosa");
        builder.Property(a => a.TempoGastoMin).HasColumnName("tempo_gasto_min");
        builder.Property(a => a.Observacao).HasColumnName("observacao");

        builder.Ignore(a => a.QtdProcessada);
    }
}
