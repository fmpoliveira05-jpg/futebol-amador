using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato de repositório para a entidade [CancelledMatch].
    /// 
    /// Esta interface é responsável por gerir a adição de registos de jogos que foram cancelados.
    /// </summary>
    public interface ICancelledMatchRepository
    {
        /// <summary>
        /// Adiciona um novo registo de partida cancelada à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="cancelledMatch">A entidade [CancelledMatch] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public Task AddCancelledMatch(CancelledMatch cancelledMatch);
    }
}