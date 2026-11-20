using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class OrdemProducaoConfiguration : IEntityTypeConfiguration<OrdemProducao>
{
    public void Configure(EntityTypeBuilder<OrdemProducao> builder)
    {
        builder.ToTable("ordem_producao");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.Numero).HasColumnName("numero").IsRequired();
        builder.Property(o => o.ProdutoId).HasColumnName("produto_id");
        builder.Property(o => o.Quantidade).HasColumnName("quantidade");
        builder.Property(o => o.PrazoEntrega).HasColumnName("prazo_entrega");
        builder.Property(o => o.Prioridade).HasColumnName("prioridade").HasConversion(new EnumTextoConverter<Prioridade>()).IsRequired();
        builder.Property(o => o.Status).HasColumnName("status").HasConversion(new EnumTextoConverter<StatusOrdem>()).IsRequired();
        builder.Property(o => o.CriadaEm).HasColumnName("criada_em");
        builder.Property(o => o.ConcluidaEm).HasColumnName("concluida_em");

        builder.HasIndex(o => o.Numero).IsUnique();
    }
}
