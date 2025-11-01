using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AmateurFootballContext context;

        public UserRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task<List<Users?>> GetAllUsersAsync()
        {
            return await context.User.ToListAsync();
        }

        public async Task<Users?> GetUserByIdAsync(Guid id)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Users?> GetUserByPhoneAsync(string phone)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Phone == phone);
        }
    }
}