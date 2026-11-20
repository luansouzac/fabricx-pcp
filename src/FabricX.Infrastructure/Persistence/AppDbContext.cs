using FabricX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
    public DbSet<ItemEstrutura> ItensEstrutura => Set<ItemEstrutura>();
    public DbSet<OrdemProducao> OrdensProducao => Set<OrdemProducao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
