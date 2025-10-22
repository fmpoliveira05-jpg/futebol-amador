using Application.Interfaces.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        public Task<Player?> GetPlayerByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
