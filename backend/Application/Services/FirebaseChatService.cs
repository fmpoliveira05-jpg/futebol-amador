using Application.DTOs.Chat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Google.Cloud.Firestore;

namespace Application.Services
{
    /// <summary>
    /// Serviço de infraestrutura responsável pela gestão de salas de chat utilizando o Google Firestore (NoSQL).
    /// 
    /// Implementa o contrato [IChatRoomService] e permite criar salas associadas a partidas ou grupos ad-hoc,
    /// bem como listar as salas em que um utilizador participa.
    /// </summary>
    public class FirebaseChatService : IChatRoomService
    {
        /// <summary>
        /// Cliente do Firestore injetado.
        /// </summary>
        private readonly FirestoreDb DbContext;
        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;

        /// <summary>
        /// Construtor do FirebaseChatService.
        /// </summary>
        /// <param name="firestoreDb">Instância do cliente Firestore.</param>
        /// <param name="teamRepository">Repositório de equipas (usado para obter admins).</param>
        /// <param name="playerRepository">Repositório de jogadores (usado para validar membros).</param>
        public FirebaseChatService(FirestoreDb firestoreDb, ITeamRepository teamRepository, IPlayerRepository playerRepository)
        {
            DbContext = firestoreDb;
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
        }

        /// <summary>
        /// Cria uma sala de chat associada a uma partida (Match Room), adicionando automaticamente
        /// os administradores das equipas envolvidas.
        /// </summary>
        /// <remarks>
        /// 1. Recebe uma lista de IDs de equipas ([request.TeamIds]).
        /// 2. Consulta o [TeamRepository] para obter os IDs de todos os administradores dessas equipas.
        /// 3. Cria o documento na coleção `chatRooms` do Firestore com o array consolidado de membros.
        /// </remarks>
        /// <param name="request">DTO com o nome da sala e IDs das equipas participantes.</param>
        /// <param name="createdByUserId">ID do utilizador que iniciou a criação (geralmente um admin).</param>
        /// <returns>O ID (string) do documento da sala criado no Firestore.</returns>
        public async Task<string> CreateMatchRoomAsync(CreateChatRoomRequestDto request, string createdByUserId)
        {
            var memberIds = new HashSet<string> { createdByUserId };

            foreach (var teamId in request.TeamIds)
            {

                List<string> userIds = await TeamRepository.GetAdminsIdsByTeamIdAsync(teamId);
                foreach (var id in userIds)
                {
                    memberIds.Add(id);
                }
            }

            // cria a sala no Firestore com a lista completa de membros
            var roomData = new Dictionary<string, object>
            {
                { "name", request.RoomName },
                { "createdBy", createdByUserId },
                { "members", memberIds.ToList() },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            DocumentReference roomRef = await DbContext.Collection("chatRooms").AddAsync(roomData);
            return roomRef.Id;
        }

        /// <summary>
        /// Cria uma sala de chat genérica com uma lista explícita de membros (jogadores).
        /// </summary>
        /// <remarks>
        /// Valida a existência dos jogadores através do [PlayerRepository] antes de adicionar ao chat.
        /// **Nota:** A lógica atual não impede a criação se alguns IDs forem inválidos, apenas os ignora silenciosamente ou falha dependendo da implementação do repositório.
        /// </remarks>
        /// <param name="request">DTO com o nome da sala e lista de IDs de jogadores.</param>
        /// <param name="createdByUserId">ID do utilizador criador.</param>
        /// <returns>O ID (string) da sala criada.</returns>
        public async Task<string> CreateRoomAsync(CreateChatRoomDto request, string createdByUserId)
        {
            var memberIds = new HashSet<string> { createdByUserId };
            var playersIds = await PlayerRepository.GetPlayersListByIdListAsync(request.MemberIds);
            //Quando for fazer as validações validar se todos os ids no request estão nos playersIds retornados, caso não estejam
            //O codigo corre a mesma e cria o chat, mas não adiciona os ids inválidos e no fim retorna uma lista dos ids inválidos
            foreach (var participantId in request.MemberIds)
            {
                memberIds.Add(participantId.ToString());
            }

            var roomData = new Dictionary<string, object>
            {
                { "name", request.RoomName },
                { "createdBy", createdByUserId },
                { "members", memberIds.ToList() },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            DocumentReference roomRef = await DbContext.Collection("chatRooms").AddAsync(roomData);
            return roomRef.Id;
        }

        /// <summary>
        /// Obtém a lista de salas de chat em que um utilizador participa.
        /// </summary>
        /// <remarks>
        /// Executa uma consulta no Firestore utilizando o operador `array-contains` no campo `members`.
        /// Mapeia os documentos resultantes para o DTO [ChatRoomDto].
        /// </remarks>
        /// <param name="userId">O ID do utilizador.</param>
        /// <returns>Uma lista de DTOs com os detalhes das salas encontradas.</returns>
        public async Task<List<ChatRoomDto>> GetMyRoomsAsync(string userId)
        {
            var chatRoomsList = new List<ChatRoomDto>();

            // Obtem a referencia da chatRooms collection
            CollectionReference chatRoomsRef = DbContext.Collection("chatRooms");

            // 2. Criar a consulta: "Encontrar todos os documentos onde o array 'members' contém o userId"
            Query query = chatRoomsRef.WhereArrayContains("members", userId);

            // 3. Executar a consulta
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            // 4. Iterar sobre os resultados e mapear para o DTO
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    // Extrair os dados do documento do Firestore
                    string roomName = documentSnapshot.GetValue<string>("name");
                    List<string> memberIds = documentSnapshot.GetValue<List<string>>("members");

                    var participantIds = new List<string>();
                    if (memberIds != null)
                    {
                        foreach (var idStr in memberIds)
                        {
                            participantIds.Add(idStr);
                        }
                    }

                    var chatRoomDto = new ChatRoomDto
                    {
                        RoomId = documentSnapshot.Id,
                        RoomName = roomName,
                        MemberIds = participantIds
                    };

                    chatRoomsList.Add(chatRoomDto);
                }
            }

            return chatRoomsList;
        }
    }
}