using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    internal interface ITeamRepository
    {
        Task<IEnumerable<string>> GetAll();
        Task<IEnumerable<string>> GetAllAsync();
        Task<IEnumerable<Teams>> GetTeamByID(string  teamID);

    }
}
