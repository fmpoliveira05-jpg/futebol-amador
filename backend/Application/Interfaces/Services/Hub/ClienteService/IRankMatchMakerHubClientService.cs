using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Services.Hub.ClienteService
{
    public interface IRankMatchMakerHubClientService
    {
        public Task InitializeAsync();
        public Task JoinRankMatchMakerAsync(StartSearchDto startSearch);
        public Task LeaveRankMatchMakerAsync();
    }
}
