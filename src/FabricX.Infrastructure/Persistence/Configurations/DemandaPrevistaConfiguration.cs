using FabricX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class DemandaPrevistaConfiguration : IEntityTypeConfiguration<DemandaPrevista>
{
    public void Configure(EntityTypeBuilder<DemandaPrevista> builder)
    {
        builder.ToTable("demanda_prevista");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.ProdutoId).HasColumnName("produto_id");
        builder.Property(d => d.Periodo).HasColumnName("periodo").IsRequired();
        builder.Property(d => d.Quantidade).HasColumnName("quantidade");

        builder.HasIndex(d => new { d.ProdutoId, d.Periodo }).IsUnique();
    }
}
