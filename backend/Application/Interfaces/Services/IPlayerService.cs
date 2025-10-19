using Application.DTOs.PlayerDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    internal interface IPlayerService
    {
        Task<Guid> CreatePlayerAsync(CreatePlayerDTO playerDTO);

        Task<PlayerDetailsDTO> GetPlayerByIdAsync(Guid teamId);

        Task UpdatePlayerAsync(Guid playerId, UpdatePlayerDTO dto);

        Task DeletePlayerAsync(Guid playerId);

        Task<String> LeaveTeam(Guid playerId);
    }
}
