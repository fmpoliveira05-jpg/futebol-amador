using Application.DTOs.MatchInvites;
using Domain.Entities;

namespace Application.Interfaces.Repositorys
{
    public interface IMatchInviteRepository
    {
        Task AddMatchInvite(MatchInvite matchInvite);

        Task DeleteMatchInvite(MatchInvite matchInvite);

        Task<MatchInvite?> GetMatchInviteById(Guid id);

        Task<MatchInvite?> GetMatchInviteByTeams(Guid idSender, Guid idReceiver);

        public Task<MatchInvite?> GetMatchInvite(SendMatchInviteDTO dto);

        public Task<List<InfoMatchInviteDTO>> GetAllMatchInviteReceiverById(Guid idReceiver);
    }
}
