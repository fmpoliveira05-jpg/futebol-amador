using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Hubs
{
    public class NotificationValidator : INotificationValidator
    {
        private readonly ITeamRepository teamRepository;
        private readonly IPlayerRepository playerRepository;

        public NotificationValidator(ITeamRepository teamRepository, IPlayerRepository playerRepository)
        {
            this.teamRepository = teamRepository;
            this.playerRepository = playerRepository;
        }

        public async Task ValidateTeamMembershipAsync(Guid teamId, string? userId)
        {
            Team team = await teamRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                throw new ArgumentException("The team does not exist.");
            }

            Player player = await playerRepository.GetPlayerByIdAsync(userId);

            if (player == null)
            {
                throw new ArgumentException("The player does not exist.");
            }

            Player playerExist = team.Members.FirstOrDefault(p => p.Id == player.Id);

            if (playerExist == null)
            {
                throw new ArgumentException("The player does not belong to this team.");
            }
        }
    }
}
