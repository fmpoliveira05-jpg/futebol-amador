using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência de dados e consulta da entidade base [User].
    /// 
    /// Esta classe implementa o contrato [IUserRepository] e é utilizada para carregar dados de utilizadores
    /// de forma agnóstica ao seu tipo (Player, SuperAdmin), usando a tabela base de herança.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder à tabela base de utilizadores.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [UserRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public UserRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Obtém uma lista de todos os utilizadores (SuperAdmin e Player).
        /// </summary>
        /// <returns>Uma lista de todas as entidades [User].</returns>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await context.User.ToListAsync();
        }

        /// <summary>
        /// Obtém um utilizador pelo seu identificador único (ID) de forma assíncrona.
        /// </summary>
        /// <param name="userId">O ID (string) do utilizador a procurar.</param>
        /// <returns>A entidade [User] correspondente ou null.</returns>
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Id == userId);
        }

        /// <summary>
        /// Obtém um utilizador pelo seu endereço de e-mail de forma assíncrona.
        /// </summary>
        /// <param name="email">O endereço de e-mail do utilizador.</param>
        /// <returns>A entidade [User] ou null.</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Email == email);
        }

        /// <summary>
        /// Obtém um utilizador pelo seu número de telefone de forma assíncrona.
        /// </summary>
        /// <param name="phone">O número de telefone completo.</param>
        /// <returns>A entidade [User] ou null.</returns>
        public async Task<User?> GetUserByPhoneAsync(string phone)
        {
            return await context.User.FirstOrDefaultAsync(p => p.Phone == phone);
        }

        /// <summary>
        /// Marca uma entidade [Player] para ser atualizada na base de dados.
        /// </summary>
        /// <param name="updatedPlayer">A entidade [Player] com os novos valores.</param>
        public void UpdatePlayer(Player updatedPlayer)
        {
            throw new NotImplementedException();
        }
    }
}