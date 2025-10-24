using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace Api.Hubs.SupporteEntities
{
    public class FinishMatchLobbyState
    {
        [Required]
        public ConcurrentDictionary<Guid, AdminLobbyInfo> Admins { get; set; } = new();

        public FinishMatchLobbyState() { }
    }
}
