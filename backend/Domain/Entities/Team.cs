using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma equipa desportiva no sistema.
    /// 
    /// Esta entidade é o ponto central de agregação para membros, estatísticas, ranking e agenda de jogos.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// O identificador único (GUID) da Equipa.
        /// Serve como chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// O nome da equipa.
        /// </summary>
        /// <value>Obrigatório. O comprimento máximo da string é definido em [ModelConstants.TeamConst.MaxNameLength].</value>
        [MaxLength(ModelConstants.TeamConst.MaxNameLength), Required(ErrorMessage = "O nome do time é obrigatorio.")]
        public string Name { get; set; }

        /// <summary>
        /// Uma breve descrição da equipa (ex: histórico, estilo de jogo).
        /// </summary>
        [MaxLength(ModelConstants.TeamConst.MaxDescriptionLength)]
        public string? Description { get; set; }

        /// <summary>
        /// O logótipo ou imagem de perfil da equipa (armazenado como um array de bytes).
        /// </summary>
        public byte[]? Icon { get; set; }

        /// <summary>
        /// Entidade de navegação para o Campo ([Pitch]) associado à equipa (campo da casa).
        /// </summary>
        public Pitch Pitch { get; set; }

        /// <summary>
        /// Chave Estrangeira (FK) para o Campo ([Pitch]) da equipa.
        /// </summary>
        [ForeignKey("Pitch")]
        public Guid IdPitch { get; set; } //FK

        /// <summary>
        /// A data em que a equipa foi criada ou fundada no sistema.
        /// </summary>
        public DateTime DataFoundation { get; set; }

        /// <summary>
        /// Coleção de jogadores que são membros desta equipa.
        /// Relação One-to-Many.
        /// </summary>
        public ICollection<Player> Members { get; set; } = new List<Player>();

        /// <summary>
        /// O total de pontos de ranking atuais da equipa.
        /// </summary>
        /// <value>O valor é não negativo (0 ou superior).</value>
        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int CurrentPoints { get; set; }

        /// <summary>
        /// Entidade de navegação para o nível de classificação ([Rank]) atual da equipa.
        /// </summary>
        public Rank Rank { get; set; }

        /// <summary>
        /// Chave Estrangeira (FK) para o Rank atual ([Rank]) da equipa.
        /// </summary>
        [ForeignKey("Rank")]
        public Guid IdRank { get; set; } //FK

        /// <summary>
        /// Coleção de todos os pedidos de adesão ([MembershipRequest]) que a equipa enviou ou recebeu.
        /// </summary>
        public ICollection<MembershipRequest> MembershipRequests { get; set; } = new List<MembershipRequest>();

        /// <summary>
        /// Coleção de convites de partida que a equipa **enviou** (Atua como remetente).
        /// </summary>
        [InverseProperty("Sender")]
        public ICollection<MatchInvite> SentInvites { get; set; } = new List<MatchInvite>();

        /// <summary>
        /// Coleção de convites de partida que a equipa **recebeu** (Atua como recetor).
        /// </summary>
        [InverseProperty("Receiver")]
        public ICollection<MatchInvite> ReceivedInvites { get; set; } = new List<MatchInvite>();

        /// <summary>
        /// Entidade de navegação para o Calendário ([Calendar]) da equipa.
        /// </summary>
        public Calendar Calendar { get; set; }

        /// <summary>
        /// Chave Estrangeira (FK) para o Calendário ([Calendar]) da equipa.
        /// </summary>
        [ForeignKey("Calendar")]
        public Guid IdCalendar { get; set; } //FK

        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public Team() { }

        /// <summary>
        /// Construtor para inicializar uma nova equipa com as entidades obrigatórias.
        /// </summary>
        /// <param name="name">O nome da equipa (obrigatório).</param>
        /// <param name="description">A descrição (opcional).</param>
        /// <param name="icon">O logótipo (opcional).</param>
        /// <param name="pitch">O campo de jogo associado.</param>
        /// <param name="defaultRank">O Rank inicial da equipa.</param>
        public Team(string name, string? description, byte[]? icon, Pitch pitch, Rank defaultRank)
        {
            this.Name = name;
            this.Description = description;
            this.Icon = icon;
            this.Pitch = pitch;
            this.IdPitch = pitch.Id;
            this.DataFoundation = DateTime.Now;
            this.Rank = defaultRank;
            this.IdRank = defaultRank.Id;
            this.CurrentPoints = 0;
            this.Calendar = new Calendar();
            this.IdCalendar = Calendar.Id;
            this.Members = new List<Player>();
            this.MembershipRequests = new List<MembershipRequest>();
            this.SentInvites = new List<MatchInvite>();
            this.ReceivedInvites = new List<MatchInvite>();
        }
    }
}