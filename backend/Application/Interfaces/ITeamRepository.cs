using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace Application.Interfaces
{
    public interface ITeamRepository
    {
        Task<List<Teams>?> GetAllTeamsAsync();
        Task<Teams?> GetTeamById(Guid id);
        Task<Teams?> GetTeamByName(string name);
        void DeleteTeam(Teams teamToRemove);

        void UpdateTeam(Teams updatedTeam);

        Task AddAsync(Teams team);

    }
}
