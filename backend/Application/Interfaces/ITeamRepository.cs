using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace Application.Interfaces
{
    internal interface ITeamRepository
    {
        Task<IEnumerable<Teams>> GetAllTeams();
        Task<IEnumerable<Teams>> GetAllTeamsAsync();

        Task<IEnumerable<Teams>> GetTeamById(int id);
        Task DeleteTeam(int id);

        Task<IEnumerable<Teams>> EditTeam(int id);
        //TESTE 2
        //Teste 3
        Task SaveTeam(Teams teams);

    }
}
