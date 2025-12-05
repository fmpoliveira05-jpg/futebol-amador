using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidade central que representa um jogo ou partida agendada entre equipas.
/// 
/// É a principal entidade de agregação, contendo referências ao estatuto do jogo, local e chat associado.
/// </summary>
namespace Domain.Entities
{
    public class Matches
    {
        /// <summary>
        /// O identificador único (GUID) da Partida.
        /// Serve como a chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// O estado atual da partida (Agendada, A Decorrer, Finalizada, etc.).
        /// Padrão: [MatchStatus.SCHEDULED].
        /// </summary>
        [Required]
        public MatchStatus MatchStatus { get; set; } = MatchStatus.SCHEDULED;

        /// <summary>
        /// Coleção de estatísticas das equipas que participam nesta partida.
        /// (Geralmente haverá 2 entradas nesta coleção).
        /// </summary>
        public ICollection<TeamStatistics> Teams { get; set; }

        /// <summary>
        /// A data e hora planeada para a realização da partida.
        /// </summary>
        [Required]
        public DateTime MatchDate { get; set; }

        /// <summary>
        /// O carimbo de data e hora em que a partida efetivamente começou.
        /// É nulo se a partida ainda não tiver iniciado.
        /// </summary>
        public DateTime? TimeStart { get; set; } = null;

        /// <summary>
        /// Flag que indica se o jogo é competitivo e conta para o ranking/pontuação global (true)
        /// ou se é apenas amigável/casual (false).
        /// </summary>
        [Required]
        public bool IsCompetive { get; set; }

        /// <summary>
        /// Entidade de navegação para o local onde a partida será disputada.
        /// </summary>
        public Pitch Pitch { get; set; }

        /// <summary>
        /// Chave Estrangeira (FK) para o Local ([Pitch]) da partida.
        /// </summary>
        [Required]
        [ForeignKey("Pitch")]
        public Guid idPitch { get; set; }

        /// <summary>
        /// Entidade de navegação para o Chat associado a esta partida.
        /// </summary>
        public Chat Chat { get; set; }

        /// <summary>
        /// Chave Estrangeira (FK) para a Sala de Chat ([Chat]) da partida.
        /// </summary>
        [Required]
        [ForeignKey("Chat")]
        public Guid IdChat { get; set; }

        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public Matches() { }

        /// <summary>
        /// Construtor para inicializar a partida utilizando o ID do campo (FK) em vez da entidade completa.
        /// </summary>
        /// <param name="matchDate">Data e hora do jogo.</param>
        /// <param name="isCompetive">Indica se o jogo é competitivo.</param>
        /// <param name="idPitch">O ID da Chave Estrangeira do campo.</param>
        /// <param name="teamStatistics">A lista de estatísticas das equipas participantes.</param>
        /// <param name="chat">O objeto Chat pré-existente.</param>
        public Matches(DateTime matchDate, bool isCompetive, Guid idPitch, List<TeamStatistics> teamStatistics, Chat chat)
        {
            this.MatchDate = matchDate;
            this.IsCompetive = isCompetive;
            this.idPitch = idPitch;
            this.Teams = teamStatistics;

            if (Chat != null)
            {
                this.Chat = chat;
                this.IdChat = idPitch;
            }
            else
            {
                this.Chat = new Chat();
                this.IdChat = this.Chat.Id;
            }
        }

        /// <summary>
        /// Construtor para inicializar a partida usando apenas o ID do campo e sem a entidade Chat pré-existente.
        /// </summary>
        /// <param name="matchDate">Data e hora do jogo.</param>
        /// <param name="isCompetive">Indica se o jogo é competitivo.</param>
        /// <param name="idPitch">O ID da Chave Estrangeira do campo.</param>
        /// <param name="teamStatistics">A lista de estatísticas das equipas participantes.</param>
        public Matches(DateTime matchDate, bool isCompetive, Guid idPitch, List<TeamStatistics> teamStatistics)
        {
            this.MatchDate = matchDate;
            this.IsCompetive = isCompetive;
            this.idPitch = idPitch;
            this.Teams = teamStatistics;

            if (Chat != null)
            {
                this.Chat = new Chat();
                this.IdChat = idPitch;
            }
            else
            {
                this.Chat = new Chat();
                this.IdChat = this.Chat.Id;
            }
        }
    }
}
