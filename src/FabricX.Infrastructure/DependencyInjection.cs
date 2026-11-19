using FabricX.Application.Interfaces;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FabricX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IMovimentacaoEstoqueRepository, MovimentacaoEstoqueRepository>();
        services.AddScoped<IItemEstruturaRepository, ItemEstruturaRepository>();
        return services;
    }
}
