using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class ProgramacaoConfiguration : IEntityTypeConfiguration<Programacao>
{
    public void Configure(EntityTypeBuilder<Programacao> builder)
    {
        builder.ToTable("programacao");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.OrdemId).HasColumnName("ordem_id");
        builder.Property(p => p.EtapaId).HasColumnName("etapa_id");
        builder.Property(p => p.RecursoId).HasColumnName("recurso_id");
        builder.Property(p => p.InicioPrevisto).HasColumnName("inicio_previsto");
        builder.Property(p => p.FimPrevisto).HasColumnName("fim_previsto");
        builder.Property(p => p.Status).HasColumnName("status").HasConversion(new EnumTextoConverter<StatusProgramacao>()).IsRequired();

        builder.HasIndex(p => new { p.OrdemId, p.EtapaId }).IsUnique();
    }
}
