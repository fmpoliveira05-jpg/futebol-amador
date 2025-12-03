using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência de dados e consulta da entidade [SuperAdmin].
    /// 
    /// Esta classe é responsável por gerir o ciclo de vida dos utilizadores com privilégios de Super Administrador.
    /// </summary>
    public class SuperAdminRepository : ISuperAdminRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder à tabela de Super Administradores.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [SuperAdminRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public SuperAdminRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adiciona um novo registo de Super Administrador à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="superAdmin">A entidade [SuperAdmin] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddAsync(SuperAdmin superAdmin)
        {
            await context.AddAsync(superAdmin);
        }

        /// <summary>
        /// Marca um Super Administrador existente para ser removido da base de dados.
        /// </summary>
        /// <param name="sadminToRemove">A entidade [SuperAdmin] a ser removida.</param>
        public void DeleteSuperAdmin(SuperAdmin sadminToRemove)
        {
            context.SuperAdmin.Remove(sadminToRemove);
        }

        /// <summary>
        /// Obtém todos os registos de Super Administradores.
        /// </summary>
        /// <returns>Uma lista de todas as entidades [SuperAdmin].</returns>
        public async Task<List<SuperAdmin>> GetAllSuperAdminsAsync()
        {
            return await context.SuperAdmin.ToListAsync();
        }

        /// <summary>
        /// Obtém um Super Administrador pelo seu endereço de e-mail de forma assíncrona.
        /// </summary>
        /// <param name="email">O endereço de e-mail do administrador.</param>
        /// <returns>A entidade [SuperAdmin] ou null.</returns>
        public async Task<SuperAdmin?> GetSuperAdminByEmailAsync(string email)
        {
            return await context.SuperAdmin.FirstOrDefaultAsync(s => s.Email == email);
        }

        /// <summary>
        /// Obtém um Super Administrador pelo seu identificador único (ID) de forma assíncrona.
        /// </summary>
        /// <remarks>
        /// Utiliza o método de pesquisa Find do EF Core, que primeiro verifica o cache de rastreamento.
        /// </remarks>
        /// <param name="sadminId">O ID do Super Administrador (geralmente o UID do Firebase).</param>
        /// <returns>A entidade [SuperAdmin] ou null.</returns>
        public async Task<SuperAdmin?> GetSuperAdminByIdAsync(string sadminId)
        {
            return await context.SuperAdmin.FindAsync(sadminId);
        }

        /// <summary>
        /// Obtém um Super Administrador pelo seu número de telefone de forma assíncrona.
        /// </summary>
        /// <param name="phone">O número de telefone completo do administrador.</param>
        /// <returns>A entidade [SuperAdmin] ou null.</returns>
        public async Task<SuperAdmin?> GetSuperAdminByPhoneAsync(string phone)
        {
            var superAdmin = await context.SuperAdmin.FirstOrDefaultAsync(s => s.Phone == phone);

            return superAdmin;
        }

        /// <summary>
        /// Marca uma entidade [SuperAdmin] existente para ser atualizada na base de dados.
        /// </summary>
        /// <param name="updatedSadmin">A entidade [SuperAdmin] com os novos valores.</param>
        public void UpdateSuperAdmin(SuperAdmin updatedSadmin)
        {
            context.SuperAdmin.Update(updatedSadmin);
        }
    }
}