using Application.DTOs.Chat;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Google.Api;
using Google.Cloud.Firestore;

namespace Application.Services
{
    public class FirebaseChatService : IChatRoomService
    {
        private readonly FirestoreDb DbContext;

        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;

        public FirebaseChatService(FirestoreDb firestoreDb, ITeamRepository teamRepository, IPlayerRepository playerRepository)
        {
            DbContext = firestoreDb;
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
        }

        public async Task<string> CreateMatchRoomAsync(CreateChatRoomRequestDto request, string createdByUserId)
        {
            // Criar uma lista de membros única (HashSet evita duplicados)
            var memberIds = new HashSet<string> { createdByUserId };

            // 2. Ir à sua base de dados principal buscar os membros das equipas
            foreach (var teamId in request.TeamIds)
            {
                //Adiciona os admins das equipas como membros da sala
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
            var playersIds = await PlayerRepository.GetPlayersListByIdListAsync(request.ParticipantIds);
            //Quando for fazer as validações validar se todos os ids no request estão nos playersIds retornados, caso não estejam
            //O codigo corre a mesma e cria o chat, mas não adiciona os ids inválidos e no fim retorna uma lista dos ids inválidos
            foreach (var participantId in request.ParticipantIds)
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
    }
}