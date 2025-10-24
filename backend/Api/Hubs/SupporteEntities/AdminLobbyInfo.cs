using Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Api.Hubs.SupporteEntities
{
    public class AdminLobbyInfo
    {
        [Required]
        public string ConnectionId { get; set; }
        [Required]
        public Guid IdAdmin { get; set; }
        [Required]
        public Guid IdTeam { get; set; }
        [Required]
        public ResultMatchDto Result { get; set; }

        public AdminLobbyInfo(string connectionId, Guid idAdmin, Guid idTeam, ResultMatchDto result)
        {
            this.ConnectionId = connectionId;
            this.IdAdmin = idAdmin;
            this.IdTeam = idTeam;
            this.Result = result;
        }
    }
}
