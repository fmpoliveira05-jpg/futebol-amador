using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidade que representa um pedido de adesão (Membership Request) entre um jogador e uma equipa.
/// 
/// Esta entidade é bidirecional, podendo ser um jogador a pedir para entrar (Join Request)
/// ou uma equipa a convidar um jogador (Recruitment Invite).
/// </summary>
namespace Domain.Entities
{
    public class MembershipRequest
    {
        /// <summary>
        /// O identificador único (GUID) do pedido de adesão.
        /// Serve como a chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Entidade de navegação para o Jogador envolvido no pedido.
        /// </summary>
        public Player Player { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para o Jogador.
        /// O comprimento máximo da string é definido em [ModelConstants.UserConst.MaxIdLength].
        /// </summary>
        [Required]
        [ForeignKey("Player")]
        [MaxLength(ModelConstants.UserConst.MaxIdLength)]
        public string IdPlayer { get; set; } //FK

        /// <summary>
        /// Entidade de navegação para a Equipa envolvida no pedido.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Equipa.
        /// </summary>
        [Required]
        [ForeignKey("Team")]
        public Guid IdTeam { get; set; } //FK

        /// <summary>
        /// O carimbo de data e hora em que o pedido/convite foi enviado.
        /// </summary>
        [Required]
        public DateTime InviteDate { get; set; }

        /// <summary>
        /// Flag booleana que define a direção do pedido.
        /// - <c>true</c>: O Jogador enviou o pedido (Join Request).
        /// - <c>false</c>: A Equipa enviou o pedido (Recruitment Invite).
        /// </summary>
        [Required]
        public bool IsPlayerSender { get; set; }
       
        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public MembershipRequest() { }

        /// <summary>
        /// Construtor utilizado para inicializar um novo pedido de adesão.
        /// </summary>
        /// <param name="player">O jogador envolvido no pedido.</param>
        /// <param name="team">A equipa envolvida no pedido.</param>
        /// <param name="sender">O valor booleano que indica se o jogador (<c>true</c>) ou a equipa (<c>false</c>) é o remetente.</param>
        public MembershipRequest(Player player, Team team, bool sender)
        {
            Player = player;
            IdPlayer = player.Id;
            Team = team;
            IdTeam = team.Id;
            InviteDate = DateTime.Now;
            IsPlayerSender = sender;
        }
    }
}
