using Application.DTOs.Competition;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>Acesso ao mercado, às propostas e ao histórico de transferências.</summary>
    public interface ITransferRepository
    {
        Task<TransferListing?> GetListingAsync(string playerId);
        Task AddListingAsync(TransferListing listing);
        void RemoveListing(TransferListing listing);
        Task<HashSet<string>> GetListedPlayerIdsAsync();

        /// <summary>Jogadores do mercado (com a equipa e a liga), sem os da equipa <paramref name="excludeTeamId"/>.</summary>
        Task<List<Player>> SearchMarketAsync(MarketFilterDto filter, Guid excludeTeamId, HashSet<string> listed);

        Task AddOfferAsync(TransferOffer offer);

        /// <summary>Proposta com o jogador (e a equipa atual com os membros) e as duas equipas (com os membros).</summary>
        Task<TransferOffer?> GetOfferAsync(Guid id);

        Task<List<TransferOffer>> GetOffersOfTeamAsync(Guid teamId);
        Task<List<TransferOffer>> GetOffersWaitingForPlayerAsync(string playerId);

        /// <summary>Propostas ainda por decidir de um jogador (para anular quando muda de equipa).</summary>
        Task<List<TransferOffer>> GetOpenOffersOfPlayerAsync(string playerId);

        Task<bool> HasOpenOfferAsync(string playerId, Guid toTeamId);
        /// <summary>Apaga as propostas que envolvem a equipa (antes de a apagar).</summary>
        Task RemoveOffersOfTeamAsync(Guid teamId);

        Task AddRecordAsync(TransferRecord record);
        Task<List<TransferRecord>> GetRecordsAsync(string playerId);
    }
}
