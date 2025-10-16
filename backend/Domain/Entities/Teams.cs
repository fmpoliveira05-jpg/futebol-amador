using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Exceptions;
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

        [ForeignKey("Pitch")]
        public Guid IdPitch { get; set; } //FK

        public DateTime DataFoundation { get; set; }

        public const int MaxPlayers = 32; //Validar se é mesmo 32

        public const int MaxAdmins = 4;

        public ICollection<Player> Members { get; set; } = new List<Player>();

        [Range(18, 70, ErrorMessage = "A idade média deve estar entre os 18 e 70 anos")]
        public float AverageAge { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int CurrentPoints { get; set; }

        public Rank? Rank { get; set; }

        [ForeignKey("Rank")]
        public Guid? IdRank { get; set; } //FK

        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();

        [InverseProperty("Sender")]
        public ICollection<MatchInvite> SentInvites { get; set; } = new List<MatchInvite>();

        [InverseProperty("Receiver")]
        public ICollection<MatchInvite> ReceivedInvites { get; set; } = new List<MatchInvite>();

        public Calendar Calendar { get; set; }

        [ForeignKey("Calendar")]
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
            AverageAge = 18;
            CurrentPoints = 0;
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

        /*
         Falta metodos da partida Aceitar, Negociar, etc

        Falta:
        - Aceitar
        - Negociar
        - Enviar
        Metodos Implementados
        - Refuse
         */


        /***
         * Método que adiciona um convite de partida enviado pela equipa.
         * 
         * matchInvite: Convite de partida a adicionar.
         * 
         * Retorna o convite de partida adicionado, ou null se a adição não for possível.
         */
        public void SendMatchInvite(MatchInvite matchInvite)
        {
            if (matchInvite == null) {
                throw new ArgumentNullException("O convite de partida não pode ser nulo");
            }

            if (this.SentInvites.Contains(matchInvite)) {
                throw new MatchInviteException("O convite de partida que recebeu já está na lista de convites");
            }

            this.SentInvites.Add(matchInvite);
        }

        public void ReceiveMatchInvite(MatchInvite matchInvite)
        {
            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite de partida não pode ser nulo");
            }

            if (this.ReceivedInvites.Contains(matchInvite))
            {
                throw new MatchInviteException("O convite de partida que recebeu já está na lista de convites");
            }

            this.ReceivedInvites.Add(matchInvite);
        }

        /***
         * Método que remove um convite de partida enviado pela equipa.
         * 
         * matchInvite: Convite de partida a remover.
         * 
         * Retorna o convite de partida removido, ou null se a remoção não for possível.
         * 
         * !!apagar da BD, falta
         * 
         * public MatchInvite RefuseMatchInvite(Guid idMatchInvite)
        {
            
        }
         */
        public bool removeSendMatchInvite(MatchInvite matchInvite)
        {
            bool sucess = true;
            if (matchInvite == null)
            {
                throw new ArgumentNullException("O match Invite está a nulo");
            }

            if (!this.SentInvites.Contains(matchInvite)) {
                throw new Exception("O match Invite enviado não existe");
            }

            this.SentInvites.Remove(matchInvite);
            
            return sucess; 
        }

        public bool removeReceiverMatchInvite(MatchInvite matchInvite)
        {
            bool sucess = true;
            if (matchInvite == null)
            {
                throw new ArgumentNullException("O match Invite está a nulo");
            }

            if (!this.ReceivedInvites.Contains(matchInvite))
            {
                throw new Exception("O match Invite enviado não existe");
            }

            this.ReceivedInvites.Remove(matchInvite);

            return sucess;
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

            if (idMatchInvite == Guid.Empty)
            {
                throw new ArgumentNullException("O id da match não pode ser nulo");
            }

            MatchInvite? matchInvite = this.SentInvites.FirstOrDefault(i => i.Id == idMatchInvite);

            if (matchInvite == null)
            {
                throw new MatchInviteException("A Match invite a eliminar não existe!");
            }

            return matchInvite;
        }

        public MatchInvite ShowReceivedMatchInvite(Guid idMatchInvite)
        {


            if (idMatchInvite == Guid.Empty)
            {
                throw new ArgumentNullException("O id da match não pode ser nulo");
            }

            MatchInvite? matchInvite = this.ReceivedInvites.FirstOrDefault(i => i.Id == idMatchInvite);

            if (matchInvite == null)
            {
                throw new MatchInviteException("A Match invite a eliminar não existe!");
            }

            return matchInvite;
        }

        public override string ToString()
        {
            return $"Team: {Name}, Description: {Description}, Founded: {DataFoundation.ToShortDateString()}, Average Age: {AverageAge}, Current Points: {CurrentPoints}";
        }
    }
}