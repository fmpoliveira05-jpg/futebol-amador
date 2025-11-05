using Application.DTOs.Chat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Validators;
using Google.Cloud.Firestore;

namespace Application.Services
{
    public class FirebaseChatService : IChatRoomService
    {
        private readonly FirestoreDb DbContext;

        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IPlayerValidator PlayerValidator;


        public FirebaseChatService(FirestoreDb firestoreDb, ITeamRepository teamRepository, IPlayerRepository playerRepository)
        {
            DbContext = firestoreDb;
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            PlayerValidator = new PlayerValidator();
        }

        public async Task<string> CreateMatchRoomAsync(CreateChatRoomRequestDto request, string createdByUserId)
        {
            // Criar uma lista de membros única (HashSet evita duplicados)
            var memberIds = new HashSet<string> { createdByUserId };

            foreach (var teamId in request.TeamIds)
            {
                //adicionar verificação se a equipa existe e se os membros existem
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