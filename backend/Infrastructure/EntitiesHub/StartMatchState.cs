using Application.DTOs.Hub;
using System.Collections.Concurrent;

namespace Infrastructure.EntitiesHub
{
    public class StartMatchState
    {
        //Guarda os Admins que entraram
        public ConcurrentDictionary<string, AdminsJoinMatchDTO> Admins { get; } = new();

        public HashSet<Guid> ReadyAdmins { get; } = new();
    }
}
