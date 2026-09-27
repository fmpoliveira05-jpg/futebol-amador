using Application.DTOs.Chat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Exceptions;
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
        /// Regras (antes aceitava quaisquer ids, e qualquer pessoa podia ser metida numa sala):
        /// <list type="bullet">
        /// <item>quem cria a sala entra sempre nela e tem de pertencer a uma equipa;</item>
        /// <item>os outros membros têm de existir e ser colegas da mesma equipa;</item>
        /// <item>no máximo <see cref="ModelConstants.ChatConst.MaxMembers"/> membros e nome com até
        /// <see cref="ModelConstants.ChatConst.MaxRoomNameLength"/> caracteres.</item>
        /// </list>
        /// As salas dos jogos (administradores das duas equipas) são criadas pelo servidor em
        /// <see cref="CreateMatchRoomAsync"/>.
        /// </remarks>
        /// <param name="request">DTO com o nome da sala e lista de IDs de jogadores.</param>
        /// <param name="createdByUserId">ID do utilizador criador.</param>
        /// <returns>O ID (string) da sala criada.</returns>
        /// <exception cref="ValidationException">Nome ou lista de membros inválidos.</exception>
        /// <exception cref="ForbiddenException">Algum membro não é colega de equipa de quem cria a sala.</exception>
        public async Task<string> CreateRoomAsync(CreateChatRoomDto request, string createdByUserId)
        {
            var memberIds = await ValidarMembrosAsync(request, createdByUserId);

            var roomData = new Dictionary<string, object>
            {
                { "name", request.RoomName.Trim() },
                { "createdBy", createdByUserId },
                { "members", memberIds.ToList() },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            DocumentReference roomRef = await DbContext.Collection("chatRooms").AddAsync(roomData);
            return roomRef.Id;
        }

        /// <summary>
        /// Aplica as regras de <see cref="CreateRoomAsync"/> e devolve os membros finais da sala
        /// (sem repetidos e com quem a cria). Não toca no Firestore.
        /// </summary>
        internal async Task<List<string>> ValidarMembrosAsync(CreateChatRoomDto request, string createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(request.RoomName) ||
                request.RoomName.Trim().Length > ModelConstants.ChatConst.MaxRoomNameLength)
            {
                throw new ValidationException("O nome da sala é obrigatório e tem no máximo 80 caracteres.");
            }

            var outros = (request.MemberIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Where(id => id != createdByUserId)
                .Distinct()
                .ToList();

            if (outros.Count == 0 || outros.Count > ModelConstants.ChatConst.MaxMembers - 1 ||
                outros.Any(id => id.Length > ModelConstants.UserConst.MaxIdLength))
            {
                throw new ValidationException("Indica entre 1 e 39 membros para a sala.");
            }

            var criador = await PlayerRepository.GetPlayerByIdAsync(createdByUserId);
            if (criador?.IdTeam == null)
            {
                throw new ForbiddenException("Só quem pertence a uma equipa pode criar salas de chat.");
            }

            var jogadores = await PlayerRepository.GetPlayersListByIdListAsync(outros);
            var colegas = jogadores.Where(p => p.IdTeam == criador.IdTeam).Select(p => p.Id).ToHashSet();

            // A mesma resposta para ids inexistentes e para jogadores de outras equipas.
            if (outros.Any(id => !colegas.Contains(id)))
            {
                throw new ForbiddenException("Só podes criar salas com colegas da tua equipa.");
            }

            return outros.Prepend(createdByUserId).ToList();
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
    
        /// <inheritdoc />
        public async Task<List<Application.DTOs.MensagemExportadaDto>> ExportarMensagensAsync(string userId)
        {
            // Consulta de grupo de coleções: precisa da isenção de índice de campo único em
            // messages.senderId com âmbito "grupo de coleções" (ver docs/RGPD.md).
            var mensagens = await DbContext.CollectionGroup("messages").WhereEqualTo("senderId", userId).GetSnapshotAsync();

            return mensagens.Documents.Select(d => new Application.DTOs.MensagemExportadaDto
            {
                Sala = d.Reference.Parent.Parent?.Id ?? "",
                Texto = d.TryGetValue<string>("text", out var texto) ? texto : "",
                EnviadaEm = d.TryGetValue<Timestamp>("timestamp", out var hora) ? hora.ToDateTime() : null,
            }).ToList();
        }

        /// <inheritdoc />
        public async Task EliminarDadosUtilizadorAsync(string userId)
        {
            var mensagens = await DbContext.CollectionGroup("messages").WhereEqualTo("senderId", userId).GetSnapshotAsync();
            foreach (var bloco in mensagens.Documents.Chunk(400))
            {
                var lote = DbContext.StartBatch();
                foreach (var mensagem in bloco)
                {
                    lote.Delete(mensagem.Reference);
                }
                await lote.CommitAsync();
            }

            var salas = await DbContext.Collection("chatRooms").WhereArrayContains("members", userId).GetSnapshotAsync();
            foreach (var bloco in salas.Documents.Chunk(400))
            {
                var lote = DbContext.StartBatch();
                foreach (var sala in bloco)
                {
                    lote.Update(sala.Reference, "members", FieldValue.ArrayRemove(userId));
                    if (sala.TryGetValue<string>("createdBy", out var criador) && criador == userId)
                    {
                        lote.Update(sala.Reference, "createdBy", "removido");
                    }
                }
                await lote.CommitAsync();
            }
        }
    }
}