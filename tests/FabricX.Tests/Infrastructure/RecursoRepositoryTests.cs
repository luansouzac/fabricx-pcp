using FabricX.Domain.Entities;
using FabricX.Domain.Enums;
using FabricX.Infrastructure.Repositories;
using FabricX.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace FabricX.Tests.Infrastructure;

public class RecursoRepositoryTests : IDisposable
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private readonly BancoSqliteFixture _banco = new();

    public void Dispose() => _banco.Dispose();

    [Theory]
    [InlineData(TipoRecurso.Maquina, "MAQUINA")]
    [InlineData(TipoRecurso.Operador, "OPERADOR")]
    [InlineData(TipoRecurso.Linha, "LINHA")]
    public async Task Adicionar_GravaTipoComoTextoDoCheck(TipoRecurso tipo, string esperado)
    {
        await using var contexto = _banco.CriarContexto();

        await new RecursoRepository(contexto).AdicionarAsync(new Recurso("R1", tipo, 8), Ct);

        var texto = await contexto.Database.SqlQuery<string>($"SELECT tipo AS Value FROM recurso").SingleAsync(Ct);
        Assert.Equal(esperado, texto);
    }

    [Fact]
    public async Task Adicionar_NomeRepetido_ViolaUnique()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new RecursoRepository(contexto);
        await repositorio.AdicionarAsync(new Recurso("R1", TipoRecurso.Maquina, 8), Ct);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repositorio.AdicionarAsync(new Recurso("R1", TipoRecurso.Linha, 4), Ct));
    }

    [Fact]
    public async Task ObterPorNomeEListar_FuncionamEListaOrdenadaPorNome()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new RecursoRepository(contexto);
        await repositorio.AdicionarAsync(new Recurso("Z", TipoRecurso.Maquina, 8), Ct);
        await repositorio.AdicionarAsync(new Recurso("A", TipoRecurso.Maquina, 8), Ct);

        Assert.NotNull(await repositorio.ObterPorNomeAsync("A", Ct));
        Assert.Null(await repositorio.ObterPorNomeAsync("Q", Ct));
        Assert.Equal(["A", "Z"], (await repositorio.ListarAsync(Ct)).Select(r => r.Nome));
    }

    [Fact]
    public async Task Atualizar_PersisteInativacao()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new RecursoRepository(contexto);
        var recurso = new Recurso("R1", TipoRecurso.Maquina, 8);
        await repositorio.AdicionarAsync(recurso, Ct);

        recurso.Inativar();
        await repositorio.AtualizarAsync(recurso, Ct);

        await using var outro = _banco.CriarContexto();
        Assert.False((await outro.Recursos.SingleAsync(Ct)).Ativo);
    }
}
