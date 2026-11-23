using FabricX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class RoteiroEtapaConfiguration : IEntityTypeConfiguration<RoteiroEtapa>
{
    public void Configure(EntityTypeBuilder<RoteiroEtapa> builder)
    {
        builder.ToTable("roteiro_etapa");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ProdutoId).HasColumnName("produto_id");
        builder.Property(e => e.RecursoId).HasColumnName("recurso_id");
        builder.Property(e => e.Sequencia).HasColumnName("sequencia");
        builder.Property(e => e.Nome).HasColumnName("nome").IsRequired();
        builder.Property(e => e.TempoCicloMin).HasColumnName("tempo_ciclo_min");
        builder.Property(e => e.UnidadesPorCiclo).HasColumnName("unidades_por_ciclo");

        builder.HasIndex(e => new { e.ProdutoId, e.Sequencia }).IsUnique();
    }
}
