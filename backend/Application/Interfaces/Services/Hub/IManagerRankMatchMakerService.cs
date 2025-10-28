using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Services.Hub
{
    public interface IManagerRankMatchMakerService
    {
        public Task<InfoTeamRankMatchMakerDto> JoinRankMatchMaker(Guid idPlayer, Guid idTeam, string connectionId);
        public Task<bool> LeaveRankMatchMakerAsync(Guid teamId, string connectionId);
        public Task<bool> HandleDisconnectAsync(Guid? maybeTeamId, string connectionId);
    }
}
