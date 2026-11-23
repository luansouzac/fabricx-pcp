using FabricX.Application.Interfaces;
using FabricX.Infrastructure.Cache;
using FabricX.Infrastructure.Integracoes;
using FabricX.Infrastructure.Persistence;
using FabricX.Infrastructure.Repositories;
using FabricX.Infrastructure.Seguranca;
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
        services.AddScoped<IOrdemProducaoRepository, OrdemProducaoRepository>();
        services.AddScoped<IErpGateway, ErpGatewaySimulado>();
        services.AddScoped<ISgqGateway, SgqGatewaySimulado>();
        services.AddScoped<IRecursoRepository, RecursoRepository>();
        services.AddScoped<IProgramacaoRepository, ProgramacaoRepository>();
        services.AddScoped<IApontamentoRepository, ApontamentoRepository>();
        services.AddScoped<IDemandaPrevistaRepository, DemandaPrevistaRepository>();
        services.AddScoped<IRoteiroEtapaRepository, RoteiroEtapaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<ISenhaHasher, SenhaHasherPbkdf2>();
        services.AddSingleton<IGeradorDeToken, GeradorDeTokenJwt>();
        services.AddMemoryCache();
        services.AddSingleton<ICacheDeConsultas, CacheDeConsultasMemoria>();
        return services;
    }
}
