using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência de dados da entidade [CancelledMatch].
    /// 
    /// Esta classe implementa o contrato [ICancelledMatchRepository] e utiliza o contexto do Entity Framework Core
    /// ([AmateurFootballContext]) para interagir com a tabela 'CancelledMatch' na base de dados.
    /// </summary>
    public class CancelledMatchRepository : ICancelledMatchRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder às tabelas e executar operações de consulta/escrita.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [CancelledMatchRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public CancelledMatchRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adiciona um novo registo de partida cancelada à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="cancelledMatch">A entidade [CancelledMatch] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddCancelledMatch(CancelledMatch cancelledMatch)
        {
            await context.CancelledMatch.AddAsync(cancelledMatch);
        }
    }
}