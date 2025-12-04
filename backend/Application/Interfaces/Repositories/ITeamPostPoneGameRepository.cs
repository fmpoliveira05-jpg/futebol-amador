using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [PostPoneMatch] (Pedido de Adiamento).
    /// 
    /// Define os métodos de persistência e consulta para gerir o ciclo de vida dos pedidos de remarcação de jogos.
    /// </summary>
    public interface ITeamPostPoneGameRepository
    {
        /// <summary>
        /// Adiciona um novo registo de pedido de adiamento à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="postPoneMatch">A entidade [PostPoneMatch] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public Task AddTeamPostPoneMatch(PostPoneMatch postPoneMatch);

        /// <summary>
        /// Marca um registo de pedido de adiamento existente para ser removido da base de dados.
        /// </summary>
        /// <param name="postPoneMatch">A entidade [PostPoneMatch] a ser removida (ex: após aceitação ou rejeição).</param>
        public void RemoveTeamPostPoneMatch(PostPoneMatch postPoneMatch);

        /// <summary>
        /// Obtém um pedido de adiamento específico, carregando a entidade de navegação [Match] e [Pitch] (Local de Jogo).
        /// </summary>
        /// <remarks>
        /// Utilizado para carregar os dados completos do pedido, incluindo o local proposto para o jogo.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que fez o pedido.</param>
        /// <param name="idMatch">O ID da partida que está a ser adiada.</param>
        /// <returns>A entidade [PostPoneMatch] com Pitch carregado, ou null.</returns>
        public Task<PostPoneMatch?> GetTeamPostPoneMatchWithPitch(Guid idTeam, Guid idMatch);

        /// <summary>
        /// Obtém um pedido de adiamento específico com base na Equipa Remetente e na Partida Alvo (sem carregamento profundo).
        /// </summary>
        /// <param name="idTeam">O ID da equipa que fez o pedido.</param>
        /// <param name="idMatch">O ID da partida que está a ser adiada.</param>
        /// <returns>A entidade [PostPoneMatch] ou null se não existir.</returns>
        public Task<PostPoneMatch?> GetTeamPostPoneMatch(Guid idTeam, Guid idMatch);
    }
}