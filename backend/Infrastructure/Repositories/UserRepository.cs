using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly AmateurFootballContext _context;

    public UserRepository(AmateurFootballContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetUserByEmailAsync(string email)
    {
        return await _context.Player.FirstOrDefaultAsync(p => p.Email.ToUpper() == email.ToUpper());
    }

    public async Task<Player?> GetUserByIdAsync(Guid id)
    {
        return await _context.Player.FirstOrDefaultAsync(p => p.Id == id);
    }

    public void UpdateUser(Player player)
    {
        _context.Player.Update(player);
    }
}