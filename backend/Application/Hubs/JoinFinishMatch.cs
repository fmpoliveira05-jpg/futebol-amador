using Application.DTOs;

namespace Application.Hubs
{
    public class JoinFinishMatch
    {
        public bool IsFirstAdmin { get; set; }
        public bool MatchFinish { get; set; } // A true quer dizer que já temos os dois admins e a match vai ser iniciada
        public Guid IdTeam { get; set; }
        public FinishMatchDTO? ResultMatch { get; set; }
        public string? FirstAdminConnectionId { get; set; } //Guardar Connection string do 1º admin
        public bool? IsCoincides { get; set; }
    }
}
