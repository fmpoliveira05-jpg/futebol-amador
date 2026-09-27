using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Chat
{
    /// <summary>
    /// Pedido de criação de uma sala de chat (<c>POST /api/Chat/create-room</c>).
    /// </summary>
    /// <remarks>
    /// Quem cria a sala entra sempre nela; os outros membros têm de ser colegas da mesma equipa
    /// (ver <see cref="Services.FirebaseChatService.CreateRoomAsync"/>).
    /// </remarks>
    public class CreateChatRoomDto
    {
        [Required]
        [Length(1, ModelConstants.ChatConst.MaxRoomNameLength)]
        public string RoomName { get; set; } = null!;

        /// <summary>Ids (Firebase) dos outros membros, sem contar com quem cria a sala.</summary>
        [Required]
        [Length(1, ModelConstants.ChatConst.MaxMembers - 1)]
        public List<string> MemberIds { get; set; } = null!;
    }
}
