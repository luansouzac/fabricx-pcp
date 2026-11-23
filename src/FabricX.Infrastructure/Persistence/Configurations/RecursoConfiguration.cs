using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.ToTable("recurso");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Nome).HasColumnName("nome").IsRequired();
        builder.Property(r => r.Tipo).HasColumnName("tipo").HasConversion(new EnumTextoConverter<TipoRecurso>()).IsRequired();
        builder.Property(r => r.CapacidadeHorasDia).HasColumnName("capacidade_horas_dia");
        builder.Property(r => r.Ativo).HasColumnName("ativo");

        builder.HasIndex(r => r.Nome).IsUnique();
    }
}
