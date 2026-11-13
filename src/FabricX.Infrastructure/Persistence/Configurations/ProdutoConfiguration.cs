using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produto");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Codigo).HasColumnName("codigo").IsRequired();
        builder.Property(p => p.Nome).HasColumnName("nome").IsRequired();
        builder.Property(p => p.Descricao).HasColumnName("descricao");
        builder.Property(p => p.Tipo).HasColumnName("tipo").HasConversion(new EnumTextoConverter<TipoProduto>()).IsRequired();
        builder.Property(p => p.UnidadeMedida).HasColumnName("unidade_medida").IsRequired();
        builder.Property(p => p.EstoqueAtual).HasColumnName("estoque_atual");
        builder.Property(p => p.EstoqueMinimo).HasColumnName("estoque_minimo");
        builder.Property(p => p.CustoUnitario).HasColumnName("custo_unitario");
        builder.Property(p => p.Ativo).HasColumnName("ativo");

        builder.Ignore(p => p.EstoqueAbaixoDoMinimo);
        builder.HasIndex(p => p.Codigo).IsUnique();
    }
}
