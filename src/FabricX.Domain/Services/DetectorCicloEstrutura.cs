using FabricX.Domain.Entities;

namespace FabricX.Domain.Services;

public static class DetectorCicloEstrutura
{
    // Devolve o caminho de produtos [pai, componente, ..., pai] se ligar componente ao pai fechar um ciclo; senão, null.
    public static IReadOnlyList<int>? CaminhoDoCiclo(int produtoPaiId, int componenteId, IEnumerable<ItemEstrutura> itensExistentes)
    {
        var filhosPorPai = itensExistentes
            .GroupBy(i => i.ProdutoPaiId)
            .ToDictionary(g => g.Key, g => g.Select(i => i.ComponenteId).ToList());

        var caminho = new List<int> { componenteId };
        var visitados = new HashSet<int> { componenteId };

        if (!Buscar(componenteId, produtoPaiId, filhosPorPai, visitados, caminho))
            return null;

        caminho.Insert(0, produtoPaiId);
        return caminho;
    }

    private static bool Buscar(int atual, int alvo, Dictionary<int, List<int>> filhosPorPai, HashSet<int> visitados, List<int> caminho)
    {
        if (atual == alvo)
            return true;

        if (!filhosPorPai.TryGetValue(atual, out var filhos))
            return false;

        foreach (var filho in filhos)
        {
            if (!visitados.Add(filho))
                continue;

            caminho.Add(filho);
            if (Buscar(filho, alvo, filhosPorPai, visitados, caminho))
                return true;

            caminho.RemoveAt(caminho.Count - 1);
        }

        return false;
    }
}
