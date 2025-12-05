using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa um jogador no sistema.
    /// 
    /// Esta entidade herda de [User] e adiciona atributos específicos do contexto desportivo,
    /// como a Posição, Altura e a afiliação à equipa.
    /// </summary>
    public class Player : User
    {
        /// <summary>
        /// A posição principal do jogador em campo.
        /// </summary>
        [Required]
        public Position Position { get; set; }

        /// <summary>
        /// A altura do jogador em centímetros.
        /// </summary>
        /// <value>O valor deve estar dentro do intervalo definido por [ModelConstants.PlayerConst.MinHeight] e [ModelConstants.PlayerConst.MaxHeight].</value>
        [Required]
        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight, ErrorMessage = "Um jogador deve ter entre {0} e {1} centimetors")]
        public int Height { get; set; }

        /// <summary>
        /// Entidade de navegação para a equipa à qual o jogador pertence atualmente.
        /// É nulo se o jogador for um agente livre (sem equipa).
        /// </summary>
        public Team? Team { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Equipa.
        /// É nula se o jogador não tiver equipa.
        /// </summary>
        [ForeignKey("Team")]
        public Guid? IdTeam { get; set; }

        /// <summary>
        /// Flag que indica se o jogador possui privilégios de administrador (Capitão/Staff de gestão) na sua equipa atual.
        /// </summary>
        [Required]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Carimbo de data e hora da última vez que o estatuto de administração foi alterado (promovido/despromovido).
        /// É nulo se nunca foi alterado.
        /// </summary>
        public DateTime? IsAdminLastChangedAt { get; set; }

        /// <summary>
        /// Coleção de pedidos de adesão (Membership Requests) que este jogador enviou ou recebeu.
        /// </summary>
        /// <value>O número de elementos na coleção deve ser não negativo.</value>
        [Range(0, int.MaxValue, ErrorMessage = "O número de convites tem de ser pelo menos 0")]
        public ICollection<MembershipRequest> MembershipRequests { get; set; } = new List<MembershipRequest>();

        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public Player() { }

        /// <summary>
        /// Construtor utilizado para inicializar um novo perfil de Jogador.
        /// </summary>
        /// <param name="userId">O ID único do utilizador (herdado de User/Firebase UID).</param>
        /// <param name="name">O nome do jogador.</param>
        /// <param name="dateOfBirth">A data de nascimento.</param>
        /// <param name="address">A morada/localidade.</param>
        /// <param name="email">O email de contacto.</param>
        /// <param name="phoneNumber">O número de telefone.</param>
        /// <param name="position">A posição principal em campo.</param>
        /// <param name="height">A altura em centímetros.</param>
        public Player(string userId, string name, DateOnly dateOfBirth, string address, string email, string phoneNumber, Position position, int height)
            : base(userId, name, dateOfBirth, address, email, phoneNumber)
        {
            Position = position;
            Height = height;
            IsAdmin = false;
        }
    }
}
