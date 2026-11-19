using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FabricX.Infrastructure.Persistence.Configurations;

public class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.ToTable("movimentacao_estoque");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.ProdutoId).HasColumnName("produto_id");
        builder.Property(m => m.OrdemId).HasColumnName("ordem_id");
        builder.Property(m => m.Tipo).HasColumnName("tipo").HasConversion(new EnumTextoConverter<TipoMovimentacao>()).IsRequired();
        builder.Property(m => m.Quantidade).HasColumnName("quantidade");
        builder.Property(m => m.Motivo).HasColumnName("motivo");
        builder.Property(m => m.DataHora).HasColumnName("data_hora");
    }
}
