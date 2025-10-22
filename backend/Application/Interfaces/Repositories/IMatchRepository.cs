using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Domain.Entities;

namespace Application.Interfaces.Repositorys
{
    public interface IMatchRepository
    {
        public Task AddMatch(Matches match);

        public Task<Matches?> GetMatchById(Guid idMatch);

        public Task<Matches?> GetMatchValideToCancelById(Guid idMatch);
        public Task<Matches?> GetMatchWitchPitchById(Guid idMatch);

        public Task<Matches?> GetMatchProxim12HoursMatchs(Guid idReceiver, DateTime gameDate);

        //Criar também um com filtros
        public Task<List<MatchDto>> GetAllMatchesTeam(Guid idTeam);

        public Task<List<InfoPostPoneMatch>> GetAllMatchPostPoneReceiverById(Guid idReceiver);
    }
}
