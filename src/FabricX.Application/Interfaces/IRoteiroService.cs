using FabricX.Application.DTOs.Roteiro;

namespace FabricX.Application.Interfaces;

public interface IRoteiroService
{
    Task<IReadOnlyList<EtapaDto>> ListarAsync(int produtoId, CancellationToken cancellationToken);
    Task<EtapaDto> AdicionarAsync(int produtoId, CriarEtapaDto dto, CancellationToken cancellationToken);
    Task<EtapaDto> AtualizarAsync(int produtoId, int etapaId, AtualizarEtapaDto dto, CancellationToken cancellationToken);
    Task<EtapaDto> RemoverAsync(int produtoId, int etapaId, CancellationToken cancellationToken);
}
