using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/***
 * Entidade que representa uma equipa desportiva.
 */
namespace Domain.Entities
{
    public class Teams
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public byte[]? Icon { get; set; }

        public Pitch Pitch { get; set; }

        public Guid IdPitch { get; set; } //FK

        public DateTime DataFoundation { get; set; }

        public const int MaxPlayers = 32; //Validar se é mesmo 32

        public const int MaxAdmins = 4;

        [Range(1, MaxPlayers, ErrorMessage = "O número minimo de players é 1 de máximo 32")]
        public int MemberCount { get; set; }

        [Range(1, MaxAdmins, ErrorMessage = "O número minimo de admins é 1 de máximo 4")]
        public int AdminCount { get; set; }

        public ICollection<Player> Members { get; set; } = new List<Player>();

        [Range(18, 70, ErrorMessage = "A idade média deve estar entre os 18 e 70 anos")]
        public float AverageAge { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int CurrentPoints { get; set; }

        public Rank? Rank { get; set; }

        public Guid? IdRank { get; set; } //FK

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou pedidos de adesão")]
        public int CountMemvberShipsRequests { get; set; }

        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais convites de partida enviados")]
        public int CountSendInvites { get; set; }

        [InverseProperty("Sender")]
        public ICollection<MatchInvite> SentInvites { get; set; } = new List<MatchInvite>();

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou recevidos pedidos de partidas recebidos")]
        public int CountReceivedIntes { get; set; }
        [InverseProperty("Receiver")]
        public ICollection<MatchInvite> ReceivedInvites { get; set; } = new List<MatchInvite>();

        public Calendar Calendar { get; set; }

        public Guid IdCalendar { get; set; } //FK

        //EF
        protected Teams() { }

        public Teams(string name, string? description, byte[]? icon, Pitch pitch)
        {
            Name = name;
            Description = description;
            Icon = icon;
            Pitch = pitch;
            IdPitch = pitch.Id;
            DataFoundation = DateTime.Now;
            MemberCount = 1;
            AdminCount = 1;
            AverageAge = 18;
            CurrentPoints = 0;
            CountMemvberShipsRequests = 0;
            CountSendInvites = 0;
            CountReceivedIntes = 0;
            Calendar = new Calendar();
            IdCalendar = Calendar.Id;
        }

        /**
         * Metodo responsavel por atualizar a idade média da equipa.
         */
        private void UpdateAverageAge()
        {

        }

        /**(
         * Metodo que procura um jogador na equipa pelo seu ID
         * 
         * idPlayer: ID do jogador a procurar.
         * 
         * Retorna o jogador se encontrado, ou null se não encontrado.
         */
        private Player FindPlayer(Guid idPlayer)
        {
            return null;
        }

        /**
         * Metodo que procura um admin na equipa pelo seu ID
         * 
         * idAdminTeam: ID do admin a procurar.
         * 
         * Retorna o admin se encontrado, ou null se não encontrado.
         */
        private Player FindAdminTeam(Guid idAdminTeam)
        {
            return null;
        }

        /**
         * Metodo que adiciona um jogador à equipa.
         * 
         * Player: Jogador a adicionar.
         */
        public Player AddPlayer(Player player)
        {
            return null;
        }

        public Player RemovePlayer(Player player)
        {
            return null;
        }

        /**
         * Método que promove um jogador a admin da equipa.
         * 
         * Player: Jogador a promover.
         * 
         * Retorna o jogador promovido como admin, ou null se a promoção não for possível.
         */
        public Player PromoteToAdmin(Player player)
        {
            return null;
        }

        /**
         * Método que rebaixa um admin a jogador comum.
         * 
         * Player: Admin a rebaixar.
         * 
         * Retorna o jogador rebaixado, ou null se a rebaixa não for possível.
         */
        public Player DemoteFromAdmin(Player player)
        {
            return null;

        }

        /*** Método que adiciona um pedido de adesão à equipa.
         * 
         * membershipRequest: Pedido de adesão a adicionar.
         * 
         * Retorna o pedido de adesão adicionado, ou null se a adição não for possível.
         */
        public MembershipRequests AddMembershipRequest(MembershipRequests membershipRequest)
        {
            return null;
        }

        /*** Método que remove um pedido de adesão da equipa.
         * 
         * membershipRequest: Pedido de adesão a remover.
         * 
         * Retorna o pedido de adesão removido, ou null se a remoção não for possível.
         */
        public MembershipRequests RemoveMembershipRequest(MembershipRequests membershipRequest) 
        {
            return null;
        }

        /***
         * Método que mostra um pedido de adesão específico da equipa.
         * 
         * idMembershipRequest: ID do pedido de adesão a mostrar.
         * 
         * Retorna o pedido de adesão se encontrado, ou null se não encontrado.
         */
        public MembershipRequests ShowMembershipRequest(Guid idMembershipRequest) { 
            return null;
        }

        /***
         * Método que adiciona um convite de partida enviado pela equipa.
         * 
         * matchInvite: Convite de partida a adicionar.
         * 
         * Retorna o convite de partida adicionado, ou null se a adição não for possível.
         */
        public MatchInvite AddSendMatchInvite(MatchInvite matchInvite)
        {
            return null;
        }

        /***
         * Método que remove um convite de partida enviado pela equipa.
         * 
         * matchInvite: Convite de partida a remover.
         * 
         * Retorna o convite de partida removido, ou null se a remoção não for possível.
         */
        public MatchInvite RemoveSendMatchInvite(MatchInvite matchInvite)
        {
            return null;
        }

        /***
         * Método que mostra um convite de partida enviado específico da equipa.
         * 
         * idMatchInvite: ID do convite de partida a mostrar.
         * 
         * Retorna o convite de partida se encontrado, ou null se não encontrado.
         */
        public MatchInvite ShowSendMatchInvite(Guid idMatchInvite)
        {
            return null;
        }

        /***
         * Método que adiciona um convite de partida recebido pela equipa.
         * 
         * matchInvite: Convite de partida a adicionar.
         * 
         * Retorna o convite de partida adicionado, ou null se a adição não for possível.
         */
        public MatchInvite AddReceivedMatchInvite(MatchInvite matchInvite) 
        {
            return null;
        }

        /***
         * Método que remove um convite de partida recebido pela equipa.
         * 
         * matchInvite: Convite de partida a remover.
         * 
         * Retorna o convite de partida removido, ou null se a remoção não for possível.
         */
        public MatchInvite RemoveReceivedMatchInvite(MatchInvite matchInvite) 
        {
            return null;
        }

        /***
         * Método que mostra um convite de partida recebido específico da equipa.
         * 
         * idMatchInvite: ID do convite de partida a mostrar.
         * 
         * Retorna o convite de partida se encontrado, ou null se não encontrado.
         */
        public MatchInvite ShowReceivedMatchInvite(Guid idMatchInvite) 
        {
            return null;
        }

        public override string ToString()
        {
            return $"Team: {Name}, Description: {Description}, Founded: {DataFoundation.ToShortDateString()}, Members: {MemberCount}, Admins: {AdminCount}, Average Age: {AverageAge}, Current Points: {CurrentPoints}";
        }
    }
}