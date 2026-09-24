using Application.DTOs.Chat;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a gestão de Salas de Chat (Chat Rooms).
    /// 
    /// Esta interface abstrai a lógica de criação e consulta de canais de comunicação, utilizando tipicamente uma base de dados NoSQL (como o Firestore)
    /// para suportar a funcionalidade de mensagens em tempo real.
    /// </summary>
    public interface IChatRoomService
    {
        /// <summary>
        /// Cria uma sala de chat genérica com uma lista explícita de membros (jogadores).
        /// </summary>
        /// <remarks>
        /// Utilizada para criar canais de comunicação ad-hoc entre utilizadores selecionados.
        /// </remarks>
        /// <param name="request">DTO com o nome da sala e a lista de IDs de membros.</param>
        /// <param name="createdByUserId">ID do utilizador que iniciou a criação.</param>
        /// <returns>O ID (string) da nova sala de chat criada.</returns>
        Task<string> CreateRoomAsync(CreateChatRoomDto request, string createdByUserId);

        /// <summary>
        /// Cria uma sala de chat especificamente para uma partida, adicionando os administradores das equipas envolvidas.
        /// </summary>
        /// <remarks>
        /// Utilizada para criar um canal de comunicação dedicado para as equipas que vão disputar um jogo.
        /// O serviço deve, internamente, resolver os IDs de equipa para os IDs dos administradores.
        /// </remarks>
        /// <param name="request">DTO com o nome da sala e IDs das equipas participantes.</param>
        /// <param name="createdByUserId">ID do utilizador que iniciou a criação.</param>
        /// <returns>O ID (string) da nova sala de chat de partida criada.</returns>
        Task<string> CreateMatchRoomAsync(CreateChatRoomRequestDto request, string createdByUserId);

        /// <summary>
        /// Obtém a lista de todas as salas de chat em que um utilizador participa.
        /// </summary>
        /// <param name="userId">O ID do utilizador (o membro) cujas salas se pretende consultar.</param>
        /// <returns>Uma tarefa assíncrona que retorna uma lista de [ChatRoomDto] com os detalhes das salas.</returns>
        Task<List<ChatRoomDto>> GetMyRoomsAsync(string userId);
    }
}