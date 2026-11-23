using FabricX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class ItemEstruturaConfiguration : IEntityTypeConfiguration<ItemEstrutura>
{
    public void Configure(EntityTypeBuilder<ItemEstrutura> builder)
    {
        builder.ToTable("item_estrutura");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.ProdutoPaiId).HasColumnName("produto_pai_id");
        builder.Property(i => i.ComponenteId).HasColumnName("componente_id");
        builder.Property(i => i.Quantidade).HasColumnName("quantidade");

        builder.HasIndex(i => new { i.ProdutoPaiId, i.ComponenteId }).IsUnique();
    }
}
