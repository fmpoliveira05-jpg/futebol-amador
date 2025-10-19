using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace Application.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        Task<List<Teams>?> GetAllTeamsAsync();
        Task<Teams?> GetTeamByIdAsync(Guid id);
        Task<Teams?> GetTeamByNameAsync(string name);
        void DeleteTeam(Teams teamToRemove);

        void UpdateTeam(Teams updatedTeam);

        Task AddAsync(Teams team);

    }
}
