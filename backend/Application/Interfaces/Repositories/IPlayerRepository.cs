using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IPlayerRepository
    {
        Task<List<Player>?> GetAllTPlayersAsync();

        Task<Player?> GetPlayerByIdAsync(Guid id);

        Task<Player?> GetPlayerByEmailAsync(string email);

        Task<Player?> GetPlayerByPhoneAsync(string phone);

        void DeletePlayer(Player playerToRemove);

        void UpdatePlayer(Player updatedPlayer);

        Task<Player?> GetPlayerByIdWithRequestsAsync(Guid id);

        Task AddAsync(Player player);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsDtoAsync(Guid playerId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsDtoAsyncWithFilters(Guid playerId, FilterMembershipRequestsPlayer filters);
    }
}